﻿using Biwen.QuickApi.Infrastructure.Html;

namespace Biwen.QuickApi
{
    [SuppressType]
    public static class StringExtensions
    {
        /// <summary>
        /// xss过滤
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        /// <exception cref="QuickApiExcetion">必须启用UseBiwenQuickApis()</exception>
        public static string SanitizeHtml(this string html)
        {
            if (ServiceRegistration.ServiceProvider is null) throw new QuickApiExcetion("mush UseBiwenQuickApis() first!");
            var svc = ActivatorUtilities.GetServiceOrCreateInstance<IHtmlSanitizerService>(ServiceRegistration.ServiceProvider);
            return svc.Sanitize(html);
        }
    }
}