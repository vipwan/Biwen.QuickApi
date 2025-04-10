// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-10 14:00:58 DocumentRenderService.cs

using Biwen.QuickApi.Contents.Abstractions;
using Biwen.QuickApi.Contents.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace Biwen.QuickApi.Contents.Rendering;


/// <summary>
/// 实现默认的文档渲染服务razor(.cshtml)
/// </summary>
internal class RazorDocumentRenderService : IDocumentRenderService
{
    private readonly IRazorViewEngine _razorViewEngine;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<BiwenContentOptions> _options;
    private readonly IContentRepository _repository;
    private readonly ContentSerializer _contentSerializer;
    private readonly string _templatePath;

    public RazorDocumentRenderService(
        IRazorViewEngine razorViewEngine,
        ITempDataProvider tempDataProvider,
        IServiceProvider serviceProvider,
        IContentRepository repository,
        ContentSerializer contentSerializer,
        IOptions<BiwenContentOptions> options)
    {
        _razorViewEngine = razorViewEngine;
        _tempDataProvider = tempDataProvider;
        _serviceProvider = serviceProvider;
        _options = options;
        _repository = repository;
        _contentSerializer = contentSerializer;
        _templatePath = Path.Combine(AppContext.BaseDirectory, _options.Value.ViewPath!);
    }

    private async Task<string> RenderAsync<T>(string viewName, T model)
    {
        var actionContext = GetActionContext();
        var view = FindView(actionContext, viewName);

        using var output = new StringWriter();
        var viewContext = new ViewContext(
            actionContext,
            view,
            new ViewDataDictionary<T>(
                new EmptyModelMetadataProvider(),
                new ModelStateDictionary())
            {
                Model = model
            },
            new TempDataDictionary(
                actionContext.HttpContext,
                _tempDataProvider),
            output,
            new HtmlHelperOptions()
        );

        await view.RenderAsync(viewContext);
        return output.ToString();
    }

    private IView FindView(ActionContext actionContext, string viewName)
    {
        var getViewResult = _razorViewEngine.GetView(null, viewName, false);
        if (getViewResult.Success)
        {
            return getViewResult.View;
        }

        var findViewResult = _razorViewEngine.FindView(actionContext, viewName, false);
        if (findViewResult.Success)
        {
            return findViewResult.View;
        }

        var searchedLocations = getViewResult.SearchedLocations.Concat(findViewResult.SearchedLocations);
        var errorMessage = string.Join(
            Environment.NewLine,
            new[] { $"Unable to find view '{viewName}'. The following locations were searched:" }.Concat(searchedLocations));

        throw new InvalidOperationException(errorMessage);
    }

    private ActionContext GetActionContext()
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = _serviceProvider
        };

        return new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor());
    }

    /// <summary>
    /// 内部方法,渲染文档,不暴露于接口中
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="document"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public Task<string> RenderDocumentAsync<T>(T document, Content content) where T : class, IContent
    {
        //文件名约定: {ContentType}.cshtml,其中ContentType为文档.分割的类型名称
        var contentType = document.Content_ContentType.Split('.').Last();
        var viewName = $"{Path.Combine(_templatePath, contentType)}.cshtml";

        var vm = new ContentViewModel<T>
        {
            Content = document,
            ContentDefine = content
        };

        return RenderAsync(viewName, vm);
    }

    internal async Task<string> RenderDocumentAsync(Content content)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content), "内容不能为空");
        }

        // 获取文档类型
        var types = _contentSerializer.GetAllContentTypes();
        var contentType = types.FirstOrDefault(t =>
            t.Content_ContentType == content.ContentType ||
            t.Content_ContentType.Split('.').Last() == content.ContentType);

        if (contentType == null)
        {
            throw new ArgumentException($"不支持的文档类型: {content.ContentType}");
        }

        // 获取实际类型
        var actualType = contentType.GetType();

        //使用反射获取GetContentAsync的泛型方法并调用:
        var deserializeMethod = typeof(ContentSerializer)
            .GetMethod(nameof(ContentSerializer.DeserializeContent))
            ?.MakeGenericMethod(actualType);

        var document = deserializeMethod!.Invoke(_contentSerializer, [content.JsonContent])!;

        // 使用反射调用渲染服务的RenderDocumentAsync方法
        var renderMethod = GetType().GetMethods()
            .First(RenderDocumentAsync =>
                RenderDocumentAsync.Name == nameof(RenderDocumentAsync) &&
                RenderDocumentAsync.IsGenericMethodDefinition)
            ?.MakeGenericMethod(actualType);

        return await (Task<string>)renderMethod!.Invoke(this, [document, content])!;

    }

    public async Task<string> RenderDocumentAsync(Guid id)
    {
        //根据ID获取文档
        var contentRaw = await _repository.GetRawContentAsync(id);
        if (contentRaw == null)
        {
            throw new KeyNotFoundException($"未找到ID为{id}的内容");
        }

        return await RenderDocumentAsync(contentRaw);
    }

    public async Task<string> RenderDocumentBySlugAsync(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ArgumentNullException(nameof(slug), "slug不能为空");
        }

        // 根据slug获取内容
        var content = await _repository.GetContentIdBySlugAsync(slug);
        if (content == null)
        {
            throw new KeyNotFoundException($"未找到slug为{slug}的内容");
        }

#if !DEBUG

        // 根据ID获取文档
        if (content.Status != ContentStatus.Published)
        {
            throw new KeyNotFoundException($"slug为{slug}的内容未发布");
        }
#endif

        return await RenderDocumentAsync(content);
    }
}