namespace SmartUninstaller.Core.Models;

/// <summary>
/// 卸载记录模型
/// </summary>
public class UninstallRecord
{
    /// <summary>记录唯一标识</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    /// <summary>软件名称</summary>
    public string SoftwareName { get; set; } = string.Empty;
    /// <summary>软件版本</summary>
    public string? SoftwareVersion { get; set; }
    /// <summary>卸载时间</summary>
    public DateTime UninstallTime { get; set; } = DateTime.Now;
    /// <summary>是否成功</summary>
    public bool Success { get; set; }
    /// <summary>删除的文件数量</summary>
    public int FilesDeleted { get; set; }
    /// <summary>删除的注册表项数量</summary>
    public int RegistryEntriesDeleted { get; set; }
    /// <summary>释放的空间（字节）</summary>
    public long SpaceFreed { get; set; }
    /// <summary>错误信息</summary>
    public string? ErrorMessage { get; set; }
    /// <summary>详细日志</summary>
    public string? DetailedLog { get; set; }
    /// <summary>是否已备份</summary>
    public bool IsBackedUp { get; set; }
    /// <summary>备份路径</summary>
    public string? BackupPath { get; set; }
}
