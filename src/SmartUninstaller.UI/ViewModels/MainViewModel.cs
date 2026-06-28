using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartUninstaller.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace SmartUninstaller.UI.ViewModels;

/// <summary>
/// 主视图模型
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IUninstallService _uninstallService;
    private readonly IScanService _scanService;
    private readonly ICleanupService _cleanupService;
    private readonly IBackupService _backupService;

    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private string _statusMessage = "就绪";

    [ObservableProperty]
    private double _progressValue;

    [ObservableProperty]
    private bool _isProgressVisible;

    [ObservableProperty]
    private bool _isLoading;

    public MainViewModel()
    {
        var sp = App.ServiceProvider;
        _uninstallService = sp.GetRequiredService<IUninstallService>();
        _scanService = sp.GetRequiredService<IScanService>();
        _cleanupService = sp.GetRequiredService<ICleanupService>();
        _backupService = sp.GetRequiredService<IBackupService>();
    }

    /// <summary>
    /// 导航到已安装软件视图
    /// </summary>
    [RelayCommand]
    private async Task NavigateToInstalledAsync()
    {
        IsLoading = true;
        StatusMessage = "正在加载已安装软件...";

        try
        {
            var softwareList = await _scanService.ScanInstalledSoftwareAsync();
            CurrentView = new System.Windows.Controls.TextBlock
            {
                Text = $"已加载 {softwareList.Count()} 个软件\n\n" +
                       string.Join("\n", softwareList.Take(20).Select(s => $"• {s.Name} {s.Version}")),
                FontSize = 14,
                Margin = new Thickness(20),
                TextWrapping = TextWrapping.Wrap
            };
            StatusMessage = $"已加载 {softwareList.Count()} 个软件";
        }
        catch (Exception ex)
        {
            StatusMessage = $"加载失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 导航到便携软件视图
    /// </summary>
    [RelayCommand]
    private async Task NavigateToPortableAsync()
    {
        IsLoading = true;
        StatusMessage = "正在扫描便携软件...";

        try
        {
            var softwareList = await _scanService.ScanPortableSoftwareAsync();
            CurrentView = new System.Windows.Controls.TextBlock
            {
                Text = $"已发现 {softwareList.Count()} 个便携软件\n\n" +
                       string.Join("\n", softwareList.Select(s => $"• {s.Name} - {s.InstallPath}")),
                FontSize = 14,
                Margin = new Thickness(20),
                TextWrapping = TextWrapping.Wrap
            };
            StatusMessage = $"已发现 {softwareList.Count()} 个便携软件";
        }
        catch (Exception ex)
        {
            StatusMessage = $"扫描失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 导航到残留清理视图
    /// </summary>
    [RelayCommand]
    private async Task NavigateToLeftoversAsync()
    {
        IsLoading = true;
        StatusMessage = "正在扫描系统残留...";
        try
        {
            var leftovers = await _scanService.ScanSystemLeftoversAsync();
            CurrentView = new System.Windows.Controls.TextBlock
            {
                Text = $"已发现 {leftovers.Count()} 个残留项",
                FontSize = 14,
                Margin = new Thickness(20)
            };
            StatusMessage = $"已发现 {leftovers.Count()} 个残留项";
        }
        catch (Exception ex)
        {
            StatusMessage = $"扫描失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 导航到浏览器扩展视图
    /// </summary>
    [RelayCommand]
    private async Task NavigateToExtensionsAsync()
    {
        IsLoading = true;
        StatusMessage = "正在扫描浏览器扩展...";
        try
        {
            var extensions = await _scanService.ScanBrowserExtensionsAsync();
            CurrentView = new System.Windows.Controls.TextBlock
            {
                Text = $"已发现 {extensions.Count()} 个扩展",
                FontSize = 14,
                Margin = new Thickness(20)
            };
            StatusMessage = $"已发现 {extensions.Count()} 个扩展";
        }
        catch (Exception ex)
        {
            StatusMessage = $"扫描失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 导航到Windows应用视图
    /// </summary>
    [RelayCommand]
    private async Task NavigateToWindowsAppsAsync()
    {
        IsLoading = true;
        StatusMessage = "正在扫描Windows应用...";
        try
        {
            var apps = await _scanService.ScanWindowsAppsAsync();
            CurrentView = new System.Windows.Controls.TextBlock
            {
                Text = $"已发现 {apps.Count()} 个Windows应用",
                FontSize = 14,
                Margin = new Thickness(20)
            };
            StatusMessage = $"已发现 {apps.Count()} 个Windows应用";
        }
        catch (Exception ex)
        {
            StatusMessage = $"扫描失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 导航到备份管理视图
    /// </summary>
    [RelayCommand]
    private async Task NavigateToBackupAsync()
    {
        IsLoading = true;
        StatusMessage = "正在加载备份列表...";
        try
        {
            var backups = await _backupService.GetAllBackupsAsync();
            CurrentView = new System.Windows.Controls.TextBlock
            {
                Text = $"已加载 {backups.Count()} 个备份",
                FontSize = 14,
                Margin = new Thickness(20)
            };
            StatusMessage = $"已加载 {backups.Count()} 个备份";
        }
        catch (Exception ex)
        {
            StatusMessage = $"加载失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 导航到系统清理视图
    /// </summary>
    [RelayCommand]
    private async Task NavigateToCleanupAsync()
    {
        IsLoading = true;
        StatusMessage = "正在分析系统...";
        try
        {
            var info = await _cleanupService.AnalyzeSystemAsync();
            CurrentView = new System.Windows.Controls.TextBlock
            {
                Text = $"可清理空间: {info.TotalCleanableSize / 1024 / 1024} MB\n" +
                       $"临时文件: {info.TempFilesSize / 1024 / 1024} MB",
                FontSize = 14,
                Margin = new Thickness(20)
            };
            StatusMessage = "系统分析完成";
        }
        catch (Exception ex)
        {
            StatusMessage = $"分析失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 打开设置
    /// </summary>
    [RelayCommand]
    private void OpenSettings()
    {
        MessageBox.Show("设置功能即将推出", "设置", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// 打开关于
    /// </summary>
    [RelayCommand]
    private void OpenAbout()
    {
        MessageBox.Show(
            "SmartUninstaller v1.0.0\n\n" +
            "一款创新的Windows卸载工具\n" +
            "具有智能便携软件管理、AI驱动的残留识别等核心特性\n\n" +
            "© 2026 SmartUninstaller",
            "关于",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
