using Ganss.Xss;
using HtmlSanitizerOptions = Biwen.QuickApi.Infrastructure.Html.HtmlSanitizerOptions;

namespace Microsoft.Extensions.DependencyInjection
{
    [SuppressType]
    public static class HtmlSanitizerOptionsExtensions
    {
        /// <summary>
        /// Adds a configuration action to the html sanitizer.
        /// </summary>
        public static void ConfigureHtmlSanitizer(this IServiceCollection services, Action<HtmlSanitizer> action)
        {
            services.Configure<HtmlSanitizerOptions>(o =>
            {
                o.Configure.Add(action);
            });
        }
    }
}
