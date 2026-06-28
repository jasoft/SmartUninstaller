using SmartUninstaller.Core.Models;

namespace SmartUninstaller.Core.Interfaces;

/// <summary>
/// 备份服务接口
/// </summary>
public interface IBackupService
{
    /// <summary>备份软件数据</summary>
    Task<BackupRecord> BackupSoftwareDataAsync(SoftwareInfo software);
    /// <summary>恢复备份</summary>
    Task<bool> RestoreBackupAsync(BackupRecord record);
    /// <summary>获取所有备份记录</summary>
    Task<IEnumerable<BackupRecord>> GetAllBackupsAsync();
    /// <summary>删除备份</summary>
    Task<bool> DeleteBackupAsync(string backupId);
}
