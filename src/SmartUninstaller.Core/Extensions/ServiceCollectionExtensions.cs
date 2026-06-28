using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartUninstaller.Core.Interfaces;
using SmartUninstaller.Core.Services;
using SmartUninstaller.Core.Engines;
using SmartUninstaller.Core.Utils;
using SmartUninstaller.Data.Context;
using SmartUninstaller.Data.Repositories;

namespace SmartUninstaller.Core.Extensions;

/// <summary>
/// 依赖注入扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册 SmartUninstaller 核心服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddSmartUninstallerCore(this IServiceCollection services)
    {
                // 日志
        services.AddLogging();

        // 数据层
        services.AddSingleton<AppDbContext>(sp =>
        {
            var context = new AppDbContext();
            context.EnsureDatabaseCreated();
            return context;
        });
        services.AddSingleton<UninstallHistoryRepository>();

        // 注册服务
        services.AddScoped<IUninstallService, UninstallService>();
        services.AddScoped<IScanService, ScanService>();
        services.AddScoped<ICleanupService, CleanupService>();
        services.AddScoped<IBackupService, BackupService>();
        services.AddScoped<ILoggingService, LoggingService>();

        // 注册引擎
        services.AddScoped<UninstallEngine>();
        services.AddScoped<ScanEngine>();
        services.AddScoped<CleanupEngine>();
        services.AddScoped<DetectionEngine>();
        services.AddScoped<PortableSoftwareEngine>();

        // 注册工具类
        services.AddSingleton<RegistryHelper>();
        services.AddSingleton<FileHelper>();
        services.AddSingleton<ProcessHelper>();
        services.AddSingleton<WmiHelper>();

        return services;
    }
}
