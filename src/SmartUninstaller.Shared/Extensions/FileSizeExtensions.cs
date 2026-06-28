namespace SmartUninstaller.Shared.Extensions;

/// <summary>
/// 文件大小格式化扩展方法
/// </summary>
public static class FileSizeExtensions
{
    /// <summary>
    /// 将字节数格式化为可读的文件大小字符串
    /// </summary>
    /// <param name="bytes">字节数</param>
    /// <returns>格式化后的字符串</returns>
    public static string ToFileSizeString(this long bytes)
    {
        string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
        int order = 0;
        double size = bytes;

        while (size >= 1024 && order < suffixes.Length - 1)
        {
            order++;
            size /= 1024;
        }

        return $"{size:0.##} {suffixes[order]}";
    }
}
