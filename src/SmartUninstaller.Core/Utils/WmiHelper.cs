using System.Management;
using Microsoft.Extensions.Logging;
using SmartUninstaller.Core.Models;

namespace SmartUninstaller.Core.Utils;

/// <summary>
/// WMI查询辅助工具类
/// </summary>
public class WmiHelper
{
    private readonly ILogger<WmiHelper> _logger;

    public WmiHelper(ILogger<WmiHelper> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 查询已安装软件（通过WMI）
    /// </summary>
    /// <returns>软件信息列表</returns>
    public virtual async Task<IEnumerable<SoftwareInfo>> QueryInstalledSoftwareAsync()
    {
        var softwareList = new List<SoftwareInfo>();

        try
        {
            await Task.Run(() =>
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT * FROM Win32_Product");

                foreach (ManagementObject obj in searcher.Get())
                {
                    try
                    {
                        var software = new SoftwareInfo
                        {
                            Id = obj["IdentifyingNumber"]?.ToString() ?? Guid.NewGuid().ToString(),
                            Name = obj["Name"]?.ToString() ?? "Unknown",
                            DisplayName = obj["Name"]?.ToString() ?? "Unknown",
                            Version = obj["Version"]?.ToString(),
                            Publisher = obj["Vendor"]?.ToString(),
                            InstallPath = obj["InstallLocation"]?.ToString(),
                            Description = obj["Description"]?.ToString()
                        };

                        if (long.TryParse(obj["InstallState"]?.ToString(), out _))
                        {
                            software.IsInstalled = true;
                        }

                        softwareList.Add(software);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "解析WMI对象失败");
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WMI查询失败");
        }

        return softwareList;
    }
}
