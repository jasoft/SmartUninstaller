using SmartUninstaller.Core.Interfaces;
using SmartUninstaller.Core.Models;
using SmartUninstaller.Core.Engines;
using Microsoft.Extensions.Logging;

namespace SmartUninstaller.Core.Services;

/// <summary>
/// 卸载服务实现
/// </summary>
public class UninstallService : IUninstallService
{
    private readonly ILogger<UninstallService> _logger;
    private readonly UninstallEngine _uninstallEngine;
    private readonly PortableSoftwareEngine _portableEngine;

    public UninstallService(
        ILogger<UninstallService> logger,
        UninstallEngine uninstallEngine,
        PortableSoftwareEngine portableEngine)
    {
        _logger = logger;
        _uninstallEngine = uninstallEngine;
        _portableEngine = portableEngine;
    }

    /// <inheritdoc/>
    public async Task<UninstallResult> UninstallAsync(SoftwareInfo software, UninstallOptions options)
    {
        return await _uninstallEngine.ExecuteUninstallAsync(software, options);
    }

    /// <inheritdoc/>
    public async Task<UninstallResult> ForceUninstallAsync(SoftwareInfo software, UninstallOptions options)
    {
        options.ForceUninstall = true;
        options.DeepClean = true;
        return await _uninstallEngine.ExecuteUninstallAsync(software, options);
    }

    /// <inheritdoc/>
    public async Task<BatchUninstallResult> BatchUninstallAsync(IEnumerable<SoftwareInfo> softwareList, UninstallOptions options)
    {
        var results = new List<UninstallResult>();
        var totalCount = 0;

        foreach (var software in softwareList)
        {
            totalCount++;
            var result = await UninstallAsync(software, options);
            results.Add(result);
        }

        return new BatchUninstallResult
        {
            TotalCount = totalCount,
            SuccessCount = results.Count(r => r.Success),
            FailedCount = results.Count(r => !r.Success),
            Results = results,
            TotalSpaceFreed = results.Sum(r => r.SpaceFreed),
            TotalDuration = TimeSpan.FromTicks(results.Sum(r => r.Duration.Ticks))
        };
    }

    /// <inheritdoc/>
    public Task<bool> CanUninstallAsync(SoftwareInfo software)
    {
        var canUninstall = !string.IsNullOrEmpty(software.UninstallString) || software.IsPortable;
        return Task.FromResult(canUninstall);
    }

    /// <inheritdoc/>
    public Task<UninstallEstimate> GetUninstallEstimateAsync(SoftwareInfo software)
    {
        var estimate = new UninstallEstimate
        {
            EstimatedSpaceFreed = software.Size,
            EstimatedDurationSeconds = software.IsPortable ? 5 : 30,
            EstimatedFilesToDelete = (int)(software.Size / 1024 / 10),
            EstimatedRegistryEntriesToDelete = software.IsPortable ? 0 : 5,
            RiskLevel = software.IsRunning ? RiskLevel.Medium : RiskLevel.Low,
            RiskDescription = software.IsRunning ? "软件正在运行，卸载前将自动关闭" : "可以安全卸载"
        };
        return Task.FromResult(estimate);
    }
}
