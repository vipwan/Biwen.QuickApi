namespace Biwen.QuickApi.Infrastructure.Html
{
    [CoreModular]
    internal class HtmlSanitizerModular : ModularBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddHtmlSanitizer();
        }
    }
}