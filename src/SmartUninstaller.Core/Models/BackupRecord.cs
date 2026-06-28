namespace SmartUninstaller.Core.Models;

/// <summary>
/// 备份记录模型
/// </summary>
public class BackupRecord
{
    /// <summary>备份唯一标识</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    /// <summary>软件名称</summary>
    public string SoftwareName { get; set; } = string.Empty;
    /// <summary>备份时间</summary>
    public DateTime BackupTime { get; set; } = DateTime.Now;
    /// <summary>备份大小（字节）</summary>
    public long BackupSize { get; set; }
    /// <summary>备份路径</summary>
    public string BackupPath { get; set; } = string.Empty;
    /// <summary>备份描述</summary>
    public string? Description { get; set; }
    /// <summary>是否已恢复</summary>
    public bool IsRestored { get; set; }
    /// <summary>备份类型</summary>
    public BackupType Type { get; set; } = BackupType.Full;
}

/// <summary>
/// 备份类型枚举
/// </summary>
public enum BackupType
{
    Full, FilesOnly, RegistryOnly, SettingsOnly
}
