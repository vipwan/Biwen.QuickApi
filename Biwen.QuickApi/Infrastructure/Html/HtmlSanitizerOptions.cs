using Ganss.Xss;

namespace Biwen.QuickApi.Infrastructure.Html
{
    public class HtmlSanitizerOptions
    {
        public List<Action<HtmlSanitizer>> Configure { get; } = [];
    }
}
