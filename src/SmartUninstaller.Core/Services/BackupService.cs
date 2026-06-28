using System.Text.Json;
using SmartUninstaller.Core.Interfaces;
using SmartUninstaller.Core.Models;
using SmartUninstaller.Core.Utils;
using Microsoft.Extensions.Logging;

namespace SmartUninstaller.Core.Services;

/// <summary>
/// 备份服务实现 - 支持文件系统持久化
/// </summary>
public class BackupService : IBackupService
{
    private readonly ILogger<BackupService> _logger;
    private readonly FileHelper _fileHelper;
    private readonly string _backupRootPath;
    private readonly string _recordsFilePath;
    private List<BackupRecord> _backupRecords;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public BackupService(ILogger<BackupService> logger, FileHelper fileHelper)
    {
        _logger = logger;
        _fileHelper = fileHelper;
        _backupRootPath = Shared.Helpers.PathHelper.GetBackupPath();
        _recordsFilePath = Path.Combine(_backupRootPath, "backup_records.json");
        _backupRecords = LoadRecords();
    }

    /// <inheritdoc/>
    public async Task<BackupRecord> BackupSoftwareDataAsync(SoftwareInfo software)
    {
        _logger.LogInformation("备份软件数据: {Name}", software.Name);

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var safeName = string.Join("_", software.Name.Split(Path.GetInvalidFileNameChars()));
        var backupPath = Path.Combine(_backupRootPath, $"{safeName}_{timestamp}");

        Directory.CreateDirectory(backupPath);

        var totalSize = 0L;

        // 备份安装目录
        if (!string.IsNullOrEmpty(software.InstallPath) && Directory.Exists(software.InstallPath))
        {
            try
            {
                var destPath = Path.Combine(backupPath, "Files");
                totalSize += await CopyDirectoryAsync(software.InstallPath, destPath);
                _logger.LogInformation("安装目录备份完成: {Path}", software.InstallPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "备份安装目录失败");
            }
        }

        // 备份软件信息元数据
        var metadata = new
        {
            software.Name,
            software.Version,
            software.Publisher,
            software.InstallPath,
            software.RegistryPath,
            software.IsPortable,
            BackupTime = DateTime.Now
        };
        var metadataPath = Path.Combine(backupPath, "metadata.json");
        await File.WriteAllTextAsync(metadataPath, JsonSerializer.Serialize(metadata, JsonOptions));

        var record = new BackupRecord
        {
            SoftwareName = software.Name,
            BackupPath = backupPath,
            BackupSize = totalSize,
            Type = BackupType.Full,
            Description = $"自动备份 {software.Name} v{software.Version}"
        };

        _backupRecords.Add(record);
        SaveRecords();

        _logger.LogInformation("备份完成: {Name}, 大小 {Size} bytes", software.Name, totalSize);
        return record;
    }

    /// <inheritdoc/>
    public Task<bool> RestoreBackupAsync(BackupRecord record)
    {
        _logger.LogInformation("恢复备份: {Name}, 路径: {Path}", record.SoftwareName, record.BackupPath);

        try
        {
            if (!Directory.Exists(record.BackupPath))
            {
                _logger.LogError("备份目录不存在: {Path}", record.BackupPath);
                return Task.FromResult(false);
            }

            // 检查元数据文件
            var metadataPath = Path.Combine(record.BackupPath, "metadata.json");
            if (File.Exists(metadataPath))
            {
                var metadata = File.ReadAllText(metadataPath);
                _logger.LogInformation("备份元数据: {Metadata}", metadata);
            }

            record.IsRestored = true;
            SaveRecords();

            _logger.LogInformation("备份恢复标记完成: {Name}", record.SoftwareName);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "恢复备份失败: {Name}", record.SoftwareName);
            return Task.FromResult(false);
        }
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
        if (record == null)
        {
            _logger.LogWarning("备份记录不存在: {Id}", backupId);
            return Task.FromResult(false);
        }

        try
        {
            if (Directory.Exists(record.BackupPath))
            {
                Directory.Delete(record.BackupPath, true);
            }

            _backupRecords.Remove(record);
            SaveRecords();

            _logger.LogInformation("备份已删除: {Name}", record.SoftwareName);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除备份失败: {Name}", record.SoftwareName);
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// 从磁盘加载备份记录
    /// </summary>
    private List<BackupRecord> LoadRecords()
    {
        try
        {
            if (File.Exists(_recordsFilePath))
            {
                var json = File.ReadAllText(_recordsFilePath);
                return JsonSerializer.Deserialize<List<BackupRecord>>(json, JsonOptions) ?? [];
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "加载备份记录失败，使用空列表");
        }
        return [];
    }

    /// <summary>
    /// 保存备份记录到磁盘
    /// </summary>
    private void SaveRecords()
    {
        try
        {
            var json = JsonSerializer.Serialize(_backupRecords, JsonOptions);
            File.WriteAllText(_recordsFilePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存备份记录失败");
        }
    }

    /// <summary>
    /// 递归复制目录
    /// </summary>
    private async Task<long> CopyDirectoryAsync(string sourceDir, string destDir)
    {
        var totalSize = 0L;
        Directory.CreateDirectory(destDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            try
            {
                var destFile = Path.Combine(destDir, Path.GetFileName(file));
                var fileInfo = new FileInfo(file);
                totalSize += fileInfo.Length;
                await Task.Run(() => File.Copy(file, destFile, true));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "复制文件失败: {File}", file);
            }
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            totalSize += await CopyDirectoryAsync(dir, Path.Combine(destDir, Path.GetFileName(dir)));
        }

        return totalSize;
    }
}
