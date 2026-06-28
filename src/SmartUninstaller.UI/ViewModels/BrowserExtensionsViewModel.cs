using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartUninstaller.Core.Interfaces;
using SmartUninstaller.Core.Models;

namespace SmartUninstaller.UI.ViewModels;

/// <summary>
/// 浏览器扩展视图模型
/// </summary>
public partial class BrowserExtensionsViewModel : ObservableObject
{
    private readonly IScanService _scanService;

    public ObservableCollection<BrowserExtension> AllExtensions { get; } = [];

    [ObservableProperty]
    private ObservableCollection<BrowserExtension> _filteredExtensions = [];

    [ObservableProperty]
    private BrowserExtension? _selectedExtension;

    [ObservableProperty]
    private int _extensionCount;

    private BrowserType? _currentFilter;

    public BrowserExtensionsViewModel(IScanService scanService)
    {
        _scanService = scanService;
    }

    /// <summary>
    /// 扫描浏览器扩展
    /// </summary>
    public async Task ScanExtensionsAsync()
    {
        AllExtensions.Clear();
        var extensions = await _scanService.ScanBrowserExtensionsAsync();
        foreach (var ext in extensions)
        {
            AllExtensions.Add(ext);
        }
        ApplyFilter();
    }

    [RelayCommand]
    private async Task ScanAsync()
    {
        await ScanExtensionsAsync();
    }

    [RelayCommand]
    private void FilterAll()
    {
        _currentFilter = null;
        ApplyFilter();
    }

    [RelayCommand]
    private void FilterChrome()
    {
        _currentFilter = BrowserType.Chrome;
        ApplyFilter();
    }

    [RelayCommand]
    private void FilterEdge()
    {
        _currentFilter = BrowserType.Edge;
        ApplyFilter();
    }

    [RelayCommand]
    private void FilterFirefox()
    {
        _currentFilter = BrowserType.Firefox;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var filtered = _currentFilter.HasValue
            ? AllExtensions.Where(e => e.Browser == _currentFilter.Value)
            : AllExtensions.AsEnumerable();

        FilteredExtensions = new ObservableCollection<BrowserExtension>(filtered);
        ExtensionCount = FilteredExtensions.Count;
    }
}
