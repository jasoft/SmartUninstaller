using Microsoft.Win32;
using Microsoft.Extensions.Logging;
using SmartUninstaller.Core.Models;

namespace SmartUninstaller.Core.Utils;

/// <summary>
/// 注册表操作辅助工具类
/// </summary>
public class RegistryHelper
{
    private readonly ILogger<RegistryHelper> _logger;

    public RegistryHelper(ILogger<RegistryHelper> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 扫描卸载注册表中的软件信息
    /// </summary>
    /// <param name="subKeyPath">注册表子键路径</param>
    /// <param name="isCurrentUser">是否为当前用户</param>
    /// <returns>软件信息列表</returns>
    public virtual async Task<IEnumerable<SoftwareInfo>> ScanUninstallRegistryAsync(string subKeyPath, bool isCurrentUser = false)
    {
        var softwareList = new List<SoftwareInfo>();

        try
        {
            var hive = isCurrentUser ? RegistryHive.CurrentUser : RegistryHive.LocalMachine;
            using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            using var subKey = baseKey.OpenSubKey(subKeyPath);

            if (subKey == null) return softwareList;

            foreach (var keyName in subKey.GetSubKeyNames())
            {
                try
                {
                    using var appKey = subKey.OpenSubKey(keyName);
                    if (appKey == null) continue;

                    var displayName = appKey.GetValue("DisplayName")?.ToString();
                    if (string.IsNullOrWhiteSpace(displayName)) continue;

                    var software = new SoftwareInfo
                    {
                        Id = keyName,
                        Name = displayName,
                        DisplayName = displayName,
                        Version = appKey.GetValue("DisplayVersion")?.ToString(),
                        Publisher = appKey.GetValue("Publisher")?.ToString(),
                        InstallPath = appKey.GetValue("InstallLocation")?.ToString(),
                        UninstallString = appKey.GetValue("UninstallString")?.ToString(),
                        IconPath = appKey.GetValue("DisplayIcon")?.ToString(),
                        Description = appKey.GetValue("Comments")?.ToString(),
                        HelpLink = appKey.GetValue("HelpLink")?.ToString(),
                        UrlInfoAbout = appKey.GetValue("URLInfoAbout")?.ToString(),
                        RegistryPath = $@"{(isCurrentUser ? "HKCU" : "HKLM")}\{subKeyPath}\{keyName}"
                    };

                    if (DateTime.TryParse(appKey.GetValue("InstallDate")?.ToString(), out var installDate))
                        software.InstallDate = installDate;

                    if (long.TryParse(appKey.GetValue("EstimatedSize")?.ToString(), out var size))
                        software.Size = size * 1024;

                    softwareList.Add(software);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "读取注册表项失败: {KeyName}", keyName);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "扫描注册表失败: {Path}", subKeyPath);
        }

        return await Task.FromResult(softwareList);
    }

    /// <summary>
    /// 删除注册表键
    /// </summary>
    /// <param name="registryPath">注册表路径</param>
    /// <returns>删除的键数量</returns>
    public virtual async Task<int> DeleteRegistryKeyAsync(string registryPath)
    {
        try
        {
            var parts = registryPath.Split('\\', 2);
            if (parts.Length != 2) return 0;

            var hiveName = parts[0].ToUpperInvariant();
            var subPath = parts[1];

            var hive = hiveName switch
            {
                "HKLM" or "HKEY_LOCAL_MACHINE" => RegistryHive.LocalMachine,
                "HKCU" or "HKEY_CURRENT_USER" => RegistryHive.CurrentUser,
                "HKCR" or "HKEY_CLASSES_ROOT" => RegistryHive.ClassesRoot,
                _ => (RegistryHive?)null
            };

            if (hive == null) return 0;

            using var baseKey = RegistryKey.OpenBaseKey(hive.Value, RegistryView.Registry64);
            baseKey.DeleteSubKeyTree(subPath, throwOnMissingSubKey: false);
            return 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除注册表键失败: {Path}", registryPath);
            return 0;
        }
    }

    /// <summary>
    /// 检查注册表键是否存在
    /// </summary>
    /// <param name="registryPath">注册表路径</param>
    /// <returns>是否存在</returns>
    public virtual async Task<bool> RegistryKeyExistsAsync(string registryPath)
    {
        try
        {
            var parts = registryPath.Split('\\', 2);
            if (parts.Length != 2) return false;

            var hiveName = parts[0].ToUpperInvariant();
            var subPath = parts[1];

            var hive = hiveName switch
            {
                "HKLM" or "HKEY_LOCAL_MACHINE" => RegistryHive.LocalMachine,
                "HKCU" or "HKEY_CURRENT_USER" => RegistryHive.CurrentUser,
                _ => (RegistryHive?)null
            };

            if (hive == null) return false;

            using var baseKey = RegistryKey.OpenBaseKey(hive.Value, RegistryView.Registry64);
            using var subKey = baseKey.OpenSubKey(subPath);
            return await Task.FromResult(subKey != null);
        }
        catch
        {
            return false;
        }
    }
}

