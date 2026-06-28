using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartUninstaller.Core.Interfaces;
using SmartUninstaller.Core.Models;

namespace SmartUninstaller.UI.ViewModels;

/// <summary>
/// Windows应用视图模型
/// </summary>
public partial class WindowsAppsViewModel : ObservableObject
{
    private readonly IUninstallService _uninstallService;
    private readonly IScanService _scanService;

    public ObservableCollection<SelectableSoftware> AppList { get; } = [];

    [ObservableProperty]
    private SelectableSoftware? _selectedApp;

    [ObservableProperty]
    private int _appCount;

    public WindowsAppsViewModel(IUninstallService uninstallService, IScanService scanService)
    {
        _uninstallService = uninstallService;
        _scanService = scanService;
    }

    public async Task ScanWindowsAppsAsync()
    {
        AppList.Clear();
        var apps = await _scanService.ScanWindowsAppsAsync();
        foreach (var app in apps.OrderBy(a => a.Name))
        {
            AppList.Add(new SelectableSoftware { Software = app });
        }
        AppCount = AppList.Count;
    }

    [RelayCommand]
    private async Task ScanAsync()
    {
        await ScanWindowsAppsAsync();
    }

    [RelayCommand]
    private async Task UninstallSelectedAsync()
    {
        var selected = AppList.Where(a => a.IsSelected).ToList();
        if (selected.Count == 0)
        {
            MessageBox.Show("请先选择要卸载的应用", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var confirm = MessageBox.Show(
            $"确定要卸载选中的 {selected.Count} 个Windows应用吗？\n\n" +
            string.Join("\n", selected.Select(s => $"• {s.Name}")),
            "确认卸载",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) return;

        var successCount = 0;
        foreach (var item in selected)
        {
            try
            {
                // 使用PowerShell卸载AppX包
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -NonInteractive -Command \"Get-AppxPackage -Name '{item.Name}' | Remove-AppxPackage\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var process = System.Diagnostics.Process.Start(psi);
                if (process != null)
                {
                    await process.WaitForExitAsync();
                    if (process.ExitCode == 0)
                    {
                        successCount++;
                        AppList.Remove(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"卸载 {item.Name} 失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        AppCount = AppList.Count;
        MessageBox.Show($"卸载完成：成功 {successCount} 个", "结果", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
