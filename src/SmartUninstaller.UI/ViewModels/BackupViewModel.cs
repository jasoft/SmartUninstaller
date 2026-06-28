using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartUninstaller.Core.Interfaces;
using SmartUninstaller.Core.Models;

namespace SmartUninstaller.UI.ViewModels;

/// <summary>
/// 备份管理视图模型
/// </summary>
public partial class BackupViewModel : ObservableObject
{
    private readonly IBackupService _backupService;

    public ObservableCollection<BackupRecord> BackupList { get; } = new();

    [ObservableProperty]
    private BackupRecord? _selectedBackup;

    [ObservableProperty]
    private int _backupCount;

    public BackupViewModel(IBackupService backupService)
    {
        _backupService = backupService;
    }

    public async Task LoadBackupsAsync()
    {
        BackupList.Clear();
        var backups = await _backupService.GetAllBackupsAsync();
        foreach (var backup in backups)
        {
            BackupList.Add(backup);
        }
        BackupCount = BackupList.Count;
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadBackupsAsync();
    }

    [RelayCommand]
    private async Task RestoreAsync()
    {
        if (SelectedBackup == null)
        {
            MessageBox.Show("请先选择要恢复的备份", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var confirm = MessageBox.Show(
            $"确定要恢复 {SelectedBackup.SoftwareName} 的备份吗？\n备份时间: {SelectedBackup.BackupTime:yyyy-MM-dd HH:mm}",
            "确认恢复",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes) return;

        var result = await _backupService.RestoreBackupAsync(SelectedBackup);
        if (result)
        {
            MessageBox.Show("备份恢复成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show("备份恢复失败", "失败", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedBackup == null) return;

        var confirm = MessageBox.Show($"确定要删除 {SelectedBackup.SoftwareName} 的备份吗？", "确认删除",
            MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        var result = await _backupService.DeleteBackupAsync(SelectedBackup.Id);
        if (result)
        {
            BackupList.Remove(SelectedBackup);
            BackupCount = BackupList.Count;
        }
    }

    [RelayCommand]
    private void OpenBackupDir()
    {
        try
        {
            System.Diagnostics.Process.Start("explorer.exe", Shared.Helpers.PathHelper.GetBackupPath());
        }
        catch { }
    }

    [RelayCommand]
    private void OpenDir()
    {
        if (SelectedBackup == null) return;
        try
        {
            System.Diagnostics.Process.Start("explorer.exe", SelectedBackup.BackupPath);
        }
        catch { }
    }
}
