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
