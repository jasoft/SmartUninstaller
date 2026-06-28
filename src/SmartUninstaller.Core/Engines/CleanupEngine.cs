using Microsoft.Extensions.Logging;

namespace SmartUninstaller.Core.Engines;

/// <summary>
/// 清理引擎，负责系统清理操作
/// </summary>
public class CleanupEngine
{
    private readonly ILogger<CleanupEngine> _logger;

    public CleanupEngine(ILogger<CleanupEngine> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 清理临时文件
    /// </summary>
    /// <param name="olderThan">仅清理早于指定时间的文件</param>
    /// <returns>清理的文件大小（字节）</returns>
    public virtual async Task<long> CleanTempFilesAsync(TimeSpan? olderThan = null)
    {
        var totalCleaned = 0L;
        var tempPaths = new[]
        {
            Path.GetTempPath(),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Temp")
        };

        foreach (var tempPath in tempPaths)
        {
            if (!Directory.Exists(tempPath)) continue;

            try
            {
                var files = Directory.GetFiles(tempPath, "*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        if (olderThan.HasValue && fileInfo.LastWriteTime > DateTime.Now - olderThan.Value)
                            continue;

                        totalCleaned += fileInfo.Length;
                        fileInfo.Delete();
                    }
                    catch
                    {
                        // 忽略无法删除的文件
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "清理临时目录失败: {Path}", tempPath);
            }
        }

        _logger.LogInformation("临时文件清理完成，释放 {Size} 空间", totalCleaned);
        return await Task.FromResult(totalCleaned);
    }
}
