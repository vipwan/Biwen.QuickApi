using System.ComponentModel.DataAnnotations;

namespace Biwen.QuickApi.Test.UnitOfWork
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(512)]
        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}