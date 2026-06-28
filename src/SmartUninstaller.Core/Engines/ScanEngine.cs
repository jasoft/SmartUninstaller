using SmartUninstaller.Core.Models;
using SmartUninstaller.Core.Utils;
using Microsoft.Extensions.Logging;

namespace SmartUninstaller.Core.Engines;

/// <summary>
/// 扫描引擎，负责扫描系统中已安装的软件
/// </summary>
public class ScanEngine
{
    private readonly ILogger<ScanEngine> _logger;
    private readonly RegistryHelper _registryHelper;
    private readonly FileHelper _fileHelper;
    private readonly WmiHelper _wmiHelper;
    private readonly ProcessHelper _processHelper;

    private Interfaces.ScanProgress _progress = new();
    private CancellationTokenSource? _cancellationTokenSource;

    public ScanEngine(
        ILogger<ScanEngine> logger,
        RegistryHelper registryHelper,
        FileHelper fileHelper,
        WmiHelper wmiHelper,
        ProcessHelper processHelper)
    {
        _logger = logger;
        _registryHelper = registryHelper;
        _fileHelper = fileHelper;
        _wmiHelper = wmiHelper;
        _processHelper = processHelper;
    }

    /// <summary>
    /// 扫描已安装软件
    /// </summary>
    /// <returns>软件信息列表</returns>
    public virtual async Task<IEnumerable<SoftwareInfo>> ScanInstalledSoftwareAsync()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _progress = new Interfaces.ScanProgress { Stage = Interfaces.ScanStage.Initializing };

        var softwareList = new List<SoftwareInfo>();

        try
        {
            _logger.LogInformation("开始扫描已安装软件...");

            // 1. 扫描注册表
            _progress.Stage = Interfaces.ScanStage.ScanningRegistry;
            var registrySoftware = await ScanRegistryAsync();
            softwareList.AddRange(registrySoftware);

            // 2. 扫描文件系统
            _progress.Stage = Interfaces.ScanStage.ScanningFileSystem;
            var fileSystemSoftware = await ScanFileSystemAsync();
            softwareList.AddRange(fileSystemSoftware);

            // 3. 扫描WMI
            _progress.Stage = Interfaces.ScanStage.ScanningWMI;
            var wmiSoftware = await _wmiHelper.QueryInstalledSoftwareAsync();
            softwareList.AddRange(wmiSoftware);

            // 4. 去重合并
            _progress.Stage = Interfaces.ScanStage.AnalyzingResults;
            var uniqueSoftware = MergeAndDeduplicateSoftware(softwareList);

            // 5. 丰富进程信息
            _progress.Stage = Interfaces.ScanStage.ScanningProcesses;
            await EnrichWithProcessInfoAsync(uniqueSoftware);

            _progress.Stage = Interfaces.ScanStage.Completed;
            _progress.IsCompleted = true;

            _logger.LogInformation("扫描完成，发现 {Count} 个软件", uniqueSoftware.Count);
            return uniqueSoftware;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("扫描已取消");
            _progress.IsCancelled = true;
            return softwareList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "扫描失败");
            throw;
        }
    }

    private async Task<IEnumerable<SoftwareInfo>> ScanRegistryAsync()
    {
        var softwareList = new List<SoftwareInfo>();

        var hklmSoftware = await _registryHelper.ScanUninstallRegistryAsync(
            Shared.Constants.AppConstants.RegistryUninstallPath);
        softwareList.AddRange(hklmSoftware);

        var hkcuSoftware = await _registryHelper.ScanUninstallRegistryAsync(
            Shared.Constants.AppConstants.RegistryUninstallPath, true);
        softwareList.AddRange(hkcuSoftware);

        if (Environment.Is64BitOperatingSystem)
        {
            var wow64Software = await _registryHelper.ScanUninstallRegistryAsync(
                Shared.Constants.AppConstants.RegistryUninstallPathWow64);
            softwareList.AddRange(wow64Software);
        }

        return softwareList;
    }

    private async Task<IEnumerable<SoftwareInfo>> ScanFileSystemAsync()
    {
        var softwareList = new List<SoftwareInfo>();
        var programFilesPaths = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
        };

        foreach (var programFilesPath in programFilesPaths)
        {
            if (!Directory.Exists(programFilesPath)) continue;

            foreach (var directory in Directory.GetDirectories(programFilesPath))
            {
                _cancellationTokenSource?.Token.ThrowIfCancellationRequested();
                var software = await AnalyzeDirectoryAsync(directory);
                if (software != null) softwareList.Add(software);
            }
        }

        return softwareList;
    }

    private async Task<SoftwareInfo?> AnalyzeDirectoryAsync(string directoryPath)
    {
        try
        {
            var directoryInfo = new DirectoryInfo(directoryPath);
            var executables = Directory.GetFiles(directoryPath, "*.exe", SearchOption.TopDirectoryOnly);
            if (executables.Length == 0) return null;

            var uninstallers = Directory.GetFiles(directoryPath, "uninstall*.exe", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(directoryPath, "uninst*.exe", SearchOption.AllDirectories))
                .ToArray();

            var software = new SoftwareInfo
            {
                Id = Guid.NewGuid().ToString(),
                Name = directoryInfo.Name,
                InstallPath = directoryPath,
                InstallDate = directoryInfo.CreationTime,
                Size = await _fileHelper.CalculateDirectorySizeAsync(directoryPath),
                IsPortable = false,
                Architecture = Shared.Enums.SoftwareArchitecture.AnyCPU
            };

            if (uninstallers.Length > 0)
                software.UninstallString = uninstallers[0];

            if (executables.Length > 0)
            {
                var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(executables[0]);
                software.Version = versionInfo.FileVersion;
                software.Publisher = versionInfo.CompanyName;
                software.Description = versionInfo.FileDescription;
            }

            return software;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "分析目录失败: {Path}", directoryPath);
            return null;
        }
    }

    private List<SoftwareInfo> MergeAndDeduplicateSoftware(List<SoftwareInfo> softwareList)
    {
        var uniqueSoftware = new Dictionary<string, SoftwareInfo>();

        foreach (var software in softwareList)
        {
            var key = $"{software.Name}|{software.Version}|{software.Publisher}";
            if (!uniqueSoftware.ContainsKey(key))
            {
                uniqueSoftware[key] = software;
            }
            else
            {
                var existing = uniqueSoftware[key];
                if (string.IsNullOrEmpty(existing.InstallPath) && !string.IsNullOrEmpty(software.InstallPath))
                    existing.InstallPath = software.InstallPath;
                if (string.IsNullOrEmpty(existing.UninstallString) && !string.IsNullOrEmpty(software.UninstallString))
                    existing.UninstallString = software.UninstallString;
            }
        }

        return uniqueSoftware.Values.ToList();
    }

    private async Task EnrichWithProcessInfoAsync(List<SoftwareInfo> softwareList)
    {
        var runningProcesses = _processHelper.GetRunningProcesses();

        foreach (var software in softwareList)
        {
            if (string.IsNullOrEmpty(software.InstallPath)) continue;

            var matchingProcess = runningProcesses.FirstOrDefault(p =>
                p.MainModule?.FileName?.StartsWith(software.InstallPath, StringComparison.OrdinalIgnoreCase) == true);

            if (matchingProcess != null)
            {
                software.IsRunning = true;
                software.ProcessId = matchingProcess.Id;
            }
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// 获取扫描进度
    /// </summary>
    public Interfaces.ScanProgress GetScanProgress() => _progress;

    /// <summary>
    /// 取消扫描
    /// </summary>
    public void CancelScan() => _cancellationTokenSource?.Cancel();
}
