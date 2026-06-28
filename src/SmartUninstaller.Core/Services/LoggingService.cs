using SmartUninstaller.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace SmartUninstaller.Core.Services;

/// <summary>
/// 日志服务实现
/// </summary>
public class LoggingService : ILoggingService
{
    private readonly ILogger<LoggingService> _logger;
    private readonly string _logFilePath;

    public LoggingService(ILogger<LoggingService> logger)
    {
        _logger = logger;
        _logFilePath = Path.Combine(
            Shared.Helpers.PathHelper.GetLogPath(),
            Shared.Constants.AppConstants.LogFileName);
    }

    /// <inheritdoc/>
    public void LogInfo(string message)
    {
        _logger.LogInformation(message);
        WriteToFile("INFO", message);
    }

    /// <inheritdoc/>
    public void LogWarning(string message)
    {
        _logger.LogWarning(message);
        WriteToFile("WARN", message);
    }

    /// <inheritdoc/>
    public void LogError(string message, Exception? exception = null)
    {
        _logger.LogError(exception, message);
        WriteToFile("ERROR", $"{message} {exception?.Message}");
    }

    /// <inheritdoc/>
    public void LogUninstallOperation(string softwareName, bool success, string? details = null)
    {
        var status = success ? "成功" : "失败";
        var message = $"卸载操作: {softwareName} - {status}";
        if (!string.IsNullOrEmpty(details)) message += $" | {details}";

        _logger.LogInformation(message);
        WriteToFile("UNINSTALL", message);
    }

    /// <inheritdoc/>
    public string GetLogFilePath() => _logFilePath;

    private void WriteToFile(string level, string message)
    {
        try
        {
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}{Environment.NewLine}";
            File.AppendAllText(_logFilePath, logEntry);
        }
        catch
        {
            // 日志写入失败不应影响主程序
        }
    }
}
