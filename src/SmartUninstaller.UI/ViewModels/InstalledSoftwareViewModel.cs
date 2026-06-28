using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartUninstaller.Core.Interfaces;
using SmartUninstaller.Core.Models;

namespace SmartUninstaller.UI.ViewModels;

/// <summary>
/// 可选择的软件信息包装类
/// </summary>
public class SelectableSoftware : INotifyPropertyChanged
{
    private bool _isSelected;
    /// <summary>是否被选中</summary>
    public bool IsSelected { get => _isSelected; set { _isSelected = value; PropertyChanged?.Invoke(this, new(nameof(IsSelected))); } }
    /// <summary>软件信息</summary>
    public SoftwareInfo Software { get; set; } = new();
    /// <summary>软件名称</summary>
    public string Name => Software.Name;
    /// <summary>版本</summary>
    public string? Version => Software.Version;
    /// <summary>发布者</summary>
    public string? Publisher => Software.Publisher;
    /// <summary>大小</summary>
    public long Size => Software.Size;
    /// <summary>安装日期</summary>
    public DateTime? InstallDate => Software.InstallDate;
    /// <summary>是否便携</summary>
    public bool IsPortable => Software.IsPortable;
    /// <summary>是否运行中</summary>
    public bool IsRunning => Software.IsRunning;

    public event PropertyChangedEventHandler? PropertyChanged;
}

/// <summary>
/// 已安装软件视图模型
/// </summary>
public partial class InstalledSoftwareViewModel : ObservableObject
{
    private readonly IUninstallService _uninstallService;
    private readonly IScanService _scanService;

    public ObservableCollection<SelectableSoftware> SoftwareList { get; } = [];

    [ObservableProperty]
    private ObservableCollection<SelectableSoftware> _filteredSoftwareList = [];

    [ObservableProperty]
    private SelectableSoftware? _selectedSoftware;

    [ObservableProperty]
    private string _searchText = "";

    [ObservableProperty]
    private bool _isSearchEmpty = true;

    [ObservableProperty]
    private int _softwareCount;

    [ObservableProperty]
    private int _selectedCount;

    [ObservableProperty]
    private string _totalSize = "0 B";

    public InstalledSoftwareViewModel(IUninstallService uninstallService, IScanService scanService)
    {
        _uninstallService = uninstallService;
        _scanService = scanService;
    }

    /// <summary>
    /// 加载软件列表
    /// </summary>
    public async Task LoadSoftwareAsync()
    {
        SoftwareList.Clear();
        var softwareList = await _scanService.ScanInstalledSoftwareAsync();

        foreach (var software in softwareList.OrderBy(s => s.Name))
        {
            SoftwareList.Add(new SelectableSoftware { Software = software });
        }

        FilteredSoftwareList = new ObservableCollection<SelectableSoftware>(SoftwareList);
        SoftwareCount = SoftwareList.Count;
        UpdateTotalSize();
    }

    partial void OnSearchTextChanged(string value)
    {
        IsSearchEmpty = string.IsNullOrWhiteSpace(value);
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            FilteredSoftwareList = new ObservableCollection<SelectableSoftware>(SoftwareList);
        }
        else
        {
            var filtered = SoftwareList.Where(s =>
                s.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (s.Publisher?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
            FilteredSoftwareList = new ObservableCollection<SelectableSoftware>(filtered);
        }
        SoftwareCount = FilteredSoftwareList.Count;
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadSoftwareAsync();
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var item in FilteredSoftwareList) item.IsSelected = true;
        UpdateSelectedCount();
    }

    [RelayCommand]
    private void DeselectAll()
    {
        foreach (var item in FilteredSoftwareList) item.IsSelected = false;
        UpdateSelectedCount();
    }

