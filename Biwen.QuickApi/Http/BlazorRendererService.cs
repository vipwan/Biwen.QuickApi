// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:44:01 BlazorRendererService.cs

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using IComponent = Microsoft.AspNetCore.Components.IComponent;

namespace Biwen.QuickApi.Http
{
    /// <summary>
    /// 根据Blazor组件渲染Html的服务
    /// </summary>
    /// <param name="serviceProvider"></param>
    public sealed class BlazorRendererService(IServiceProvider serviceProvider)
    {

        /// <summary>
        /// 根据Blazor组件渲染Html
        /// </summary>
        /// <typeparam name="T">Blazor组件</typeparam>
        /// <param name="parms">传递的参数字典</param>
        /// <returns></returns>
        public async Task<string> Render<T>(IDictionary<string, object?>? parms) where T : class, IComponent
        {
            using var htmlRenderer = ActivatorUtilities.GetServiceOrCreateInstance<HtmlRenderer>(serviceProvider);

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