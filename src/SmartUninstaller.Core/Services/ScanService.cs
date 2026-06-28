using SmartUninstaller.Core.Interfaces;
using SmartUninstaller.Core.Models;
using SmartUninstaller.Core.Engines;
using Microsoft.Extensions.Logging;

namespace SmartUninstaller.Core.Services;

/// <summary>
/// 扫描服务实现
/// </summary>
public class ScanService : IScanService
{
    private readonly ILogger<ScanService> _logger;
    private readonly ScanEngine _scanEngine;
    private readonly PortableSoftwareEngine _portableEngine;

    public ScanService(
        ILogger<ScanService> logger,
        ScanEngine scanEngine,
        PortableSoftwareEngine portableEngine)
    {
        _logger = logger;
        _scanEngine = scanEngine;
        _portableEngine = portableEngine;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<SoftwareInfo>> ScanInstalledSoftwareAsync()
    {
        return await _scanEngine.ScanInstalledSoftwareAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<SoftwareInfo>> ScanPortableSoftwareAsync()
    {
        return await _portableEngine.ScanPortableSoftwareAsync();
    }

    /// <inheritdoc/>
    public Task<IEnumerable<LeftoverInfo>> ScanLeftoversAsync(SoftwareInfo software)
    {
        _logger.LogInformation("扫描软件残留: {Name}", software.Name);
        // 占位实现 - 后续集成AI引擎
        return Task.FromResult<IEnumerable<LeftoverInfo>>([]);
    }

    /// <inheritdoc/>
    public Task<IEnumerable<LeftoverInfo>> ScanSystemLeftoversAsync()
    {
        _logger.LogInformation("扫描系统残留");
        return Task.FromResult<IEnumerable<LeftoverInfo>>([]);
    }

    /// <inheritdoc/>
    public Task<IEnumerable<BrowserExtension>> ScanBrowserExtensionsAsync()
    {
        _logger.LogInformation("扫描浏览器扩展");
        return Task.FromResult<IEnumerable<BrowserExtension>>([]);
    }

    /// <inheritdoc/>
    public Task<IEnumerable<SoftwareInfo>> ScanWindowsAppsAsync()
    {
        _logger.LogInformation("扫描Windows应用");
        return Task.FromResult<IEnumerable<SoftwareInfo>>([]);
    }

    /// <inheritdoc/>
    public Task<IEnumerable<SoftwareInfo>> SearchSoftwareAsync(string keyword)
    {
        _logger.LogInformation("搜索软件: {Keyword}", keyword);
        return Task.FromResult<IEnumerable<SoftwareInfo>>([]);
    }

    /// <inheritdoc/>
    public Task<SoftwareInfo?> GetSoftwareDetailsAsync(string softwareId)
    {
        return Task.FromResult<SoftwareInfo?>(null);
    }

    /// <inheritdoc/>
    public ScanProgress GetScanProgress() => _scanEngine.GetScanProgress();

    /// <inheritdoc/>
    public void CancelScan() => _scanEngine.CancelScan();
}
