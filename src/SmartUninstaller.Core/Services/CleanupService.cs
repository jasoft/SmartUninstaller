using SmartUninstaller.Core.Interfaces;
using SmartUninstaller.Core.Engines;
using SmartUninstaller.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace SmartUninstaller.Core.Services;

/// <summary>
/// 清理服务实现 - 完整支持所有清理类型
/// </summary>
public class CleanupService : ICleanupService
{
    private readonly ILogger<CleanupService> _logger;
    private readonly CleanupEngine _cleanupEngine;

    public CleanupService(ILogger<CleanupService> logger, CleanupEngine cleanupEngine)
    {
        _logger = logger;
        _cleanupEngine = cleanupEngine;
    }

    /// <inheritdoc/>
    public async Task<Interfaces.CleanupResult> CleanSystemAsync(CleanupType type)
    {
        _logger.LogInformation("执行系统清理: {Type}", type);

        long size;
        switch (type)
        {
            case CleanupType.TemporaryFiles:
                size = await _cleanupEngine.CleanTempFilesAsync();
                break;
            case CleanupType.RecycleBin:
                size = await _cleanupEngine.CleanRecycleBinAsync();
                break;
            case CleanupType.BrowserCache:
                size = await _cleanupEngine.CleanBrowserCacheAsync();
                break;
            case CleanupType.SystemLogs:
                size = await _cleanupEngine.CleanSystemLogsAsync();
                break;
            default:
                _logger.LogWarning("不支持的清理类型: {Type}", type);
                size = 0;
                break;
        }

        _logger.LogInformation("清理完成: {Type}, 释放 {Size} bytes", type, size);
        return new Interfaces.CleanupResult { SpaceFreed = size };
    }

    /// <inheritdoc/>
    public async Task<Interfaces.SystemCleanupInfo> AnalyzeSystemAsync()
    {
        _logger.LogInformation("开始分析系统可清理项...");

        var tempSize = await _cleanupEngine.AnalyzeTempFilesAsync();
        var recycleBinSize = await _cleanupEngine.AnalyzeRecycleBinAsync();
        var browserCacheSize = await _cleanupEngine.AnalyzeBrowserCacheAsync();
        var systemLogsSize = await _cleanupEngine.AnalyzeSystemLogsAsync();

        _logger.LogInformation(
            "系统分析完成 - 临时文件: {Temp}MB, 回收站: {Recycle}MB, 浏览器缓存: {Browser}MB, 系统日志: {Log}MB",
            tempSize / 1024 / 1024, recycleBinSize / 1024 / 1024,
            browserCacheSize / 1024 / 1024, systemLogsSize / 1024 / 1024);

        return new Interfaces.SystemCleanupInfo
        {
            TempFilesSize = tempSize,
            RecycleBinSize = recycleBinSize,
            BrowserCacheSize = browserCacheSize,
            SystemLogsSize = systemLogsSize
        };
    }

    /// <inheritdoc/>
    public async Task<long> CleanPathAsync(string path, TimeSpan? olderThan = null)
    {
        _logger.LogInformation("清理路径: {Path}", path);
        if (!Directory.Exists(path)) return 0;
        return await _cleanupEngine.CleanTempFilesAsync(olderThan);
    }
}
