using Microsoft.EntityFrameworkCore;

namespace Biwen.QuickApi.Test.UnitOfWork
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<User>().Property(x => x.Name).IsRequired().HasMaxLength(50);
        }


    }
}