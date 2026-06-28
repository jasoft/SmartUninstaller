using SmartUninstaller.Core.Models;
using SmartUninstaller.Core.Utils;
using Microsoft.Extensions.Logging;

namespace SmartUninstaller.Core.Engines;

/// <summary>
/// 便携软件检测引擎
/// </summary>
public class PortableSoftwareEngine
{
    private readonly ILogger<PortableSoftwareEngine> _logger;
    private readonly FileHelper _fileHelper;

    private readonly string[] _portableSoftwareDirectories;
    private readonly string[] _portableSoftwareIndicators =
    [
        "portable.dat", "AppName.portable", ".portable",
        "portableapps.ini", "settings.ini", "config.ini"
    ];

    public PortableSoftwareEngine(ILogger<PortableSoftwareEngine> logger, FileHelper fileHelper)
    {
        _logger = logger;
        _fileHelper = fileHelper;
        _portableSoftwareDirectories =
        [
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "PortableApps"),
            @"C:\PortableApps",
            @"D:\PortableApps",
            @"E:\PortableApps"
        ];
    }

    /// <summary>
    /// 扫描便携软件
    /// </summary>
    /// <returns>便携软件列表</returns>
    public virtual async Task<IEnumerable<SoftwareInfo>> ScanPortableSoftwareAsync()
    {
        var portableSoftwareList = new List<SoftwareInfo>();

        try
        {
            _logger.LogInformation("开始扫描便携软件...");

            foreach (var directory in _portableSoftwareDirectories)
            {
                if (!Directory.Exists(directory)) continue;
                var software = await ScanDirectoryForPortableSoftwareAsync(directory);
                portableSoftwareList.AddRange(software);
            }

            _logger.LogInformation("扫描完成，发现 {Count} 个便携软件", portableSoftwareList.Count);
            return portableSoftwareList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "扫描便携软件失败");
            throw;
        }
    }

    private async Task<IEnumerable<SoftwareInfo>> ScanDirectoryForPortableSoftwareAsync(string directoryPath)
    {
        var portableSoftwareList = new List<SoftwareInfo>();

        try
        {
            foreach (var directory in Directory.GetDirectories(directoryPath))
            {
                if (await IsPortableSoftwareAsync(directory))
                {
                    var software = await CreatePortableSoftwareInfoAsync(directory);
                    if (software != null) portableSoftwareList.Add(software);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "扫描目录失败: {Path}", directoryPath);
        }

        return portableSoftwareList;
    }

    private async Task<bool> IsPortableSoftwareAsync(string directoryPath)
    {
        try
        {
            // 检查特征文件
            foreach (var indicator in _portableSoftwareIndicators)
            {
                if (File.Exists(Path.Combine(directoryPath, indicator)))
                    return true;
            }

            // 检查有可执行文件但无卸载程序
            var executables = Directory.GetFiles(directoryPath, "*.exe", SearchOption.TopDirectoryOnly);
            var uninstallers = Directory.GetFiles(directoryPath, "uninstall*.exe", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(directoryPath, "uninst*.exe", SearchOption.AllDirectories))
                .ToArray();

            if (executables.Length > 0 && uninstallers.Length == 0)
            {
                var configFiles = Directory.GetFiles(directoryPath, "*.ini", SearchOption.TopDirectoryOnly)
                    .Concat(Directory.GetFiles(directoryPath, "*.json", SearchOption.TopDirectoryOnly))
                    .Concat(Directory.GetFiles(directoryPath, "*.xml", SearchOption.TopDirectoryOnly))
                    .ToArray();

                if (configFiles.Length > 0) return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "检查便携软件失败: {Path}", directoryPath);
            return false;
        }
    }

    private async Task<SoftwareInfo?> CreatePortableSoftwareInfoAsync(string directoryPath)
    {
        try
        {
            var directoryInfo = new DirectoryInfo(directoryPath);
            var executables = Directory.GetFiles(directoryPath, "*.exe", SearchOption.TopDirectoryOnly);

            var software = new SoftwareInfo
            {
                Id = Guid.NewGuid().ToString(),
                Name = directoryInfo.Name,
                InstallPath = directoryPath,
                InstallDate = directoryInfo.CreationTime,
                Size = await _fileHelper.CalculateDirectorySizeAsync(directoryPath),
                IsPortable = true,
                Architecture = Shared.Enums.SoftwareArchitecture.AnyCPU,
                IsInstalled = true,
                Category = Shared.Enums.SoftwareCategory.Application
            };

            if (executables.Length > 0)
            {
                var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(executables[0]);
                software.Version = versionInfo.FileVersion;
                software.Publisher = versionInfo.CompanyName;
                software.Description = versionInfo.FileDescription;
                software.IconPath = executables[0];

                var runningProcesses = System.Diagnostics.Process.GetProcessesByName(
                    Path.GetFileNameWithoutExtension(executables[0]));
                if (runningProcesses.Length > 0)
                {
                    software.IsRunning = true;
                    software.ProcessId = runningProcesses[0].Id;
                }
            }

            return software;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "创建便携软件信息失败: {Path}", directoryPath);
            return null;
        }
    }
}
