using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace SmartUninstaller.Core.Utils;

/// <summary>
/// 进程操作辅助工具类
/// </summary>
public class ProcessHelper
{
    private readonly ILogger<ProcessHelper> _logger;

    public ProcessHelper(ILogger<ProcessHelper> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 结束指定进程
    /// </summary>
    /// <param name="processId">进程ID</param>
    public virtual async Task KillProcessAsync(int processId)
    {
        try
        {
            var process = Process.GetProcessById(processId);
            process.Kill();
            await process.WaitForExitAsync();
        }
        catch (ArgumentException)
        {
            _logger.LogInformation("进程已不存在: {ProcessId}", processId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "结束进程失败: {ProcessId}", processId);
            throw;
        }
    }

    /// <summary>
    /// 检查进程是否正在运行
    /// </summary>
    /// <param name="processId">进程ID</param>
    /// <returns>是否正在运行</returns>
    public virtual bool IsProcessRunning(int processId)
    {
        try
        {
            var process = Process.GetProcessById(processId);
            return !process.HasExited;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 获取运行中的进程列表
    /// </summary>
    /// <returns>进程列表</returns>
    public virtual Process[] GetRunningProcesses()
    {
        return Process.GetProcesses();
    }

    /// <summary>
    /// 执行PowerShell脚本
    /// </summary>
    /// <param name="script">脚本内容</param>
    /// <returns>是否执行成功</returns>
    public virtual async Task<bool> ExecutePowerShellScriptAsync(string script)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -NonInteractive -Command \"{script}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null) return false;

            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行PowerShell脚本失败");
            return false;
        }
    }
}