    [RelayCommand]
    private async Task UninstallSelectedAsync()
    {
        var selected = SoftwareList.Where(s => s.IsSelected).ToList();
        if (selected.Count == 0)
        {
            MessageBox.Show("请先选择要卸载的软件", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var confirm = MessageBox.Show(
            $"确定要卸载选中的 {selected.Count} 个软件吗？\n\n" +
            string.Join("\n", selected.Select(s => $"• {s.Name}")),
            "确认卸载",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) return;

        var options = new UninstallOptions
        {
            CreateRestorePoint = true,
            DeepClean = true,
            SilentUninstall = true
        };

        var successCount = 0;
        var failedCount = 0;

        foreach (var item in selected)
        {
            try
            {
                var result = await _uninstallService.UninstallAsync(item.Software, options);
                if (result.Success)
                {
                    successCount++;
                    SoftwareList.Remove(item);
                }
                else
                {
                    failedCount++;
                    MessageBox.Show($"卸载 {item.Name} 失败:\n{result.ErrorMessage}", "卸载失败", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                failedCount++;
                MessageBox.Show($"卸载 {item.Name} 异常:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        ApplyFilter();
        MessageBox.Show($"卸载完成：成功 {successCount}，失败 {failedCount}", "结果", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private async Task ForceUninstallAsync()
    {
        if (SelectedSoftware == null)
        {
            MessageBox.Show("请先选择要强制卸载的软件", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var confirm = MessageBox.Show(
            $"确定要强制卸载 {SelectedSoftware.Name} 吗？\n\n警告：强制卸载会直接删除文件和注册表项，可能导致系统不稳定。",
            "确认强制卸载",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        var options = new UninstallOptions
        {
            ForceUninstall = true,
            DeepClean = true,
            CreateRestorePoint = true
        };

        var result = await _uninstallService.ForceUninstallAsync(SelectedSoftware.Software, options);
        if (result.Success)
        {
            SoftwareList.Remove(SelectedSoftware);
            ApplyFilter();
            MessageBox.Show($"强制卸载成功！\n删除了 {result.FilesDeleted} 个文件，释放了 {result.SpaceFreed / 1024 / 1024} MB 空间",
                "成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show($"强制卸载失败:\n{result.ErrorMessage}", "失败", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ViewDetails()
    {
        if (SelectedSoftware == null) return;
        var s = SelectedSoftware.Software;
        MessageBox.Show(
            $"名称: {s.Name}\n" +
            $"版本: {s.Version}\n" +
            $"发布者: {s.Publisher}\n" +
            $"安装路径: {s.InstallPath}\n" +
            $"卸载命令: {s.UninstallString}\n" +
            $"注册表路径: {s.RegistryPath}\n" +
            $"大小: {s.Size / 1024 / 1024} MB\n" +
            $"便携软件: {(s.IsPortable ? "是" : "否")}\n" +
            $"运行中: {(s.IsRunning ? "是" : "否")}",
            $"软件详情 - {s.Name}",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    [RelayCommand]
    private void OpenInstallPath()
    {
        if (SelectedSoftware?.Software?.InstallPath == null) return;
        try
        {
            System.Diagnostics.Process.Start("explorer.exe", SelectedSoftware.Software.InstallPath);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"打开目录失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task ScanLeftoversAsync()
    {
        if (SelectedSoftware == null) return;
        var leftovers = await _scanService.ScanLeftoversAsync(SelectedSoftware.Software);
        var list = leftovers.ToList();
        if (list.Count == 0)
        {
            MessageBox.Show($"未发现 {SelectedSoftware.Name} 的残留文件", "扫描结果", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            var message = $"发现 {list.Count} 个残留项：\n\n" +
                string.Join("\n", list.Take(20).Select(l => $"• [{l.Type}] {l.Path} ({l.Size / 1024} KB)"));
            if (list.Count > 20) message += $"\n...还有 {list.Count - 20} 项";
            MessageBox.Show(message, $"残留扫描 - {SelectedSoftware.Name}", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void UpdateTotalSize()
    {
        var total = SoftwareList.Sum(s => s.Size);
        TotalSize = $"{total / 1024 / 1024 / 1024.0:0.##} GB";
    }

    private void UpdateSelectedCount()
    {
        SelectedCount = SoftwareList.Count(s => s.IsSelected);
    }
}
