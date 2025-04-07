//
using Microsoft.EntityFrameworkCore;

namespace Biwen.QuickApi.DemoWeb.Db
{
    using Biwen.QuickApi.Contents.Domain;

    public class ContentDbContext : DbContext, IContentDbContext
    {
        public ContentDbContext(DbContextOptions<ContentDbContext> options) : base(options)
        {
        }

        public DbSet<Content> Contents { get; set; }

        public DbSet<ContentAuditLog> ContentAuditLogs { get; set; }

        public DbSet<ContentVersion> ContentVersions { get; set; }

        public DbContext Context => this;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}