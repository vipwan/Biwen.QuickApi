using System.ComponentModel.DataAnnotations;

namespace Biwen.QuickApi.DemoWeb.Db.Entity
{

    public partial class User
    {
        public int Id { get; set; }

        [Required, StringLength(50, MinimumLength = 2)]
        public string Name { get; set; } = null!;

        public int? Age { get; set; }

        public string? Address { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
