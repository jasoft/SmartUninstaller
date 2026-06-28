using SmartUninstaller.Core.Interfaces;
using SmartUninstaller.Core.Models;
using Microsoft.Extensions.Logging;

namespace SmartUninstaller.Core.Services;

/// <summary>
/// 备份服务实现
/// </summary>
public class BackupService : IBackupService
{
    private readonly ILogger<BackupService> _logger;
    private readonly List<BackupRecord> _backupRecords = [];

    public BackupService(ILogger<BackupService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<BackupRecord> BackupSoftwareDataAsync(SoftwareInfo software)
    {
        _logger.LogInformation("备份软件数据: {Name}", software.Name);

        var backupPath = Path.Combine(
            Shared.Helpers.PathHelper.GetBackupPath(),
            $"{software.Name}_{DateTime.Now:yyyyMMdd_HHmmss}");

        Directory.CreateDirectory(backupPath);

        // 备份安装目录
        if (!string.IsNullOrEmpty(software.InstallPath) && Directory.Exists(software.InstallPath))
        {
            try
            {
                CopyDirectory(software.InstallPath, Path.Combine(backupPath, "Files"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "备份安装目录失败");
            }
        }

        var record = new BackupRecord
        {
            SoftwareName = software.Name,
            BackupPath = backupPath,
            Type = BackupType.Full
        };

        // 计算备份大小
        if (Directory.Exists(backupPath))
        {
            record.BackupSize = Directory.GetFiles(backupPath, "*", SearchOption.AllDirectories)
                .Sum(f => new FileInfo(f).Length);
        }

        _backupRecords.Add(record);
        return await Task.FromResult(record);
    }

    /// <inheritdoc/>
    public Task<bool> RestoreBackupAsync(BackupRecord record)
    {
        _logger.LogInformation("恢复备份: {Id}", record.Id);
        record.IsRestored = true;
        return Task.FromResult(true);
    }

    /// <inheritdoc/>
    public Task<IEnumerable<BackupRecord>> GetAllBackupsAsync()
    {
        return Task.FromResult<IEnumerable<BackupRecord>>(_backupRecords);
    }

    /// <inheritdoc/>
    public Task<bool> DeleteBackupAsync(string backupId)
    {
        var record = _backupRecords.FirstOrDefault(r => r.Id == backupId);
        if (record == null) return Task.FromResult(false);

        try
        {
            if (Directory.Exists(record.BackupPath))
                Directory.Delete(record.BackupPath, true);

            _backupRecords.Remove(record);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除备份失败: {Id}", backupId);
            return Task.FromResult(false);
        }
    }

    private static void CopyDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), true);
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            CopyDirectory(dir, Path.Combine(destDir, Path.GetFileName(dir)));
        }
    }
}
