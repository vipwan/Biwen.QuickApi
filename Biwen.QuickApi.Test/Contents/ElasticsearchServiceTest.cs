// Licensed to the Biwen.QuickApi.Test under one or more agreements.
// The Biwen.QuickApi.Test licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi.Test Author: 万雅虎, Github: https://github.com/vipwan
// 
// Modify Date: 2025-04-30 16:38:21 ElasticsearchServiceTest.cs

using Biwen.QuickApi.Contents.Domain;
using Biwen.QuickApi.Contents.Searching;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using Biwen.QuickApi.Contents; // 引入ContentFieldType枚举

namespace Biwen.QuickApi.Test.Contents;

public class ElasticsearchServiceTest : IAsyncLifetime
{
    private readonly ElasticsearchClient _client;
    private readonly ILogger<ElasticsearchService> _logger;
    private readonly ElasticsearchService _service;
    private readonly ITestOutputHelper _output;
    private const string IndexName = "biwen.quickapi.contents";
    private readonly List<Content> _testContents = new();

    public ElasticsearchServiceTest(ITestOutputHelper output)
    {
        _output = output;


        //测试前需要确保Elasticsearch服务已启动并运行!

        // 从appsettings.json读取ElasticSearch连接字符串
        //var config = new ConfigurationBuilder()
        //    .AddJsonFile("appsettings.json", optional: false)
        //    .Build();


        var elasticConnectionString = "http://elastic:A6k}8!Zu9r~_bD.yZt8{+R@localhost:62077";

        _output.WriteLine($"Elasticsearch连接字符串: {elasticConnectionString}");

        // 创建真实的ElasticsearchClient
        _client = new ElasticsearchClient(new Uri(elasticConnectionString));

        // 使用NullLogger或TestLogger
        _logger = new NullLogger<ElasticsearchService>();

        // 创建ElasticsearchService实例
        _service = new ElasticsearchService(_client, _logger);
    }

    /// <summary>
    /// 测试执行前的初始化操作
    /// </summary>
    public async Task InitializeAsync()
    {
        _output.WriteLine("开始测试初始化...");

        // 创建测试数据
        for (int i = 1; i <= 5; i++)
        {
            _testContents.Add(CreateSampleContent($"测试文档 {i}", $"test-content-{i}", "blog"));
            _testContents.Add(CreateSampleContent($"新闻 {i}", $"news-{i}", "news"));
        }

        // 初始化索引（删除旧索引并创建新索引）
        await _service.RebuildIndexAsync(_testContents);

        // 等待索引刷新
        await Task.Delay(1000);

        _output.WriteLine($"测试初始化完成，共创建 {_testContents.Count} 个测试文档");
    }

    /// <summary>
    /// 测试执行后的清理操作
    /// </summary>
    public async Task DisposeAsync()
    {
        _output.WriteLine("开始测试清理...");

        try
        {
            // 删除测试索引
            var existsResponse = await _client.Indices.ExistsAsync(IndexName);
            if (existsResponse.Exists)
            {
                await _client.Indices.DeleteAsync(IndexName);
                _output.WriteLine($"删除索引 {IndexName} 成功");
            }
        }
        catch (Exception ex)
        {
            _output.WriteLine($"清理索引时出错: {ex.Message}");
        }
    }

    #region 健康检查测试

    [Fact]
    public async Task HealthCheckAsync_ShouldReturnTrue_WhenElasticsearchIsRunning()
    {
        // Act
        var result = await _service.HealthCheckAsync();

        // Assert
        Assert.True(result, "Elasticsearch应该处于运行状态");
    }

    #endregion

    #region 索引操作测试

    [Fact]
    public async Task InitializeIndexAsync_ShouldCreateIndex_WhenIndexNotExists()
    {
        // Arrange - 确保索引不存在
        var existsResponse = await _client.Indices.ExistsAsync(IndexName);
        if (existsResponse.Exists)
        {
            await _client.Indices.DeleteAsync(IndexName);
        }

        // Act
        await _service.InitializeIndexAsync();

        // Assert
        existsResponse = await _client.Indices.ExistsAsync(IndexName);
        Assert.True(existsResponse.Exists, "索引应该已创建");
    }

    [Fact]
    public async Task RebuildIndexAsync_ShouldRecreateIndexWithContents()
    {
        // Arrange
        var sampleContents = new List<Content>
        {
            CreateSampleContent("重建索引测试", "rebuild-test", "test")
        };

        // Act
        await _service.RebuildIndexAsync(sampleContents);
        await Task.Delay(1000); // 等待索引刷新

        // Assert - 验证索引存在
        var existsResponse = await _client.Indices.ExistsAsync(IndexName);
        Assert.True(existsResponse.Exists, "索引应该已创建");
    }

    #endregion

    #region 文档操作测试

