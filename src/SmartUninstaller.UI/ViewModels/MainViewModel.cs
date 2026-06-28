using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartUninstaller.Core.Interfaces;
using SmartUninstaller.UI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace SmartUninstaller.UI.ViewModels;

/// <summary>
/// 主视图模型 - 管理导航和全局状态
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

    [RelayCommand]
    private async Task NavigateToInstalledAsync()
    {
        IsLoading = true;
        StatusMessage = "正在加载已安装软件...";

        try
        {
            var viewModel = new InstalledSoftwareViewModel(_uninstallService, _scanService);
            await viewModel.LoadSoftwareAsync();
            CurrentView = new InstalledSoftwareView { DataContext = viewModel };
            StatusMessage = $"已加载 {viewModel.SoftwareCount} 个软件";
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

    [RelayCommand]
    private async Task NavigateToPortableAsync()
    {
        IsLoading = true;
        StatusMessage = "正在扫描便携软件...";

        try
        {
            var viewModel = new PortableSoftwareViewModel(_uninstallService, _scanService);
            CurrentView = new PortableSoftwareView { DataContext = viewModel };
            await viewModel.ScanCommand.ExecuteAsync(null);
            StatusMessage = $"已发现 {viewModel.SoftwareCount} 个便携软件";
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

    [RelayCommand]
    private async Task NavigateToLeftoversAsync()
    {
        IsLoading = true;
        StatusMessage = "正在扫描系统残留...";

        try
        {
            var viewModel = new LeftoversViewModel(_scanService, _cleanupService);
            CurrentView = new LeftoversView { DataContext = viewModel };
            StatusMessage = "残留清理就绪，点击扫描按钮开始检测";
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

    [RelayCommand]
    private async Task NavigateToExtensionsAsync()
    {
        IsLoading = true;
        StatusMessage = "正在扫描浏览器扩展...";

        try
        {
            var viewModel = new BrowserExtensionsViewModel(_scanService);
            await viewModel.ScanExtensionsAsync();
            CurrentView = new BrowserExtensionsView { DataContext = viewModel };
            StatusMessage = $"已发现 {viewModel.ExtensionCount} 个浏览器扩展";
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

    [RelayCommand]
    private async Task NavigateToWindowsAppsAsync()
    {
        IsLoading = true;
        StatusMessage = "正在扫描Windows应用...";

        try
        {
            var viewModel = new WindowsAppsViewModel(_uninstallService, _scanService);
            await viewModel.ScanWindowsAppsAsync();
            CurrentView = new WindowsAppsView { DataContext = viewModel };
            StatusMessage = $"已发现 {viewModel.AppCount} 个Windows应用";
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

    [RelayCommand]
    private async Task NavigateToBackupAsync()
    {
        IsLoading = true;
        StatusMessage = "正在加载备份列表...";

        try
        {
            var viewModel = new BackupViewModel(_backupService);
            await viewModel.LoadBackupsAsync();
            CurrentView = new BackupView { DataContext = viewModel };
            StatusMessage = $"已加载 {viewModel.BackupCount} 个备份";
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

    [RelayCommand]
    private async Task NavigateToCleanupAsync()
    {
        IsLoading = true;
        StatusMessage = "正在分析系统...";

        try
        {
            var viewModel = new SystemCleanupViewModel(_cleanupService);
            await viewModel.AnalyzeSystemAsync();
            CurrentView = new SystemCleanupView { DataContext = viewModel };
            StatusMessage = $"系统分析完成，可清理 {viewModel.TotalCleanableSize / 1024 / 1024} MB";
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

    [RelayCommand]
    private void OpenSettings()
    {
        MessageBox.Show(
            "SmartUninstaller 设置\n\n" +
            "功能设置将在后续版本中提供。\n" +
            "当前版本使用默认设置运行。",
            "设置",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    [RelayCommand]
    private void OpenAbout()
    {
        MessageBox.Show(
            "SmartUninstaller v1.0.0\n\n" +
            "一款创新的Windows卸载工具\n" +
            "• 智能便携软件管理\n" +
            "• AI驱动的残留识别\n" +
            "• 损坏软件救援模式\n" +
            "• 浏览器扩展管理\n" +
            "• Windows应用管理\n" +
            "• 系统清理优化\n\n" +
            "GitHub: https://github.com/jasoft/SmartUninstaller\n\n" +
            "© 2026 SmartUninstaller",
            "关于",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
