using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartUninstaller.Core.Interfaces;
using SmartUninstaller.Core.Models;

namespace SmartUninstaller.UI.ViewModels;

/// <summary>
/// 便携软件视图模型
/// </summary>
public partial class PortableSoftwareViewModel : ObservableObject
{
    private readonly IUninstallService _uninstallService;
    private readonly IScanService _scanService;

    public ObservableCollection<SelectableSoftware> SoftwareList { get; } = new();

    [ObservableProperty]
    private SelectableSoftware? _selectedSoftware;

    [ObservableProperty]
    private int _softwareCount;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    public PortableSoftwareViewModel(IUninstallService uninstallService, IScanService scanService)
    {
        _uninstallService = uninstallService;
        _scanService = scanService;
    }

    [RelayCommand]
    private async Task ScanAsync()
    {
        StatusMessage = "Scanning portable software...";
        SoftwareList.Clear();

        var software = await _scanService.ScanPortableSoftwareAsync();
        foreach (var s in software.OrderBy(s => s.Name))
        {
            SoftwareList.Add(new SelectableSoftware { Software = s });
        }

        SoftwareCount = SoftwareList.Count;
        StatusMessage = $"Scan complete. Found {SoftwareCount} portable software.";
    }

    [RelayCommand]
    private async Task DeleteSelectedAsync()
    {
        var selected = SoftwareList.Where(s => s.IsSelected).ToList();
        if (selected.Count == 0)
        {
            MessageBox.Show("Please select portable software to delete first.", "Hint", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var confirm = MessageBox.Show(
            $"Are you sure to delete {selected.Count} selected portable software?\n\n" +
            string.Join("\n", selected.Select(s => $"- {s.Name} - {s.Software.InstallPath}")),
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        foreach (var item in selected)
        {
            try
            {
                if (!string.IsNullOrEmpty(item.Software.InstallPath) && Directory.Exists(item.Software.InstallPath))
                {
                    if (item.Software.IsRunning && item.Software.ProcessId.HasValue)
                    {
                        try { System.Diagnostics.Process.GetProcessById(item.Software.ProcessId.Value).Kill(); } catch { }
                    }
                    Directory.Delete(item.Software.InstallPath, true);
                    SoftwareList.Remove(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete {item.Name}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        SoftwareCount = SoftwareList.Count;
        StatusMessage = $"Delete complete. {SoftwareCount} portable software remaining.";
    }
}