    [Fact]
    public async Task AddOrUpdateDocumentAsync_ShouldAddDocument_WhenDocumentNotExists()
    {
        // Arrange
        var content = CreateSampleContent("新增文档测试", "add-test", "test");

        // Act
        await _service.AddOrUpdateDocumentAsync(content);
        await Task.Delay(1000); // 等待索引刷新

        // Assert
        var getResponse = await _client.GetAsync<ContentSearchResult>(IndexName, content.Id.ToString());
        Assert.True(getResponse.Found, "文档应该已添加到索引");
        Assert.Equal(content.Title, getResponse.Source?.Title);
    }

    [Fact]
    public async Task AddOrUpdateDocumentAsync_ShouldUpdateDocument_WhenDocumentExists()
    {
        // Arrange
        var content = CreateSampleContent("更新文档测试", "update-test", "test");

        // 先添加文档
        await _service.AddOrUpdateDocumentAsync(content);
        await Task.Delay(1000); // 等待索引刷新

        // 更新文档
        content.Title = "已更新的标题";

        // Act
        await _service.AddOrUpdateDocumentAsync(content);
        await Task.Delay(1000); // 等待索引刷新

        // Assert
        var getResponse = await _client.GetAsync<ContentSearchResult>(IndexName, content.Id.ToString());
        Assert.True(getResponse.Found, "文档应该已添加到索引");
        Assert.Equal("已更新的标题", getResponse.Source?.Title);
    }

    [Fact]
    public async Task DeleteDocumentAsync_ShouldRemoveDocument()
    {
        // Arrange
        var content = CreateSampleContent("删除文档测试", "delete-test", "test");

        // 先添加文档
        await _service.AddOrUpdateDocumentAsync(content);
        await Task.Delay(1000); // 等待索引刷新

        // Act
        await _service.DeleteDocumentAsync(content.Id);
        await Task.Delay(1000); // 等待索引刷新

        // Assert
        var getResponse = await _client.GetAsync<ContentSearchResult>(IndexName, content.Id.ToString());
        Assert.False(getResponse.Found, "文档应该已从索引中删除");
    }

    #endregion

    #region 搜索测试

    [Fact]
    public async Task SearchContentsAsync_ShouldReturnMatchingDocuments_WhenQueryIsProvided()
    {
        // Arrange
        await EnsureTestDataExists();

        // Act
        var result = await _service.SearchContentsAsync("测试", 1, 10);

        // Assert
        Assert.NotEmpty(result.Items);
        foreach (var item in result.Items)
        {
            _output.WriteLine($"搜索结果: {item.Title}");
        }
    }

    [Fact]
    public async Task SearchContentsAsync_ShouldReturnFilteredDocuments_WhenFilterIsProvided()
    {
        // Arrange
        await EnsureTestDataExists();

        // Act - 按内容类型过滤
        var result = await _service.SearchContentsAsync("", filter: "contentType:news");

        // Assert
        Assert.NotEmpty(result.Items);
        Assert.All(result.Items, item => Assert.Equal("news", item.ContentType));
    }

    [Fact]
    public async Task SearchContentsAsync_ShouldReturnSortedDocuments_WhenSortIsProvided()
    {
        // Arrange
        await EnsureTestDataExists();

        // Act - 按创建时间降序排序
        var result = await _service.SearchContentsAsync("", sort: "createdAt:desc");

        // Assert
        Assert.NotEmpty(result.Items);

        // 验证排序是否正确
        DateTime? lastDate = null;
        foreach (var item in result.Items)
        {
            if (lastDate.HasValue)
            {
                Assert.True(item.CreatedAt <= lastDate.Value, "应该按创建时间降序排序");
            }
            lastDate = item.CreatedAt;
        }
    }

    [Fact]
    public async Task SearchContentsAsync_ShouldReturnHighlightedContent_WhenHighlightEnabled()
    {
        // Arrange
        await EnsureTestDataExists();

        // 添加一个包含指定关键词的文档
        var specialContent = CreateSampleContent("高亮测试文档", "highlight-test", "blog"); specialContent.JsonContent = JsonSerializer.Serialize(new[]
        {
            new { fieldName = "Content", value = "这是一个包含高亮关键词的测试文档内容", fieldType = Biwen.QuickApi.Contents.ContentFieldType.Text }
        });
        await _service.AddOrUpdateDocumentAsync(specialContent);
        await Task.Delay(1000); // 等待索引刷新

        // Act
        var result = await _service.SearchContentsAsync("高亮关键词", enableHighlight: true);

        // Assert
        Assert.NotEmpty(result.Items);
        //var highlightedItem = result.Items.FirstOrDefault(i => i.JsonContent.Contains("<mark>高亮关键词</mark>"));
        //Assert.NotNull(highlightedItem);
        //_output.WriteLine($"高亮结果: {highlightedItem.JsonContent}");
    }

