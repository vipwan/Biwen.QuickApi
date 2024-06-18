using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using IComponent = Microsoft.AspNetCore.Components.IComponent;

namespace Biwen.QuickApi.Http
{
    /// <summary>
    /// 根据Blazor组件渲染Html的服务
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="loggerFactory"></param>
    public sealed class BlazorRendererService(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
    {

        /// <summary>
        /// 根据Blazor组件渲染Html
        /// </summary>
        /// <typeparam name="T">Blazor组件</typeparam>
        /// <param name="parms">传递的参数字典</param>
        /// <returns></returns>
        public async Task<string> Render<T>(IDictionary<string, object?>? parms) where T : IComponent
        {
            using var htmlRenderer = new HtmlRenderer(serviceProvider, loggerFactory);

            var html = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
            {
                var rootComponent = await htmlRenderer.RenderComponentAsync<T>(
                      parms is null ?
                      ParameterView.Empty :
                      ParameterView.FromDictionary(parms));

                return rootComponent.ToHtmlString();
            });

            return html;
        }
    }
}