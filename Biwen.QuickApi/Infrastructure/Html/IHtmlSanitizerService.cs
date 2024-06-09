namespace Biwen.QuickApi.Infrastructure.Html
{
    public interface IHtmlSanitizerService
    {
        /// <summary>
        /// 消除HTML中的XSS攻击
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        string Sanitize(string html);
    }
}
