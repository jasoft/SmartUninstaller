namespace SmartUninstaller.Core.Models;

/// <summary>
/// 扫描结果模型
/// </summary>
public class ScanResult
{
    /// <summary>扫描开始时间</summary>
    public DateTime StartTime { get; set; }
    /// <summary>扫描结束时间</summary>
    public DateTime EndTime { get; set; }
    /// <summary>扫描耗时</summary>
    public TimeSpan Duration => EndTime - StartTime;
    /// <summary>发现的软件列表</summary>
    public List<SoftwareInfo> SoftwareList { get; set; } = new();
    /// <summary>发现的残留列表</summary>
    public List<LeftoverInfo> Leftovers { get; set; } = new();
    /// <summary>扫描阶段</summary>
    public string CurrentStage { get; set; } = string.Empty;
    /// <summary>是否已完成</summary>
    public bool IsCompleted { get; set; }
    /// <summary>是否有错误</summary>
    public bool HasError { get; set; }
    /// <summary>错误信息</summary>
    public string? ErrorMessage { get; set; }
}
