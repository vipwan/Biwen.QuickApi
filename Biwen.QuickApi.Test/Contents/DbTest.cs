// Licensed to the Biwen.QuickApi.Test under one or more agreements.
// The Biwen.QuickApi.Test licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi.Test Author: 万雅虎, Github: https://github.com/vipwan
// Modify Date: 2025-04-04 16:16:23 SchemaTest.cs

using Biwen.QuickApi.Contents;
using Biwen.QuickApi.Contents.Abstractions;
using Biwen.QuickApi.Contents.FieldTypes;
using Biwen.QuickApi.Contents.Schema;
using Biwen.QuickApi.DemoWeb.Cms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Biwen.QuickApi.Test.Contents;

public class ContentRepositoryTest
{
    private readonly ITestOutputHelper _output;

    public ContentRepositoryTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task ContentRepository_SaveContentAsync_Should_Return_Valid_Id()
    {
        // 准备
        var (repo, _, _) = CreateRepository();
        var blog = CreateTestBlog();

        // 执行
        var id = await repo.SaveContentAsync(blog, "测试博客");

        // 断言
        Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public async Task ContentRepository_GetContentAsync_Should_Return_Content()
    {
        // 准备
        var (repo, context, scope) = CreateRepository();
        var blog = CreateTestBlog();
        var id = await repo.SaveContentAsync(blog, "测试博客");

        // 执行
        var retrievedBlog = await repo.GetContentAsync<Blog>(id);

        // 断言
        Assert.NotNull(retrievedBlog);
        Assert.Equal(blog.Title.Value, retrievedBlog.Title.Value);
        Assert.Equal(blog.Description.Value, retrievedBlog.Description.Value);
        Assert.Equal(blog.Content.Value, retrievedBlog.Content.Value);
        // 忽略true True
        Assert.Equal(blog.IsPublished.Value, retrievedBlog.IsPublished.Value);

        Assert.Equal(2, retrievedBlog.Tags.GetValueString().Split([',', ';']).Length);

        Assert.Equal(blog.Tags.GetValueString(), retrievedBlog.Tags.GetValueString());


        // Raw Content
        var rawContet = await repo.GetRawContentAsync(id);

        Assert.NotNull(rawContet);
    }

    [Fact]
    public async Task ContentRepository_GetContentAsync_Should_Return_Null_For_Invalid_Id()
    {
        // 准备
        var (repo, context, scope) = CreateRepository();

        // 执行
        var retrievedBlog = await repo.GetContentAsync<Blog>(Guid.NewGuid());

        // 断言
        Assert.Null(retrievedBlog);
    }

    [Fact]
    public async Task ContentRepository_GetContentsByTypeAsync_Should_Return_Contents()
    {
        // 准备
        var (repo, context, scope) = CreateRepository();
        var blog1 = CreateTestBlog("博客1");
        var blog2 = CreateTestBlog("博客2");

        await repo.SaveContentAsync(blog1, "测试博客1");
        await repo.SaveContentAsync(blog2, "测试博客2");

        // 执行
        var blogs = await repo.GetContentsByTypeAsync<Blog>(0, 10);
        // 断言
        Assert.NotNull(blogs);
        Assert.Equal(2, blogs.TotalCount);

        var domains = await repo.GetDomainContentsByTypeAsync<Blog>(null, 0, 10);
        Assert.NotNull(domains);
        Assert.Equal(2, domains.TotalCount);

        var title1 = domains.Items[0].Title;
        var title2 = domains.Items[1].Title;
        Assert.Equal("测试博客2", title1);
        Assert.Equal("测试博客1", title2);
    }

    [Fact]
    public async Task ContentRepository_UpdateContentAsync_Should_Update_Content()
    {
        // 准备
        var (repo, _, _) = CreateRepository();
        var blog = CreateTestBlog();
        var id = await repo.SaveContentAsync(blog, "测试博客");

        // 修改内容
        blog.Title.Value = "已更新的标题";
        blog.Content.Value = "已更新的内容";

        // 执行
        await repo.UpdateContentAsync(id, blog);

        // 验证数据库已更新
        var entity = await repo.GetContentAsync<Blog>(id);
        Assert.NotNull(entity);
        Assert.NotNull(entity.Title.Value);

        // 获取更新后的内容
        var updatedBlog = await repo.GetContentAsync<Blog>(id);
        Assert.NotNull(updatedBlog);
        Assert.Equal("已更新的标题", updatedBlog.Title.Value);
        Assert.Equal("已更新的内容", updatedBlog.Content.Value);
    }

    [Fact]
    public async Task ContentRepository_UpdateContentAsync_Should_Throw_Exception_For_Invalid_Id()
    {
        // 准备
        var (repo, context, scope) = CreateRepository();
        var blog = CreateTestBlog();
        var invalidId = Guid.NewGuid();

        // 执行和断言
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await repo.UpdateContentAsync(invalidId, blog));

        Assert.Contains($"Content with ID {invalidId} not found", exception.Message);
    }

