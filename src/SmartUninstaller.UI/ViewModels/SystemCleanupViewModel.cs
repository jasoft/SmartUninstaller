using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartUninstaller.Core.Interfaces;
using SmartUninstaller.Shared.Enums;

namespace SmartUninstaller.UI.ViewModels;

/// <summary>
/// 清理项
/// </summary>
public class CleanupItem : INotifyPropertyChanged
{
    private bool _isSelected = true;
    /// <summary>是否选中</summary>
    public bool IsSelected { get => _isSelected; set { _isSelected = value; PropertyChanged?.Invoke(this, new(nameof(IsSelected))); } }
    /// <summary>名称</summary>
    public string Name { get; set; } = "";
    /// <summary>描述</summary>
    public string Description { get; set; } = "";
    /// <summary>大小（字节）</summary>
    public long Size { get; set; }
    /// <summary>清理类型</summary>
    public CleanupType Type { get; set; }
    /// <summary>清理命令（绑定到视图）</summary>
    public IRelayCommand? CleanCommand { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
}

/// <summary>
/// 系统清理视图模型
/// </summary>
public partial class SystemCleanupViewModel : ObservableObject
{
    private readonly ICleanupService _cleanupService;

    public ObservableCollection<CleanupItem> CleanupItems { get; } = [];

    [ObservableProperty]
    private long _totalCleanableSize;

    [ObservableProperty]
    private bool _isAnalyzing;

    [ObservableProperty]
    private double _progressValue;

    public SystemCleanupViewModel(ICleanupService cleanupService)
    {
        _cleanupService = cleanupService;
    }

    /// <summary>
    /// 分析系统可清理项
    /// </summary>
    public async Task AnalyzeSystemAsync()
    {
        IsAnalyzing = true;
        CleanupItems.Clear();

        try
        {
            var info = await _cleanupService.AnalyzeSystemAsync();

            if (info.TempFilesSize > 0)
            {
                CleanupItems.Add(new CleanupItem
                {
                    Name = "临时文件",
                    Description = $"系统临时目录中的文件 ({Path.GetTempPath()})",
                    Size = info.TempFilesSize,
                    Type = CleanupType.TemporaryFiles,
                    CleanCommand = new AsyncRelayCommand(() => CleanItemAsync(CleanupType.TemporaryFiles))
                });
            }

            if (info.WindowsUpdateCacheSize > 0)
            {
                CleanupItems.Add(new CleanupItem
                {
                    Name = "Windows更新缓存",
                    Description = "Windows Update下载的更新安装包缓存",
                    Size = info.WindowsUpdateCacheSize,
                    Type = CleanupType.WindowsUpdate
                });
            }

            if (info.BrowserCacheSize > 0)
            {
                CleanupItems.Add(new CleanupItem
                {
                    Name = "浏览器缓存",
                    Description = "Chrome/Edge/Firefox浏览器缓存文件",
                    Size = info.BrowserCacheSize,
                    Type = CleanupType.BrowserCache
                });
            }

            if (info.SystemLogsSize > 0)
            {
                CleanupItems.Add(new CleanupItem
                {
                    Name = "系统日志",
                    Description = "Windows事件日志和错误报告",
                    Size = info.SystemLogsSize,
                    Type = CleanupType.SystemLogs
                });
            }

            TotalCleanableSize = CleanupItems.Sum(c => c.Size);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"分析失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsAnalyzing = false;
        }
    }

    [RelayCommand]
    private async Task AnalyzeAsync()
    {
        await AnalyzeSystemAsync();
    }

    [RelayCommand]
    private async Task CleanAllAsync()
    {
        var selected = CleanupItems.Where(c => c.IsSelected).ToList();
        if (selected.Count == 0)
        {
            MessageBox.Show("请先选择要清理的项目", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var confirm = MessageBox.Show(
            $"确定要清理选中的 {selected.Count} 个项目吗？\n可释放: {selected.Sum(s => s.Size) / 1024 / 1024.0:0.##} MB",
            "确认清理",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) return;

        var totalFreed = 0L;
        foreach (var item in selected)
        {
            try
            {
                var result = await _cleanupService.CleanSystemAsync(item.Type);
                totalFreed += result.SpaceFreed;
                CleanupItems.Remove(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"清理 {item.Name} 失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        TotalCleanableSize = CleanupItems.Sum(c => c.Size);
        MessageBox.Show($"清理完成！释放了 {totalFreed / 1024 / 1024.0:0.##} MB 空间", "完成", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task CleanItemAsync(CleanupType type)
    {
        try
        {
            var result = await _cleanupService.CleanSystemAsync(type);
            var item = CleanupItems.FirstOrDefault(c => c.Type == type);
            if (item != null) CleanupItems.Remove(item);
            TotalCleanableSize = CleanupItems.Sum(c => c.Size);
            MessageBox.Show($"清理完成，释放了 {result.SpaceFreed / 1024 / 1024.0:0.##} MB", "完成", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"清理失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
