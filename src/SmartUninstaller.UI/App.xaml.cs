using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SmartUninstaller.Core.Extensions;

namespace SmartUninstaller.UI;

/// <summary>
/// App.xaml 的交互逻辑
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// 服务提供者
    /// </summary>
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    /// <summary>
    /// 静态构造函数 - 在任何WPF类型初始化之前运行
    /// </summary>
    static App()
    {
        // 修复 Windows 11 25H2 + .NET 8 WPF 的 FontCache.Util 静态构造函数崩溃
        // 堆栈: MS.Internal.FontCache.Util..cctor() -> new Uri(string) -> UriFormatException
        // 根因: WPF 内部的 FontCache 路径解析在某些 Windows 版本上获取到空/非法值
        try
        {
            // 确保 FONTCONFIG_PATH 存在且合法
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("FONTCONFIG_PATH")))
            {
                Environment.SetEnvironmentVariable("FONTCONFIG_PATH", @"C:\Windows\Fonts");
            }

            // 设置 WPF 的 LocalAppData 基础路径环境变量
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (!string.IsNullOrEmpty(localAppData))
            {
                // 确保 WPF 能解析 LocalAppData 的URI
                Environment.SetEnvironmentVariable("LOCALAPPDATA", localAppData);
            }
        }
        catch
        {
            // 环境变量设置失败不影响启动
        }
    }

    /// <summary>
    /// 应用启动事件
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        services.AddSmartUninstallerCore();
        ServiceProvider = services.BuildServiceProvider();
    }
}
