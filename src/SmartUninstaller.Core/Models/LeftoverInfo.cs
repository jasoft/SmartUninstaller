using SmartUninstaller.Shared.Enums;

namespace SmartUninstaller.Core.Models;

/// <summary>
/// 残留信息模型
/// </summary>
public class LeftoverInfo
{
    /// <summary>残留项唯一标识</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    /// <summary>关联软件名称</summary>
    public string SoftwareName { get; set; } = string.Empty;
    /// <summary>残留项路径</summary>
    public string Path { get; set; } = string.Empty;
    /// <summary>残留类型</summary>
    public LeftoverType Type { get; set; }
    /// <summary>大小（字节）</summary>
    public long Size { get; set; }
    /// <summary>置信度分数（0-1）</summary>
    public double ConfidenceScore { get; set; }
    /// <summary>风险等级</summary>
    public RiskLevel RiskLevel { get; set; }
    /// <summary>风险说明</summary>
    public string? RiskDescription { get; set; }
    /// <summary>是否可安全删除</summary>
    public bool IsSafeToDelete { get; set; }
    /// <summary>建议操作</summary>
    public string? RecommendedAction { get; set; }
    /// <summary>详细信息</summary>
    public string? Details { get; set; }
    /// <summary>创建时间</summary>
    public DateTime? CreatedTime { get; set; }
    /// <summary>修改时间</summary>
    public DateTime? ModifiedTime { get; set; }
    /// <summary>记录创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// 残留类型枚举
/// </summary>
public enum LeftoverType
{
    File, Folder, RegistryKey, RegistryValue, Service,
    ScheduledTask, StartupEntry, BrowserExtension, COMObject, Driver, Other
}

/// <summary>
/// 风险等级枚举
/// </summary>
public enum RiskLevel
{
    Low, Medium, High, Critical
}