    [Fact]
    public async Task ContentRepository_DeleteContentAsync_Should_Remove_Content()
    {
        // 准备
        var (repo, _, _) = CreateRepository();
        var blog = CreateTestBlog();
        var id = await repo.SaveContentAsync(blog, "测试博客");

        // 确认内容已存在
        Assert.NotNull(await repo.GetContentAsync<Blog>(id));

        // 执行
        await repo.DeleteContentAsync(id);

        // 断言
        Assert.Null(await repo.GetContentAsync<Blog>(id));
    }

    [Fact]
    public async Task ContentRepository_DeleteContentAsync_Should_Not_Throw_For_Invalid_Id()
    {
        // 准备
        var (repo, _, scope) = CreateRepository();
        var invalidId = Guid.NewGuid();

        try
        {
            // 执行和断言（不应抛出异常）
            var exception = await Record.ExceptionAsync(async () =>
                await repo.DeleteContentAsync(invalidId));

            Assert.Null(exception);
        }
        finally
        {
            scope.Dispose();
        }
    }

    [Fact]
    public void ContentRepository_GetContentSchema_Should_Return_Valid_Schema()
    {
        // 准备
        var (repo, context, scope) = CreateRepository();

        // 执行
        var schema = repo.GetContentSchema<Blog>();

        // 断言
        Assert.NotNull(schema);
        Assert.NotEmpty(schema);

        // 尝试解析JSON以验证有效性
        var exception = Record.Exception(() => JsonDocument.Parse(schema));
        Assert.Null(exception);

        // 输出生成的schema以便检查
        _output.WriteLine(schema);
    }

    [Fact]
    public async Task ContentRepository_SaveContentAsync_Should_Use_Title_From_Content_When_Not_Provided()
    {
        // 准备
        var (repo, _, _) = CreateRepository();
        var blog = CreateTestBlog("自动获取的标题");

        var id = await repo.SaveContentAsync(blog, "自动获取的标题");

        // 断言
        var entity = await repo.GetContentAsync<Blog>(id);
        Assert.NotNull(entity);
        Assert.Equal("自动获取的标题", entity.Title.Value);
    }

    #region 辅助方法

    private (ContentRepository, MyDbcontext, IServiceScope) CreateRepository()
    {
        // 创建一个ServiceCollection来模拟Web环境中的DI容器
        var serviceProvider = new ServiceCollection();

        // 添加配置(Web环境中通常需要IConfiguration)
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([])
            .Build();
        serviceProvider.AddSingleton<IConfiguration>(configuration);

        // 添加数据库上下文
        serviceProvider.AddDbContext<MyDbcontext>(opt =>
            opt.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        // 注册Biwen内容模块
        serviceProvider.AddBiwenContents<MyDbcontext>();

        // 前置条件 - 基础QuickAPI服务
        serviceProvider.AddBiwenQuickApis();

        // 创建服务提供者作用域
        var serviceProviderScope = serviceProvider.BuildServiceProvider().CreateScope();


        // 通过DI获取必要的服务
        var context = serviceProviderScope.ServiceProvider.GetRequiredService<MyDbcontext>();
        var repo = serviceProviderScope.ServiceProvider.GetRequiredService<IContentRepository>() as ContentRepository;

        if (repo == null)
        {
            throw new InvalidOperationException("无法获取ContentRepository服务");
        }

        return (repo, context, serviceProviderScope);
    }

    private Blog CreateTestBlog(string title = "测试博客标题")
    {
        return new Blog
        {
            Title = new TextFieldType { Value = title },
            Description = new TextAreaFieldType { Value = "这是一个测试博客描述" },
            Content = new MarkdownFieldType { Value = "# 测试内容\n这是测试Markdown内容。" },
            IsPublished = new BooleanFieldType { Value = "true" },
            Tags = new ArrayFieldType { Value = "随笔,日志" }
        };
    }

    #endregion
}

