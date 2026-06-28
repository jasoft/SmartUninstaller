namespace SmartUninstaller.Shared.Helpers;

/// <summary>
/// 路径辅助工具类
/// </summary>
public static class PathHelper
{
    /// <summary>
    /// 获取应用数据目录路径
    /// </summary>
    /// <returns>应用数据目录路径</returns>
    public static string GetAppDataPath()
    {
        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SmartUninstaller");
        Directory.CreateDirectory(path);
        return path;
    }

    /// <summary>
    /// 获取备份目录路径
    /// </summary>
    /// <returns>备份目录路径</returns>
    public static string GetBackupPath()
    {
        var path = Path.Combine(GetAppDataPath(), "Backups");
        Directory.CreateDirectory(path);
        return path;
    }

    /// <summary>
    /// 获取日志目录路径
    /// </summary>
    /// <returns>日志目录路径</returns>
    public static string GetLogPath()
    {
        var path = Path.Combine(GetAppDataPath(), "Logs");
        Directory.CreateDirectory(path);
        return path;
    }

    /// <summary>
    /// 获取数据库文件路径
    /// </summary>
    /// <returns>数据库文件路径</returns>
    public static string GetDatabasePath()
    {
        return Path.Combine(GetAppDataPath(), Constants.AppConstants.DatabaseFileName);
    }

    /// <summary>
    /// 安全地规范化路径
    /// </summary>
    /// <param name="path">原始路径</param>
    /// <returns>规范化后的路径</returns>
    public static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return path;
        return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
}
