using SmartUninstaller.Core.Models;
using SmartUninstaller.Core.Utils;
using SmartUninstaller.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace SmartUninstaller.Core.Engines;

/// <summary>
/// 卸载引擎，负责执行软件卸载的核心逻辑
/// </summary>
public class UninstallEngine
{
    private readonly ILogger<UninstallEngine> _logger;
    private readonly RegistryHelper _registryHelper;
    private readonly FileHelper _fileHelper;
    private readonly ProcessHelper _processHelper;

    public UninstallEngine(
        ILogger<UninstallEngine> logger,
        RegistryHelper registryHelper,
        FileHelper fileHelper,
        ProcessHelper processHelper)
    {
        _logger = logger;
        _registryHelper = registryHelper;
        _fileHelper = fileHelper;
        _processHelper = processHelper;
    }

    /// <summary>
    /// 执行卸载操作
    /// </summary>
    public virtual async Task<UninstallResult> ExecuteUninstallAsync(SoftwareInfo software, UninstallOptions options)
    {
        var result = new UninstallResult
        {
            Software = software,
            StartTime = DateTime.Now
        };

        try
        {
            _logger.LogInformation("开始卸载软件: {Name}", software.Name);

            // 1. 关闭运行中的进程
            if (software.IsRunning && software.ProcessId.HasValue)
            {
                _logger.LogInformation("软件正在运行，尝试关闭进程: {ProcessId}", software.ProcessId);
                try
                {
                    await _processHelper.KillProcessAsync(software.ProcessId.Value);
                    result.Logs.Add($"已关闭进程 PID={software.ProcessId}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "关闭进程失败");
                    result.Logs.Add($"关闭进程失败: {ex.Message}");
                    if (!options.ForceUninstall)
                    {
                        result.Success = false;
                        result.ErrorMessage = "无法关闭正在运行的进程";
                        result.EndTime = DateTime.Now;
                        return result;
                    }
                }
            }

            // 2. 创建系统还原点
            if (options.CreateRestorePoint)
            {
                _logger.LogInformation("创建系统还原点...");
                try
                {
                    await CreateSystemRestorePointAsync(software.Name);
                    result.Logs.Add("系统还原点创建成功");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "创建系统还原点失败（非致命）");
                    result.Logs.Add($"创建系统还原点失败: {ex.Message}");
                }
            }

            // 3. 执行卸载命令
            if (!string.IsNullOrEmpty(software.UninstallString))
            {
                _logger.LogInformation("执行卸载命令: {Command}", software.UninstallString);
                result.Logs.Add($"执行卸载命令: {software.UninstallString}");
                var uninstallSuccess = await ExecuteUninstallCommandAsync(software.UninstallString, options.SilentUninstall);

                if (!uninstallSuccess)
                {
                    result.Logs.Add("卸载命令返回非零退出码");
                    if (!options.ForceUninstall)
                    {
                        result.Success = false;
                        result.ErrorMessage = "卸载命令执行失败";
                        result.EndTime = DateTime.Now;
                        return result;
                    }
                    result.Logs.Add("强制卸载模式，继续清理...");
                }
                else
                {
                    result.Logs.Add("卸载命令执行成功");
                }
            }
            else if (!options.ForceUninstall && !software.IsPortable)
            {
                // 没有卸载命令且不是便携软件，无法正常卸载
                result.Success = false;
                result.ErrorMessage = "该软件没有卸载命令，请使用强制卸载模式";
                result.EndTime = DateTime.Now;
                return result;
            }

            // 4. 深度清理残留
            if (options.DeepClean || options.ForceUninstall)
            {
                _logger.LogInformation("执行深度清理...");
                result.Logs.Add("开始深度清理...");
                var cleanupResult = await DeepCleanAsync(software);
                result.FilesDeleted += cleanupResult.FilesDeleted;
                result.RegistryEntriesDeleted += cleanupResult.RegistryEntriesDeleted;
                result.SpaceFreed += cleanupResult.SpaceFreed;
                result.Logs.Add($"深度清理完成: 删除{cleanupResult.FilesDeleted}个文件, {cleanupResult.RegistryEntriesDeleted}个注册表项, 释放{cleanupResult.SpaceFreed}字节");
            }

            // 5. 验证卸载结果
            result.Success = await VerifyUninstallAsync(software);
            result.EndTime = DateTime.Now;
            result.Logs.Add($"卸载验证: {(result.Success ? "成功" : "仍有残留")}");

            _logger.LogInformation("卸载完成: {Name}, 成功: {Success}", software.Name, result.Success);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "卸载失败: {Name}", software.Name);
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.EndTime = DateTime.Now;
            result.Logs.Add($"异常: {ex.Message}");
            return result;
        }
    }

    private async Task<bool> ExecuteUninstallCommandAsync(string uninstallString, bool silent)
    {
        try
        {
            // 解析可执行文件和参数
            string fileName, arguments;
            var trimmed = uninstallString.Trim();

            if (trimmed.StartsWith('"'))
            {
                var endQuote = trimmed.IndexOf('"', 1);
                if (endQuote > 0)
                {
                    fileName = trimmed[1..endQuote];
                    arguments = trimmed[(endQuote + 1)..].Trim();
                }
                else
                {
                    fileName = trimmed.Trim('"');
                    arguments = "";
                }
            }
            else
            {
                var spaceIndex = trimmed.IndexOf(' ');
                if (spaceIndex > 0)
                {
                    fileName = trimmed[..spaceIndex];
                    arguments = trimmed[(spaceIndex + 1)..];
                }
                else
                {
                    fileName = trimmed;
                    arguments = "";
                }
            }

            if (silent)
            {
                // 尝试添加常见的静默参数
                var lowerArgs = arguments.ToLowerInvariant();
                if (!lowerArgs.Contains("/s") && !lowerArgs.Contains("/q") && !lowerArgs.Contains("/silent") && !lowerArgs.Contains("/quiet"))
                {
                    // 对于MSI安装包用 /qn，对于其他用 /S
                    if (fileName.EndsWith("msiexec", StringComparison.OrdinalIgnoreCase) ||
                        fileName.EndsWith("msiexec.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        arguments += " /qn /norestart";
                    }
                    else
                    {
                        arguments += " /S";
                    }
                }
            }

            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(psi);
            if (process == null) return false;

            // 设置超时
            var timeout = TimeSpan.FromMinutes(5);
            using var cts = new CancellationTokenSource(timeout);

            try
            {
                await process.WaitForExitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("卸载命令超时，强制结束进程");
                try { process.Kill(); } catch { }
                return false;
            }

            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行卸载命令失败: {Command}", uninstallString);
            return false;
        }
    }

    private async Task<CleanupResult> DeepCleanAsync(SoftwareInfo software)
    {
        var result = new CleanupResult();

        // 清理安装目录
        if (!string.IsNullOrEmpty(software.InstallPath) && Directory.Exists(software.InstallPath))
        {
            var dirSize = await _fileHelper.CalculateDirectorySizeAsync(software.InstallPath);
            var files = Directory.GetFiles(software.InstallPath, "*", SearchOption.AllDirectories);
            result.FilesDeleted += files.Length;
            result.SpaceFreed += dirSize;
            _fileHelper.TryDeleteDirectory(software.InstallPath);
        }

        // 清理注册表
        if (!string.IsNullOrEmpty(software.RegistryPath))
        {
            result.RegistryEntriesDeleted += await _registryHelper.DeleteRegistryKeyAsync(software.RegistryPath);
        }

        // 清理用户数据目录
        var appName = software.Name;
        var appDataPaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), appName)
        };

        foreach (var path in appDataPaths)
        {
            if (Directory.Exists(path))
            {
                try
                {
                    var dirSize = await _fileHelper.CalculateDirectorySizeAsync(path);
                    var fileCount = Directory.GetFiles(path, "*", SearchOption.AllDirectories).Length;
                    result.FilesDeleted += fileCount;
                    result.SpaceFreed += dirSize;
                    _fileHelper.TryDeleteDirectory(path);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "清理用户数据目录失败: {Path}", path);
                }
            }
        }

        // 清理临时目录中的相关文件
        var tempPath = Path.GetTempPath();
        try
        {
            foreach (var dir in Directory.GetDirectories(tempPath, $"*{appName}*"))
            {
                _fileHelper.TryDeleteDirectory(dir);
            }
            foreach (var file in Directory.GetFiles(tempPath, $"*{appName}*"))
            {
                _fileHelper.TryDeleteFile(file);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "清理临时文件失败");
        }

        return result;
    }

    private async Task<bool> VerifyUninstallAsync(SoftwareInfo software)
    {
        // 便携软件只检查目录
        if (software.IsPortable)
        {
            return string.IsNullOrEmpty(software.InstallPath) || !Directory.Exists(software.InstallPath);
        }

        // 检查安装目录是否还存在
        if (!string.IsNullOrEmpty(software.InstallPath) && Directory.Exists(software.InstallPath))
        {
            // 如果目录还在但几乎为空，也算成功
            var remainingFiles = Directory.GetFiles(software.InstallPath, "*", SearchOption.AllDirectories);
            if (remainingFiles.Length > 3)
                return false;
        }

        // 检查注册表项
        if (!string.IsNullOrEmpty(software.RegistryPath))
        {
            if (await _registryHelper.RegistryKeyExistsAsync(software.RegistryPath))
                return false;
        }

        // 检查进程
        if (software.ProcessId.HasValue && _processHelper.IsProcessRunning(software.ProcessId.Value))
            return false;

        return true;
    }

    private async Task CreateSystemRestorePointAsync(string description)
    {
        var script = $"Checkpoint-Computer -Description 'SmartUninstaller: {description}' -RestorePointType 'APPLICATION_INSTALL'";
        await _processHelper.ExecutePowerShellScriptAsync(script);
    }
}
