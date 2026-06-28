namespace SmartUninstaller.Core.Interfaces;

/// <summary>
/// 日志服务接口
/// </summary>
public interface ILoggingService
{
    /// <summary>记录信息日志</summary>
    void LogInfo(string message);
    /// <summary>记录警告日志</summary>
    void LogWarning(string message);
    /// <summary>记录错误日志</summary>
    void LogError(string message, Exception? exception = null);
    /// <summary>记录卸载操作日志</summary>
    void LogUninstallOperation(string softwareName, bool success, string? details = null);
    /// <summary>获取日志文件路径</summary>
    string GetLogFilePath();
}
