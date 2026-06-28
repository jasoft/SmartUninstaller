using Microsoft.EntityFrameworkCore;
using SmartUninstaller.Data.Entities;

namespace SmartUninstaller.Data.Context;

/// <summary>
/// 应用数据库上下文 - 使用SQLite存储
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>卸载历史记录</summary>
    public DbSet<UninstallHistoryEntity> UninstallHistory => Set<UninstallHistoryEntity>();

    private readonly string _dbPath;

    /// <summary>
    /// 创建数据库上下文
    /// </summary>
    /// <param name="dbPath">数据库文件路径</param>
    public AppDbContext(string dbPath)
    {
        _dbPath = dbPath;
    }

    /// <summary>
    /// 使用默认路径创建数据库上下文
    /// </summary>
    public AppDbContext()
    {
        _dbPath = Shared.Helpers.PathHelper.GetDatabasePath();
    }

    /// <inheritdoc/>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={_dbPath}");
    }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UninstallHistoryEntity>(entity =>
        {
            entity.ToTable("UninstallHistory");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SoftwareName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.SoftwareVersion).HasMaxLength(100);
            entity.Property(e => e.Publisher).HasMaxLength(500);
            entity.Property(e => e.InstallPath).HasMaxLength(1000);
            entity.Property(e => e.UninstallMethod).HasMaxLength(50);
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            entity.HasIndex(e => e.UninstallTime);
            entity.HasIndex(e => e.SoftwareName);
        });
    }

    /// <summary>
    /// 确保数据库已创建
    /// </summary>
    public void EnsureDatabaseCreated()
    {
        Database.EnsureCreated();
    }
}
