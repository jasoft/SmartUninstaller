using SmartUninstaller.Core.Models;

namespace SmartUninstaller.Core.Interfaces;

/// <summary>
/// 扫描服务接口
/// </summary>
public interface IScanService
{
    /// <summary>扫描系统中的已安装软件</summary>
    Task<IEnumerable<SoftwareInfo>> ScanInstalledSoftwareAsync();
    /// <summary>扫描便携软件</summary>
    Task<IEnumerable<SoftwareInfo>> ScanPortableSoftwareAsync();
    /// <summary>扫描软件残留</summary>
    Task<IEnumerable<LeftoverInfo>> ScanLeftoversAsync(SoftwareInfo software);
    /// <summary>扫描系统残留</summary>
    Task<IEnumerable<LeftoverInfo>> ScanSystemLeftoversAsync();
    /// <summary>扫描浏览器扩展</summary>
    Task<IEnumerable<BrowserExtension>> ScanBrowserExtensionsAsync();
    /// <summary>扫描Windows应用</summary>
    Task<IEnumerable<SoftwareInfo>> ScanWindowsAppsAsync();
    /// <summary>搜索软件</summary>
    Task<IEnumerable<SoftwareInfo>> SearchSoftwareAsync(string keyword);
    /// <summary>获取软件详细信息</summary>
    Task<SoftwareInfo?> GetSoftwareDetailsAsync(string softwareId);
    /// <summary>获取扫描进度</summary>
    ScanProgress GetScanProgress();
    /// <summary>取消扫描</summary>
    void CancelScan();
}

/// <summary>
/// 扫描进度
/// </summary>
public class ScanProgress
{
    /// <summary>当前阶段</summary>
    public ScanStage Stage { get; set; }
    /// <summary>进度百分比</summary>
    public int ProgressPercentage { get; set; }
    /// <summary>当前扫描项</summary>
    public string? CurrentItem { get; set; }
    /// <summary>已扫描数量</summary>
    public int ScannedCount { get; set; }
    /// <summary>总数量</summary>
    public int TotalCount { get; set; }
    /// <summary>是否完成</summary>
    public bool IsCompleted { get; set; }
    /// <summary>是否取消</summary>
    public bool IsCancelled { get; set; }
}

/// <summary>
/// 扫描阶段枚举
/// </summary>
public enum ScanStage
{
    Initializing, ScanningRegistry, ScanningFileSystem, ScanningWMI,
    ScanningProcesses, ScanningPortableSoftware, ScanningBrowserExtensions,
    ScanningWindowsApps, AnalyzingResults, Completed
}

/// <summary>
/// 浏览器扩展信息
/// </summary>
public class BrowserExtension
{
    /// <summary>扩展ID</summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>扩展名称</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>浏览器类型</summary>
    public BrowserType Browser { get; set; }
    /// <summary>扩展版本</summary>
    public string? Version { get; set; }
    /// <summary>扩展描述</summary>
    public string? Description { get; set; }
    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; }
    /// <summary>安装日期</summary>
    public DateTime? InstallDate { get; set; }
    /// <summary>评分</summary>
    public double Rating { get; set; }
    /// <summary>是否可疑</summary>
    public bool IsSuspicious { get; set; }
}

/// <summary>
/// 浏览器类型枚举
/// </summary>
public enum BrowserType
{
    Chrome, Firefox, Edge, Opera, Brave, Other
}
