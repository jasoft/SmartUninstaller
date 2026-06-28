using Microsoft.EntityFrameworkCore;
using SmartUninstaller.Data.Context;
using SmartUninstaller.Data.Entities;

namespace SmartUninstaller.Data.Repositories;

/// <summary>
/// 卸载历史记录仓储
/// </summary>
public class UninstallHistoryRepository
{
    private readonly AppDbContext _context;

    public UninstallHistoryRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 添加卸载记录
    /// </summary>
    /// <param name="entity">卸载记录实体</param>
    /// <returns>添加后的实体</returns>
    public async Task<UninstallHistoryEntity> AddAsync(UninstallHistoryEntity entity)
    {
        _context.UninstallHistory.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// 获取所有卸载记录（按时间倒序）
    /// </summary>
    /// <param name="limit">最大返回数量</param>
    /// <returns>卸载记录列表</returns>
    public async Task<List<UninstallHistoryEntity>> GetAllAsync(int limit = 100)
    {
        return await _context.UninstallHistory
            .OrderByDescending(e => e.UninstallTime)
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// 按软件名称搜索卸载记录
    /// </summary>
    /// <param name="keyword">搜索关键词</param>
    /// <returns>匹配的记录</returns>
    public async Task<List<UninstallHistoryEntity>> SearchAsync(string keyword)
    {
        return await _context.UninstallHistory
            .Where(e => e.SoftwareName.Contains(keyword))
            .OrderByDescending(e => e.UninstallTime)
            .ToListAsync();
    }

    /// <summary>
    /// 获取统计信息
    /// </summary>
    /// <returns>总卸载次数、成功次数、释放总空间</returns>
    public async Task<(int Total, int Success, long TotalSpaceFreed)> GetStatisticsAsync()
    {
        var total = await _context.UninstallHistory.CountAsync();
        var success = await _context.UninstallHistory.CountAsync(e => e.Success);
        var totalSpace = await _context.UninstallHistory.SumAsync(e => e.SpaceFreed);
        return (total, success, totalSpace);
    }

    /// <summary>
    /// 删除指定记录
    /// </summary>
    /// <param name="id">记录ID</param>
    public async Task DeleteAsync(int id)
    {
        var entity = await _context.UninstallHistory.FindAsync(id);
        if (entity != null)
        {
            _context.UninstallHistory.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// 清空所有记录
    /// </summary>
    public async Task ClearAsync()
    {
        _context.UninstallHistory.RemoveRange(_context.UninstallHistory);
        await _context.SaveChangesAsync();
    }
}