    [Fact]
    public async Task SearchContentsAsync_ShouldReturnFacets_WhenFacetsRequested()
    {
        // Arrange
        await EnsureTestDataExists();

        // Act
        var result = await _service.SearchContentsAsync("", facets: new[] { "ContentType" });

        // Assert
        Assert.NotEmpty(result.Items);

        // TODO: 验证分面结果，但当前SearchContentsAsync方法没有返回分面结果
        // 这需要修改IPagedList接口或SearchContentsAsync方法以返回分面信息
    }

    #endregion

    #region 嵌套字段测试

    [Fact]
    public async Task SearchContentsAsync_ShouldFindNestedFieldContent()
    {
        // Arrange
        var nestedContent = CreateSampleContent("嵌套字段测试", "nested-test", "test"); nestedContent.JsonContent = JsonSerializer.Serialize(new[]
        {
            new { fieldName = "Title", value = "嵌套字段标题", fieldType = Biwen.QuickApi.Contents.ContentFieldType.Text },
            new { fieldName = "Content", value = "这是一个测试嵌套字段内容搜索的示例", fieldType = Biwen.QuickApi.Contents.ContentFieldType.Text }
        });

        await _service.AddOrUpdateDocumentAsync(nestedContent);
        await Task.Delay(1000); // 等待索引刷新

        // Act - 搜索嵌套字段中的内容
        var result = await _service.SearchContentsAsync("嵌套字段内容搜索");

        // Assert
        Assert.NotEmpty(result.Items);
        Assert.Contains(result.Items, i => i.Id == nestedContent.Id);
    }

    [Fact]
    public async Task SearchContentsAsync_ShouldFilterBySpecificNestedField()
    {
        // Arrange
        var nestedContent1 = CreateSampleContent("嵌套字段过滤测试1", "nested-filter-test1", "test");
        nestedContent1.JsonContent = JsonSerializer.Serialize(new[]
        {
            new { fieldName = "Category", value = "技术文章", fieldType = Biwen.QuickApi.Contents.ContentFieldType.Text },
            new { fieldName = "Content", value = "这是技术文章的内容", fieldType = Biwen.QuickApi.Contents.ContentFieldType.Text }
        });

        var nestedContent2 = CreateSampleContent("嵌套字段过滤测试2", "nested-filter-test2", "test");
        nestedContent2.JsonContent = JsonSerializer.Serialize(new[]
        {
            new { fieldName = "Category", value = "生活随笔", fieldType = Biwen.QuickApi.Contents.ContentFieldType.Text },
            new { fieldName = "Content", value = "这是生活随笔的内容", fieldType = Biwen.QuickApi.Contents.ContentFieldType.Text }
        });

        await _service.AddOrUpdateDocumentAsync(nestedContent1);
        await _service.AddOrUpdateDocumentAsync(nestedContent2);
        await Task.Delay(1000); // 等待索引刷新

        // 只过滤contentType 实际上集合为2
        var r2 = await _service.SearchContentsAsync("内容", filter: "contentType:test");
        Assert.Equal(2, r2.Items.Count);

        // Act - 按特定嵌套字段过滤,实际集合应该为1
        var result = await _service.SearchContentsAsync("内容", filter: "field:Category=技术文章");

        // Assert
        Assert.NotEmpty(result.Items);
        Assert.Contains(result.Items, i => i.Id == nestedContent1.Id);
        Assert.DoesNotContain(result.Items, i => i.Id == nestedContent2.Id);
    }

    #endregion

    #region 帮助方法

    /// <summary>
    /// 创建示例内容对象
    /// </summary>
    private Content CreateSampleContent(string title, string slug, string contentType)
    {
        return new Content
        {
            Id = Guid.NewGuid(),
            Title = title,
            Slug = slug,
            ContentType = contentType,
            Status = ContentStatus.Published,
            CreatedAt = DateTime.UtcNow.AddSeconds(0 - Random.Shared.Next(1000, 9999)),
            UpdatedAt = DateTime.UtcNow.AddSeconds(0 - Random.Shared.Next(1000, 9999)),
            JsonContent = JsonSerializer.Serialize(new[]
            {
                new { fieldName = "Title", value = title, fieldType = ContentFieldType.Text },
                new { fieldName = "Content", value = $"这是{title}的内容描述", fieldType = ContentFieldType.Text },
                new { fieldName = "Tags", value = "测试,示例", fieldType = ContentFieldType.Text }
            })
        };
    }

    /// <summary>
    /// 确保测试数据存在
    /// </summary>
    private async Task EnsureTestDataExists()
    {
        var countResponse = await _client.CountAsync(IndexName);
        if (countResponse.Count < 5)
        {
            _output.WriteLine("测试数据不足，重新创建测试数据...");
            await InitializeAsync();
        }
    }

    #endregion
}
