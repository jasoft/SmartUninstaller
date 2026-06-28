namespace SmartUninstaller.Data.Entities;

/// <summary>
/// 卸载历史记录实体
/// </summary>
public class UninstallHistoryEntity
{
    /// <summary>主键</summary>
    public int Id { get; set; }
    /// <summary>软件名称</summary>
    public string SoftwareName { get; set; } = string.Empty;
    /// <summary>软件版本</summary>
    public string? SoftwareVersion { get; set; }
    /// <summary>发布者</summary>
    public string? Publisher { get; set; }
    /// <summary>安装路径</summary>
    public string? InstallPath { get; set; }
    /// <summary>卸载时间</summary>
    public DateTime UninstallTime { get; set; }
    /// <summary>是否成功</summary>
    public bool Success { get; set; }
    /// <summary>删除的文件数量</summary>
    public int FilesDeleted { get; set; }
    /// <summary>删除的注册表项数量</summary>
    public int RegistryEntriesDeleted { get; set; }
    /// <summary>释放的空间（字节）</summary>
    public long SpaceFreed { get; set; }
    /// <summary>耗时（毫秒）</summary>
    public long DurationMs { get; set; }
    /// <summary>错误信息</summary>
    public string? ErrorMessage { get; set; }
    /// <summary>卸载方式</summary>
    public string UninstallMethod { get; set; } = "Normal";
}
