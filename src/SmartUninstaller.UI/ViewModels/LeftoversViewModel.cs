using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartUninstaller.Core.Interfaces;
using SmartUninstaller.Core.Models;

namespace SmartUninstaller.UI.ViewModels;

/// <summary>
/// Selectable leftover info wrapper
/// </summary>
public class SelectableLeftover : INotifyPropertyChanged
{
    private bool _isSelected;
    /// <summary>Whether selected</summary>
    public bool IsSelected { get => _isSelected; set { _isSelected = value; PropertyChanged?.Invoke(this, new(nameof(IsSelected))); } }
    /// <summary>Leftover info</summary>
    public LeftoverInfo Leftover { get; set; } = new();
    /// <summary>Software name</summary>
    public string SoftwareName => Leftover.SoftwareName;
    /// <summary>Type</summary>
    public LeftoverType Type => Leftover.Type;
    /// <summary>Path</summary>
    public string Path => Leftover.Path;
    /// <summary>Size</summary>
    public long Size => Leftover.Size;
    /// <summary>Confidence score</summary>
    public double ConfidenceScore => Leftover.ConfidenceScore;
    /// <summary>Risk description</summary>
    public string? RiskDescription => Leftover.RiskDescription;

    public event PropertyChangedEventHandler? PropertyChanged;
}

/// <summary>
/// Leftovers cleanup view model
/// </summary>
public partial class LeftoversViewModel : ObservableObject
{
    private readonly IScanService _scanService;
    private readonly ICleanupService _cleanupService;

    public ObservableCollection<SelectableLeftover> LeftoverList { get; } = new();

    [ObservableProperty]
    private SelectableLeftover? _selectedLeftover;

    [ObservableProperty]
    private int _leftoverCount;

    [ObservableProperty]
    private string _totalSizeText = "Total: 0 B";

    [ObservableProperty]
    private string _statusMessage = "Click Scan to detect leftovers";

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private double _progressValue;

    public LeftoversViewModel(IScanService scanService, ICleanupService cleanupService)
    {
        _scanService = scanService;
        _cleanupService = cleanupService;
    }

    [RelayCommand]
    private async Task ScanAsync()
    {
        IsScanning = true;
        StatusMessage = "Scanning system leftovers...";
        LeftoverList.Clear();

        try
        {
            var leftovers = await _scanService.ScanSystemLeftoversAsync();
            foreach (var l in leftovers)
            {
                LeftoverList.Add(new SelectableLeftover { Leftover = l });
            }

            LeftoverCount = LeftoverList.Count;
            var totalSize = LeftoverList.Sum(l => l.Size);
            TotalSizeText = $"Total cleanable: {totalSize / 1024 / 1024.0:0.##} MB";
            StatusMessage = $"Scan complete. Found {LeftoverCount} leftover items.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Scan failed: {ex.Message}";
        }
        finally
        {
            IsScanning = false;
        }
    }

    [RelayCommand]
    private async Task CleanSelectedAsync()
    {
        var selected = LeftoverList.Where(l => l.IsSelected).ToList();
        if (selected.Count == 0)
        {
            MessageBox.Show("Please select leftovers to clean first.", "Hint", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var confirm = MessageBox.Show(
            $"Are you sure to clean {selected.Count} selected leftover items?",
            "Confirm Clean",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) return;

        var cleanedCount = 0;
        var freedSpace = 0L;

        foreach (var item in selected)
        {
            try
            {
                if (item.Leftover.Type == LeftoverType.Folder && Directory.Exists(item.Leftover.Path))
                {
                    Directory.Delete(item.Leftover.Path, true);
                    cleanedCount++;
                    freedSpace += item.Size;
                    LeftoverList.Remove(item);
                }
                else if (item.Leftover.Type == LeftoverType.File && File.Exists(item.Leftover.Path))
                {
                    File.Delete(item.Leftover.Path);
                    cleanedCount++;
                    freedSpace += item.Size;
                    LeftoverList.Remove(item);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Clean failed: {ex.Message}";
            }
        }

        LeftoverCount = LeftoverList.Count;
        MessageBox.Show($"Clean complete: {cleanedCount} items cleaned, freed {freedSpace / 1024 / 1024.0:0.##} MB",
            "Result", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
