using Microsoft.EntityFrameworkCore;

namespace Biwen.QuickApi.DemoWeb.Db
{
    using Biwen.QuickApi.DemoWeb.Db.Entity;
    using Biwen.QuickApi.UnitOfWork;

    public class UserDbContext : AutoEventDbContext<UserDbContext>
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreateTime).HasDefaultValueSql("getdate()");
            });
        }
    }
}
