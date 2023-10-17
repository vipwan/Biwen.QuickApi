using Biwen.QuickApi;
using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace BenchmarkTestWeb
{

    [FromBody]
    public class MyRequest : BaseRequest<MyRequest>
    {
        [Required, StringLength(10, MinimumLength = 2)]
        public string? UserName { get; set; }

        [Required]
        public string? Password { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? Phone { get; set; }

        public string? Address { get; set; }

        public string? Remark { get; set; }

        public MyRequest()
        {
            RuleFor(x => x.Remark).Length(2, 50).NotNull();
        }
    }
}