using Microsoft.EntityFrameworkCore;

namespace Biwen.QuickApi.UnitOfWork;

/// <summary>
/// 自动广播Entity的增删改事件的DbContext
/// </summary>
public abstract class AutoEventDbContext<TDbContext> : DbContext where TDbContext : DbContext
{
    public AutoEventDbContext(DbContextOptions<TDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //实现自动广播Entity的增删改事件
        optionsBuilder.AddInterceptors(new AutoEventInterceptor());
    }
}