using Microsoft.Extensions.Logging;

namespace SmartUninstaller.Core.Engines;

/// <summary>
/// 清理引擎 - 提供系统各类可清理项的分析和清理能力
/// </summary>
public class CleanupEngine
{
    private readonly ILogger<CleanupEngine> _logger;

    public CleanupEngine(ILogger<CleanupEngine> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 分析临时文件大小
    /// </summary>
    public virtual Task<long> AnalyzeTempFilesAsync()
    {
        var totalSize = 0L;
        var tempPaths = new[]
        {
            Path.GetTempPath(),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp")
        };

        foreach (var tempPath in tempPaths)
        {
            totalSize += GetDirectorySizeSafe(tempPath);
        }

        _logger.LogInformation("临时文件分析完成: {Size} bytes", totalSize);
        return Task.FromResult(totalSize);
    }

    /// <summary>
    /// 清理临时文件
    /// </summary>
    public virtual async Task<long> CleanTempFilesAsync(TimeSpan? olderThan = null)
    {
        var totalCleaned = 0L;
        var tempPaths = new[]
        {
            Path.GetTempPath(),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp")
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
                    catch { }
                }

                // 删除空目录
                CleanEmptyDirectories(tempPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "清理临时目录失败: {Path}", tempPath);
            }
        }

