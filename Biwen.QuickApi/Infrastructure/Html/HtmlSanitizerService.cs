using Ganss.Xss;

namespace Biwen.QuickApi.Infrastructure.Html
{
    public class HtmlSanitizerService : IHtmlSanitizerService
    {
        private readonly HtmlSanitizer _sanitizer = new();

        public HtmlSanitizerService(IOptions<HtmlSanitizerOptions> options)
        {
            foreach (var action in options.Value.Configure)
            {
                action?.Invoke(_sanitizer);
            }
        }

        public string Sanitize(string html) => _sanitizer.Sanitize(html);
    }
}
