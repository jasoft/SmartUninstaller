using System.Security.Principal;

namespace SmartUninstaller.Core.Utils;

/// <summary>
/// 安全辅助工具类
/// </summary>
public static class SecurityHelper
{
    /// <summary>
    /// 检查当前是否以管理员身份运行
    /// </summary>
    /// <returns>是否为管理员</returns>
    public static bool IsRunAsAdmin()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    /// <summary>
    /// 请求管理员权限重启
    /// </summary>
    public static void RequestAdminRestart()
    {
        var processInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = Environment.ProcessPath,
            Verb = "runas",
            UseShellExecute = true
        };

        try
        {
            System.Diagnostics.Process.Start(processInfo);
            Environment.Exit(0);
        }
        catch
        {
            // 用户拒绝了UAC提示
        }
    }
}
