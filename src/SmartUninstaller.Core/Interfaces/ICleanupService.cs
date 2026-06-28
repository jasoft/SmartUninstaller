using SmartUninstaller.Shared.Enums;

namespace SmartUninstaller.Core.Interfaces;

/// <summary>
/// 清理服务接口
/// </summary>
public interface ICleanupService
{
    /// <summary>执行系统清理</summary>
    Task<Interfaces.CleanupResult> CleanSystemAsync(CleanupType type);
    /// <summary>分析系统可清理项</summary>
    Task<SystemCleanupInfo> AnalyzeSystemAsync();
    /// <summary>清理指定路径的文件</summary>
    Task<long> CleanPathAsync(string path, TimeSpan? olderThan = null);
}

/// <summary>
/// 系统清理信息
/// </summary>
public class SystemCleanupInfo
{
    /// <summary>临时文件大小（字节）</summary>
    public long TempFilesSize { get; set; }
    /// <summary>Windows更新缓存大小（字节）</summary>
    public long WindowsUpdateCacheSize { get; set; }
    /// <summary>回收站大小（字节）</summary>
    public long RecycleBinSize { get; set; }
    /// <summary>浏览器缓存大小（字节）</summary>
    public long BrowserCacheSize { get; set; }
    /// <summary>系统日志大小（字节）</summary>
    public long SystemLogsSize { get; set; }
    /// <summary>可清理总大小（字节）</summary>
    public long TotalCleanableSize => TempFilesSize + WindowsUpdateCacheSize + RecycleBinSize + BrowserCacheSize + SystemLogsSize;
}
