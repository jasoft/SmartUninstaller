namespace SmartUninstaller.Shared.Constants;

/// <summary>
/// 应用程序常量定义
/// </summary>
public static class AppConstants
{
    /// <summary>应用名称</summary>
    public const string AppName = "SmartUninstaller";
    /// <summary>应用版本</summary>
    public const string AppVersion = "1.0.0";
    /// <summary>数据库文件名</summary>
    public const string DatabaseFileName = "SmartUninstaller.db";
    /// <summary>日志文件名</summary>
    public const string LogFileName = "SmartUninstaller.log";
    /// <summary>AI模型文件名</summary>
    public const string AIModelFileName = "leftover_model.zip";
    /// <summary>最大并发扫描线程数</summary>
    public const int MaxScanThreads = 4;
    /// <summary>默认卸载超时时间（秒）</summary>
    public const int DefaultUninstallTimeoutSeconds = 300;
    /// <summary>注册表卸载路径（HKLM）</summary>
    public const string RegistryUninstallPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
    /// <summary>注册表卸载路径（WOW6432Node）</summary>
    public const string RegistryUninstallPathWow64 = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
    /// <summary>临时文件最大保留天数</summary>
    public const int TempFileMaxRetentionDays = 30;
    /// <summary>备份文件最大保留天数</summary>
    public const int BackupMaxRetentionDays = 90;
}
