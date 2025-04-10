using Biwen.QuickApi.Contents.Domain;
using Microsoft.EntityFrameworkCore;

namespace Biwen.QuickApi.DocSite
{
    /// <summary>
    /// Test
    /// </summary>
    public class TestDbContext : DbContext, IContentDbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }
        public DbSet<Content> Contents { get; set; }
        public DbSet<ContentAuditLog> ContentAuditLogs { get; set; }
        public DbSet<ContentVersion> ContentVersions { get; set; }

        public DbContext Context => this;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Content>().ToTable("Contents");
        }

    }
}