using Microsoft.Extensions.Logging;

namespace SmartUninstaller.Core.Utils;

/// <summary>
/// 文件操作辅助工具类
/// </summary>
public class FileHelper
{
    private readonly ILogger<FileHelper> _logger;

    public FileHelper(ILogger<FileHelper> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 计算目录大小
    /// </summary>
    /// <param name="directoryPath">目录路径</param>
    /// <returns>目录大小（字节）</returns>
    public virtual async Task<long> CalculateDirectorySizeAsync(string directoryPath)
    {
        try
        {
            if (!Directory.Exists(directoryPath)) return 0;

            var dirInfo = new DirectoryInfo(directoryPath);
            return await Task.Run(() => dirInfo.EnumerateFiles("*", new EnumerationOptions
            {
                RecurseSubdirectories = true,
                IgnoreInaccessible = true
            }).Sum(f => f.Length));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "计算目录大小失败: {Path}", directoryPath);
            return 0;
        }
    }

    /// <summary>
    /// 安全删除文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>是否成功删除</returns>
    public virtual bool TryDeleteFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath)) return true;
            File.Delete(filePath);
            return true;
        }
        catch (IOException)
        {
            _logger.LogWarning("文件被锁定，标记延迟删除: {Path}", filePath);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除文件失败: {Path}", filePath);
            return false;
        }
    }

    /// <summary>
    /// 安全删除目录
    /// </summary>
    /// <param name="directoryPath">目录路径</param>
    /// <param name="recursive">是否递归删除</param>
    /// <returns>是否成功删除</returns>
    public virtual bool TryDeleteDirectory(string directoryPath, bool recursive = true)
    {
        try
        {
            if (!Directory.Exists(directoryPath)) return true;
            Directory.Delete(directoryPath, recursive);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除目录失败: {Path}", directoryPath);
            return false;
        }
    }
}
