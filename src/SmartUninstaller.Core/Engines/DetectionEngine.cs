using Microsoft.Extensions.Logging;

namespace SmartUninstaller.Core.Engines;

/// <summary>
/// 检测引擎，负责软件损坏检测和状态分析
/// </summary>
public class DetectionEngine
{
    private readonly ILogger<DetectionEngine> _logger;

    public DetectionEngine(ILogger<DetectionEngine> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 检测软件是否损坏
    /// </summary>
    /// <param name="installPath">安装路径</param>
    /// <param name="uninstallString">卸载命令</param>
    /// <returns>是否损坏</returns>
    public virtual async Task<bool> IsSoftwareDamagedAsync(string? installPath, string? uninstallString)
    {
        // 检查安装目录是否存在
        if (!string.IsNullOrEmpty(installPath) && !Directory.Exists(installPath))
        {
            _logger.LogWarning("软件安装目录不存在: {Path}", installPath);
            return true;
        }

        // 检查卸载程序是否存在
        if (!string.IsNullOrEmpty(uninstallString))
        {
            var exePath = ExtractExePath(uninstallString);
            if (!string.IsNullOrEmpty(exePath) && !File.Exists(exePath))
            {
                _logger.LogWarning("卸载程序不存在: {Path}", exePath);
                return true;
            }
        }

        return await Task.FromResult(false);
    }

    private static string? ExtractExePath(string uninstallString)
    {
        if (string.IsNullOrWhiteSpace(uninstallString)) return null;

        var trimmed = uninstallString.Trim('"', '\'');
        var spaceIndex = trimmed.IndexOf(' ');
        return spaceIndex > 0 ? trimmed[..spaceIndex] : trimmed;
    }
}
