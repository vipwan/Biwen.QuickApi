using Biwen.QuickApi.Infrastructure.Html;

namespace Microsoft.Extensions.DependencyInjection
{
    internal static partial class ServiceRegistration
    {
        /// <summary>
        /// Adds html script sanitization services.
        /// </summary>
        internal static IServiceCollection AddHtmlSanitizer(this IServiceCollection services)
        {

            services.AddOptions<HtmlSanitizerOptions>();

            services.ConfigureHtmlSanitizer((sanitizer) =>
            {
                sanitizer.AllowedAttributes.Add("class");
                sanitizer.AllowedTags.Remove("form");
            });

            services.AddActivatedSingleton<IHtmlSanitizerService, HtmlSanitizerService>();
            return services;

        }
    }
}
