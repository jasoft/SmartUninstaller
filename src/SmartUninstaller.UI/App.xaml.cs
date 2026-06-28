using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SmartUninstaller.Core.Extensions;

namespace SmartUninstaller.UI;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        services.AddSmartUninstallerCore();
        ServiceProvider = services.BuildServiceProvider();

        var mainWindow = new Views.MainWindow();
        mainWindow.Show();
    }
}
