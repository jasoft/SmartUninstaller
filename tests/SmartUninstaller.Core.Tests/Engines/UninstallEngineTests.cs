using Xunit;
using Moq;
using SmartUninstaller.Core.Engines;
using SmartUninstaller.Core.Models;
using SmartUninstaller.Core.Utils;
using Microsoft.Extensions.Logging;

namespace SmartUninstaller.Core.Tests.Engines;

/// <summary>
/// 卸载引擎测试
/// </summary>
public class UninstallEngineTests
{
    private readonly Mock<ILogger<UninstallEngine>> _loggerMock;
    private readonly UninstallEngine _uninstallEngine;

    public UninstallEngineTests()
    {
        _loggerMock = new Mock<ILogger<UninstallEngine>>();
        var regLogger = new Mock<ILogger<RegistryHelper>>();
        var fileLogger = new Mock<ILogger<FileHelper>>();
        var procLogger = new Mock<ILogger<ProcessHelper>>();

        _uninstallEngine = new UninstallEngine(
            _loggerMock.Object,
            new RegistryHelper(regLogger.Object),
            new FileHelper(fileLogger.Object),
            new ProcessHelper(procLogger.Object));
    }

    [Fact]
    public void UninstallEngine_ShouldBeInstantiable()
    {
        Assert.NotNull(_uninstallEngine);
    }

    [Fact]
    public async Task ExecuteUninstallAsync_WithNullUninstallString_ShouldHandleGracefully()
    {
        // Arrange
        var software = new SoftwareInfo
        {
            Id = "test-id",
            Name = "Test Software",
            InstallPath = @"C:\NonExistent\Path",
            UninstallString = null,
            IsRunning = false
        };

        var options = new Interfaces.UninstallOptions
        {
            CreateRestorePoint = false,
            DeepClean = false,
            CleanRegistry = false,
            ForceUninstall = false
        };

        // Act
        var result = await _uninstallEngine.ExecuteUninstallAsync(software, options);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(software, result.Software);
    }
}
