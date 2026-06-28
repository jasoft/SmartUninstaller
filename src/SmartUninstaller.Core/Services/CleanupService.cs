using SmartUninstaller.Core.Interfaces;
using SmartUninstaller.Core.Engines;
using SmartUninstaller.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace SmartUninstaller.Core.Services;

/// <summary>
/// 清理服务实现
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
        var size = type switch
        {
            CleanupType.TemporaryFiles => await _cleanupEngine.CleanTempFilesAsync(),
            _ => 0L
        };

        return new Interfaces.CleanupResult { SpaceFreed = size };
    }

    /// <inheritdoc/>
    public async Task<Interfaces.SystemCleanupInfo> AnalyzeSystemAsync()
    {
        _logger.LogInformation("分析系统可清理项");
        var tempPath = Path.GetTempPath();
        var tempSize = 0L;

        try
        {
            if (Directory.Exists(tempPath))
            {
                tempSize = Directory.GetFiles(tempPath, "*", SearchOption.AllDirectories)
                    .Sum(f => new FileInfo(f).Length);
            }
        }
        catch { }

        return await Task.FromResult(new Interfaces.SystemCleanupInfo
        {
            TempFilesSize = tempSize
        });
    }

    /// <inheritdoc/>
    public async Task<long> CleanPathAsync(string path, TimeSpan? olderThan = null)
    {
        return await _cleanupEngine.CleanTempFilesAsync(olderThan);
    }
}
