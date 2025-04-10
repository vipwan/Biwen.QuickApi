﻿// Licensed to the Biwen.QuickApi.Test under one or more agreements.
// The Biwen.QuickApi.Test licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi.Test Author: vipwa, Github: https://github.com/vipwan
// 
// Modify Date: 2024-09-21 00:58:08 HtmlRendererTest.cs

using Alba;
using Biwen.QuickApi.Contents;
using Biwen.QuickApi.Contents.Rendering;
using Biwen.QuickApi.Test.Components;

namespace Biwen.QuickApi.Test;

public class BlazorRendererServiceTest : IClassFixture<TestBase.QuickApiTestFactory>
{

    private readonly ITestOutputHelper _testOutput;
    private readonly TestBase.QuickApiTestFactory _factory;

    public BlazorRendererServiceTest(TestBase.QuickApiTestFactory factory, ITestOutputHelper testOutput)
    {
        _factory = factory;
        _testOutput = testOutput;
    }


    //测试HtmlRenderer:
    [Theory]
    [InlineData("Hello from the Render Message component!")]
    [InlineData("hello 2")]
    [InlineData("hello 3")]
    public async Task ShouldRenderHtml(string message)
    {
        //var env = WebApplication.CreateSlimBuilder().Environment;

        //IServiceCollection services = new ServiceCollection();
        //services.AddLogging();

        ////添加Env
        //services.AddSingleton(env);

        ////添加Configuration
        //var configuration = new ConfigurationBuilder().Build();
        //services.AddSingleton<IConfiguration>(configuration);

        //services.AddHttpContextAccessor();
        ////提供Blazor组件服务
        //services.AddRazorComponents().AddInteractiveServerComponents();

        ////提供Blazor组件渲染服务
        //services.AddScoped<BlazorRendererService>();

        //var serviceProvider = services.BuildServiceProvider();

        //var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
        //httpContextAccessor.HttpContext = new DefaultHttpContext();

        //var service = serviceProvider.GetRequiredService<BlazorRendererService>();

        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<BlazorRendererService>();

        var pairs = new Dictionary<string, object?>
        {
            { "Message", message }
        };
        var html = await service.RenderAsync<RenderMessage>(pairs);

        _testOutput.WriteLine(html);
        Assert.Contains(message, html);

    }

    //使用alba测试 HtmlRenderer:
    [Theory]
    [InlineData("Hello from the Render Message component!")]
    [InlineData("hello 2")]
    [InlineData("hello 3")]
    public async Task ShouldRenderHtml_With_Alba(string message)
    {
        var host = await AlbaHost.For<DemoWeb.Program>();

        using var scope = host.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<BlazorRendererService>();
        var pairs = new Dictionary<string, object?>
        {
            { "Message", message }
        };
        var html = await service.RenderAsync<RenderMessage>(pairs);

        _testOutput.WriteLine(html);
        Assert.Contains(message, html);
    }

    //验证Biwen.QuickApi.Contents.Rendering.RazorDocumentRenderService

    [Theory]
    [InlineData("this is content 1!")]
    [InlineData("this is content 2!")]
    public async Task RazorDocumentRenderService_ShouldRenderHtml(string title)
    {
        var host = await AlbaHost.For<DemoWeb.Program>();
        using var scope = host.Services.CreateScope();

        var service = scope.ServiceProvider.GetRequiredService<IDocumentRenderService>();


        var samplePage = new SamplePage
        {
            Title = new QuickApi.Contents.FieldTypes.TextFieldType
            {
                Value = title
            },
            Content = new QuickApi.Contents.FieldTypes.MarkdownFieldType
            {
                Value = "<h3>Hello from the Render Message component!</h3>"
            },
            Description = new QuickApi.Contents.FieldTypes.MarkdownFieldType
            {
                Value = "Hello from the Render Message component!"
            },
            Tags = new QuickApi.Contents.FieldTypes.ArrayFieldType
            {
                Value = "Tag1,Tag2"
            }
        };

        //测试内部方法:
        var svc = (RazorDocumentRenderService)service;

        var html = await svc.RenderDocumentAsync(samplePage, new QuickApi.Contents.Domain.Content
        {
            Id = Guid.NewGuid(),
            Slug = "Slug Test"
        });

        _testOutput.WriteLine(html);
        Assert.Contains(title, html);
    }

}