using SmartUninstaller.Core.Models;
using SmartUninstaller.Core.Utils;
using Microsoft.Extensions.Logging;

using SmartUninstaller.Core.Interfaces;

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
    /// <param name="software">软件信息</param>
    /// <param name="options">卸载选项</param>
    /// <returns>卸载结果</returns>
    public virtual async Task<UninstallResult> ExecuteUninstallAsync(SoftwareInfo software, Interfaces.UninstallOptions options)
    {
        var result = new UninstallResult
        {
            Software = software,
            StartTime = DateTime.Now
        };

        try
        {
            _logger.LogInformation("开始卸载软件: {Name}", software.Name);

            // 1. 检查并关闭运行中的进程
            if (software.IsRunning && software.ProcessId.HasValue)
            {
                _logger.LogInformation("软件正在运行，尝试关闭进程: {ProcessId}", software.ProcessId);
                await _processHelper.KillProcessAsync(software.ProcessId.Value);
            }

            // 2. 创建系统还原点
            if (options.CreateRestorePoint)
            {
                _logger.LogInformation("创建系统还原点...");
                await CreateSystemRestorePointAsync(software.Name);
            }

            // 3. 执行卸载命令
            if (!string.IsNullOrEmpty(software.UninstallString))
            {
                _logger.LogInformation("执行卸载命令: {Command}", software.UninstallString);
                var uninstallSuccess = await ExecuteUninstallCommandAsync(software.UninstallString, options.SilentUninstall);

                if (!uninstallSuccess && !options.ForceUninstall)
                {
                    result.Success = false;
                    result.ErrorMessage = "卸载命令执行失败";
                    result.EndTime = DateTime.Now;
                    return result;
                }
            }

            // 4. 深度清理残留
            if (options.DeepClean || options.ForceUninstall)
            {
                _logger.LogInformation("执行深度清理...");
                var cleanupResult = await DeepCleanAsync(software);
                result.FilesDeleted += cleanupResult.FilesDeleted;
                result.RegistryEntriesDeleted += cleanupResult.RegistryEntriesDeleted;
                result.SpaceFreed += cleanupResult.SpaceFreed;
            }

            // 5. 验证卸载结果
            result.Success = await VerifyUninstallAsync(software);
            result.EndTime = DateTime.Now;

            _logger.LogInformation("卸载完成: {Name}, 成功: {Success}", software.Name, result.Success);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "卸载失败: {Name}", software.Name);
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.EndTime = DateTime.Now;
            return result;
        }
    }

    private async Task<bool> ExecuteUninstallCommandAsync(string uninstallString, bool silent)
    {
        try
        {
            var arguments = silent ? $"{uninstallString} /silent /quiet" : uninstallString;
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {arguments}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(psi);
            if (process == null) return false;
            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行卸载命令失败");
            return false;
        }
    }

    private async Task<Interfaces.CleanupResult> DeepCleanAsync(SoftwareInfo software)
    {
        var result = new Interfaces.CleanupResult();

        if (!string.IsNullOrEmpty(software.InstallPath) && Directory.Exists(software.InstallPath))
        {
            var dirSize = await new FileHelper(_logger as ILogger<FileHelper> ?? throw new InvalidOperationException()).CalculateDirectorySizeAsync(software.InstallPath);
            var files = Directory.GetFiles(software.InstallPath, "*", SearchOption.AllDirectories);
            result.FilesDeleted += files.Length;
            result.SpaceFreed += dirSize;
            _fileHelper.TryDeleteDirectory(software.InstallPath);
        }

        if (!string.IsNullOrEmpty(software.RegistryPath))
        {
            result.RegistryEntriesDeleted += await _registryHelper.DeleteRegistryKeyAsync(software.RegistryPath);
        }

        // 清理用户数据目录
        var appDataPaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), software.Name),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), software.Name),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), software.Name)
        };

        foreach (var path in appDataPaths)
        {
            if (Directory.Exists(path))
            {
                var dirSize = await new FileHelper(_logger as ILogger<FileHelper> ?? throw new InvalidOperationException()).CalculateDirectorySizeAsync(path);
                result.FilesDeleted += Directory.GetFiles(path, "*", SearchOption.AllDirectories).Length;
                result.SpaceFreed += dirSize;
                _fileHelper.TryDeleteDirectory(path);
            }
        }

        return result;
    }

    private async Task<bool> VerifyUninstallAsync(SoftwareInfo software)
    {
        if (!string.IsNullOrEmpty(software.InstallPath) && Directory.Exists(software.InstallPath))
            return false;

        if (!string.IsNullOrEmpty(software.RegistryPath))
        {
            if (await _registryHelper.RegistryKeyExistsAsync(software.RegistryPath))
                return false;
        }

        if (software.ProcessId.HasValue && _processHelper.IsProcessRunning(software.ProcessId.Value))
            return false;

        return true;
    }

    private async Task CreateSystemRestorePointAsync(string description)
    {
        try
        {
            var script = $"Checkpoint-Computer -Description 'SmartUninstaller: {description}' -RestorePointType 'APPLICATION_INSTALL'";
            await _processHelper.ExecutePowerShellScriptAsync(script);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "创建系统还原点失败");
        }
    }
}

