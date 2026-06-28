using SmartUninstaller.Core.Models;

namespace SmartUninstaller.Core.Interfaces;

/// <summary>
/// 卸载服务接口
/// </summary>
public interface IUninstallService
{
    /// <summary>卸载软件</summary>
    Task<UninstallResult> UninstallAsync(SoftwareInfo software, UninstallOptions options);
    /// <summary>强制卸载软件</summary>
    Task<UninstallResult> ForceUninstallAsync(SoftwareInfo software, UninstallOptions options);
    /// <summary>批量卸载软件</summary>
    Task<BatchUninstallResult> BatchUninstallAsync(IEnumerable<SoftwareInfo> softwareList, UninstallOptions options);
    /// <summary>检查软件是否可以卸载</summary>
    Task<bool> CanUninstallAsync(SoftwareInfo software);
    /// <summary>获取卸载预估信息</summary>
    Task<UninstallEstimate> GetUninstallEstimateAsync(SoftwareInfo software);
}

/// <summary>
/// 卸载选项
/// </summary>
public class UninstallOptions
{
    /// <summary>是否创建系统还原点</summary>
    public bool CreateRestorePoint { get; set; } = true;
    /// <summary>是否执行深度清理</summary>
    public bool DeepClean { get; set; } = true;
    /// <summary>是否清理注册表</summary>
    public bool CleanRegistry { get; set; } = true;
    /// <summary>是否备份重要数据</summary>
    public bool BackupImportantData { get; set; } = true;
    /// <summary>是否强制卸载</summary>
    public bool ForceUninstall { get; set; } = false;
    /// <summary>是否静默卸载</summary>
    public bool SilentUninstall { get; set; } = false;
    /// <summary>超时时间（秒）</summary>
    public int TimeoutSeconds { get; set; } = 300;
}

/// <summary>
/// 卸载结果
/// </summary>
public class UninstallResult
{
    /// <summary>是否成功</summary>
    public bool Success { get; set; }
    /// <summary>卸载的软件信息</summary>
    public SoftwareInfo? Software { get; set; }
    /// <summary>卸载开始时间</summary>
    public DateTime StartTime { get; set; }
    /// <summary>卸载结束时间</summary>
    public DateTime EndTime { get; set; }
    /// <summary>卸载耗时</summary>
    public TimeSpan Duration => EndTime - StartTime;
    /// <summary>删除的文件数量</summary>
    public int FilesDeleted { get; set; }
    /// <summary>删除的注册表项数量</summary>
    public int RegistryEntriesDeleted { get; set; }
    /// <summary>释放的空间（字节）</summary>
    public long SpaceFreed { get; set; }
    /// <summary>残留项数量</summary>
    public int LeftoverCount { get; set; }
    /// <summary>错误信息</summary>
    public string? ErrorMessage { get; set; }
    /// <summary>详细日志</summary>
    public List<string> Logs { get; set; } = [];
}

/// <summary>
/// 批量卸载结果
/// </summary>
public class BatchUninstallResult
{
    /// <summary>总软件数量</summary>
    public int TotalCount { get; set; }
    /// <summary>成功卸载数量</summary>
    public int SuccessCount { get; set; }
    /// <summary>失败卸载数量</summary>
    public int FailedCount { get; set; }
    /// <summary>各软件卸载结果</summary>
    public List<UninstallResult> Results { get; set; } = [];
    /// <summary>总释放空间</summary>
    public long TotalSpaceFreed { get; set; }
    /// <summary>总耗时</summary>
    public TimeSpan TotalDuration { get; set; }
}

/// <summary>
/// 卸载预估信息
/// </summary>
public class UninstallEstimate
{
    /// <summary>预估释放空间（字节）</summary>
    public long EstimatedSpaceFreed { get; set; }
    /// <summary>预估耗时（秒）</summary>
    public int EstimatedDurationSeconds { get; set; }
    /// <summary>预估删除文件数量</summary>
    public int EstimatedFilesToDelete { get; set; }
    /// <summary>预估删除注册表项数量</summary>
    public int EstimatedRegistryEntriesToDelete { get; set; }
    /// <summary>风险等级</summary>
    public RiskLevel RiskLevel { get; set; }
    /// <summary>风险说明</summary>
    public string? RiskDescription { get; set; }
}

/// <summary>
/// 清理结果
/// </summary>
public class CleanupResult
{
    /// <summary>删除的文件数量</summary>
    public int FilesDeleted { get; set; }
    /// <summary>删除的注册表项数量</summary>
    public int RegistryEntriesDeleted { get; set; }
    /// <summary>释放的空间（字节）</summary>
    public long SpaceFreed { get; set; }
}
