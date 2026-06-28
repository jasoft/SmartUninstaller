using SmartUninstaller.Shared.Enums;

namespace SmartUninstaller.Core.Models;

/// <summary>
/// 软件信息模型
/// </summary>
public class SoftwareInfo
{
    /// <summary>软件唯一标识</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    /// <summary>软件名称</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>显示名称</summary>
    public string DisplayName { get; set; } = string.Empty;
    /// <summary>软件版本</summary>
    public string? Version { get; set; }
    /// <summary>发布者</summary>
    public string? Publisher { get; set; }
    /// <summary>软件描述</summary>
    public string? Description { get; set; }
    /// <summary>安装路径</summary>
    public string? InstallPath { get; set; }
    /// <summary>卸载命令</summary>
    public string? UninstallString { get; set; }
    /// <summary>注册表路径</summary>
    public string? RegistryPath { get; set; }
    /// <summary>图标路径</summary>
    public string? IconPath { get; set; }
    /// <summary>安装日期</summary>
    public DateTime? InstallDate { get; set; }
    /// <summary>软件大小（字节）</summary>
    public long Size { get; set; }
    /// <summary>是否为便携软件</summary>
    public bool IsPortable { get; set; }
    /// <summary>是否正在运行</summary>
    public bool IsRunning { get; set; }
    /// <summary>关联进程ID</summary>
    public int? ProcessId { get; set; }
    /// <summary>是否已安装</summary>
    public bool IsInstalled { get; set; } = true;
    /// <summary>软件分类</summary>
    public SoftwareCategory Category { get; set; } = SoftwareCategory.Application;
    /// <summary>软件架构</summary>
    public SoftwareArchitecture Architecture { get; set; } = SoftwareArchitecture.AnyCPU;
    /// <summary>软件状态</summary>
    public SoftwareStatus Status { get; set; } = SoftwareStatus.Installed;
    /// <summary>网站URL</summary>
    public string? UrlInfoAbout { get; set; }
    /// <summary>帮助链接</summary>
    public string? HelpLink { get; set; }
}