        _logger.LogInformation("临时文件清理完成: {Size} bytes", totalCleaned);
        return await Task.FromResult(totalCleaned);
    }

    /// <summary>
    /// 分析回收站大小
    /// </summary>
    public virtual Task<long> AnalyzeRecycleBinAsync()
    {
        var totalSize = 0L;
        try
        {
            // 使用SHQueryRecycleBin获取回收站大小
            var info = new SHQUERYRBINFO();
            info.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(info);
            if (SHQueryRecycleBin(null, ref info) == 0)
            {
                totalSize = (long)info.i64Size;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "分析回收站失败");
        }

        return Task.FromResult(totalSize);
    }

    /// <summary>
    /// 清空回收站
    /// </summary>
    public virtual Task<long> CleanRecycleBinAsync()
    {
        var sizeBefore = 0L;
        try
        {
            var info = new SHQUERYRBINFO();
            info.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(info);
            if (SHQueryRecycleBin(null, ref info) == 0)
            {
                sizeBefore = (long)info.i64Size;
            }

            // SHEmptyRecycleBin: SHERB_NOCONFIRMATION | SHERB_NOPROGRESSUI | SHERB_NOSOUND
            SHEmptyRecycleBin(IntPtr.Zero, null, 0x00000007);
            _logger.LogInformation("回收站已清空: {Size} bytes", sizeBefore);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清空回收站失败");
        }

        return Task.FromResult(sizeBefore);
    }

    /// <summary>
    /// 分析浏览器缓存大小
    /// </summary>
    public virtual Task<long> AnalyzeBrowserCacheAsync()
    {
        var totalSize = 0L;

        // Chrome 缓存
        var chromeCachePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            @"Google\Chrome\User Data\Default\Cache");
        totalSize += GetDirectorySizeSafe(chromeCachePath);

        var chromeCodeCachePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            @"Google\Chrome\User Data\Default\Code Cache");
        totalSize += GetDirectorySizeSafe(chromeCodeCachePath);

        // Edge 缓存
        var edgeCachePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            @"Microsoft\Edge\User Data\Default\Cache");
        totalSize += GetDirectorySizeSafe(edgeCachePath);

        var edgeCodeCachePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            @"Microsoft\Edge\User Data\Default\Code Cache");
        totalSize += GetDirectorySizeSafe(edgeCodeCachePath);

        // Firefox 缓存
        var firefoxProfilesPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            @"Mozilla\Firefox\Profiles");
        if (Directory.Exists(firefoxProfilesPath))
        {
            foreach (var profileDir in Directory.GetDirectories(firefoxProfilesPath))
            {
                var cache2Path = Path.Combine(profileDir, "cache2");
                totalSize += GetDirectorySizeSafe(cache2Path);
            }
        }

        _logger.LogInformation("浏览器缓存分析完成: {Size} bytes", totalSize);
        return Task.FromResult(totalSize);
    }

    /// <summary>
    /// 清理浏览器缓存
    /// </summary>
    public virtual Task<long> CleanBrowserCacheAsync()
    {
        var totalCleaned = 0L;

        var cachePaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Google\Chrome\User Data\Default\Cache"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Google\Chrome\User Data\Default\Code Cache"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Microsoft\Edge\User Data\Default\Cache"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Microsoft\Edge\User Data\Default\Code Cache")
        };

        foreach (var cachePath in cachePaths)
        {
            totalCleaned += CleanDirectoryContents(cachePath);
        }

        // Firefox
        var firefoxProfilesPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            @"Mozilla\Firefox\Profiles");
        if (Directory.Exists(firefoxProfilesPath))
        {
            foreach (var profileDir in Directory.GetDirectories(firefoxProfilesPath))
            {
                totalCleaned += CleanDirectoryContents(Path.Combine(profileDir, "cache2"));
            }
        }

        _logger.LogInformation("浏览器缓存清理完成: {Size} bytes", totalCleaned);
        return Task.FromResult(totalCleaned);
    }

    /// <summary>
    /// 分析系统日志大小
    /// </summary>
    public virtual Task<long> AnalyzeSystemLogsAsync()
    {
        var totalSize = 0L;

        // Windows 日志目录
        var logPaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), @"Logs"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), @"Logs\CBS"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), @"Logs\DISM"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), @"System32\winevt\Logs"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Microsoft\Windows\WER")
        };

        foreach (var logPath in logPaths)
        {
            totalSize += GetDirectorySizeSafe(logPath);
        }

        _logger.LogInformation("系统日志分析完成: {Size} bytes", totalSize);
        return Task.FromResult(totalSize);
    }

    /// <summary>
    /// 清理系统日志
    /// </summary>
    public virtual Task<long> CleanSystemLogsAsync()
    {
        var totalCleaned = 0L;

        var cleanableLogPaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), @"Logs\CBS"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), @"Logs\DISM"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Microsoft\Windows\WER")
        };

        foreach (var logPath in cleanableLogPaths)
        {
            totalCleaned += CleanDirectoryContents(logPath);
        }

        _logger.LogInformation("系统日志清理完成: {Size} bytes", totalCleaned);
        return Task.FromResult(totalCleaned);
    }

    #region P/Invoke for RecycleBin

    [System.Runtime.InteropServices.DllImport("shell32.dll")]
    private static extern int SHQueryRecycleBin(string? pszRootPath, ref SHQUERYRBINFO pSHQueryRBInfo);

    [System.Runtime.InteropServices.DllImport("shell32.dll")]
    private static extern int SHEmptyRecycleBin(IntPtr hwnd, string? pszRootPath, uint dwFlags);

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 4)]
    private struct SHQUERYRBINFO
    {
        public uint cbSize;
        public long i64Size;
        public long i64NumItems;
    }

    #endregion

    #region 私有辅助方法

    /// <summary>
    /// 安全获取目录大小（忽略无法访问的文件）
    /// </summary>
    private long GetDirectorySizeSafe(string path)
    {
        if (!Directory.Exists(path)) return 0;

        try
        {
            return new DirectoryInfo(path)
                .EnumerateFiles("*", new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true })
                .Sum(f => f.Length);
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// 清理目录中的所有内容（保留目录本身）
    /// </summary>
    private long CleanDirectoryContents(string path)
    {
        if (!Directory.Exists(path)) return 0;

        var cleanedSize = 0L;
        try
        {
            foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            {
                try
                {
                    cleanedSize += new FileInfo(file).Length;
                    File.Delete(file);
                }
                catch { }
            }

            CleanEmptyDirectories(path);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "清理目录失败: {Path}", path);
        }

        return cleanedSize;
    }

    /// <summary>
    /// 递归删除空目录
    /// </summary>
    private void CleanEmptyDirectories(string path)
    {
        try
        {
            foreach (var dir in Directory.GetDirectories(path))
            {
                CleanEmptyDirectories(dir);
                if (!Directory.EnumerateFileSystemEntries(dir).Any())
                {
                    try { Directory.Delete(dir); } catch { }
                }
            }
        }
        catch { }
    }

    #endregion
}
