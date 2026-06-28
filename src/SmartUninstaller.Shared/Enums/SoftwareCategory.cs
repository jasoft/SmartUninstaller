namespace SmartUninstaller.Shared.Enums;

/// <summary>
/// 软件分类枚举
/// </summary>
public enum SoftwareCategory
{
    Application,
    Game,
    Utility,
    Driver,
    Plugin,
    Component,
    Other
}

/// <summary>
/// 软件架构枚举
/// </summary>
public enum SoftwareArchitecture
{
    X86,
    X64,
    AnyCPU,
    ARM,
    ARM64
}

/// <summary>
/// 软件状态枚举
/// </summary>
public enum SoftwareStatus
{
    Installed,
    Portable,
    Damaged,
    PartiallyInstalled,
    Unknown
}

/// <summary>
/// 卸载状态枚举
/// </summary>
public enum UninstallStatus
{
    Pending,
    InProgress,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
/// 清理类型枚举
/// </summary>
public enum CleanupType
{
    TemporaryFiles,
    WindowsUpdate,
    RecycleBin,
    BrowserCache,
    SystemLogs,
    OldDrivers,
    Other
}
