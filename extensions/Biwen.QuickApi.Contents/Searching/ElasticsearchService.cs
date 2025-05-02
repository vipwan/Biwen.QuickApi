// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-04-30 14:44:48 ElasticsearchService.cs

using Biwen.QuickApi.Contents.Domain;
using Biwen.QuickApi.UnitOfWork.Pagenation;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.Analysis;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Clients.Elasticsearch.QueryDsl;
using System.Text.Json;

namespace Biwen.QuickApi.Contents.Searching;

/// <summary>
/// 基于ES实现的内容搜索服务
/// </summary>
/// <param name="client"></param>
/// <param name="logger"></param>
public class ElasticsearchService(
    ElasticsearchClient client,
    ILogger<ElasticsearchService> logger) : IContentSearchService
{
    // Elasticsearch索引名称
    private const string IndexName = "biwen.quickapi.contents";

    /// <summary>
    /// 添加或更新单个文档到索引
    /// </summary>
    /// <param name="blog">要添加或更新的文档</param>
    public async Task AddOrUpdateDocumentAsync(Content content)
    {
        // 创建一个可以被ES正确索引的对象
        var indexContent = new
        {
            content.Id,
            content.Title,
            content.Slug,
            content.Status,
            content.CreatedAt,
            content.UpdatedAt,
            content.ContentType,
            // 将JsonContent解析为对象数组
            JsonContent = JsonSerializer.Deserialize<ContentFieldValue[]>(content.JsonContent)
        };

        // 执行索引文档请求，如果文档存在则更新，不存在则添加
        var response = await client.IndexAsync(indexContent, idx =>
            idx.Index(IndexName).Id(content.Id.ToString()));  // 指定索引名称和文档ID

        // 检查操作是否成功
        if (!response.IsValidResponse)
        {
            // 记录添加或更新文档失败的错误
            logger.LogError("添加或更新文档失败: {ErrorReason}", response.DebugInformation);
        }
    }

    /// <summary>
    /// 从索引中删除指定ID的博客文档
    /// </summary>
    /// <param name="id">要删除的ID</param>
    public async Task DeleteDocumentAsync(Guid id)
    {
        var deleteRequest = new DeleteRequest(IndexName, id.ToString());
        // 执行删除文档请求
        var response = await client.DeleteAsync(deleteRequest);

        // 检查操作是否成功，忽略文档不存在的情况
        if (!response.IsValidResponse && response.Result != Result.NotFound)
        {
            // 记录删除文档失败的错误
            logger.LogError("删除文档失败: {ErrorReason}", response.DebugInformation);
        }
    }

    /// <summary>
    /// 检查Elasticsearch服务的健康状态
    /// </summary>
    /// <returns>服务是否正常运行</returns>
    public async Task<bool> HealthCheckAsync()
    {
        try
        {
            // 调用ES集群健康API检查服务状态
            var response = await client.Cluster.HealthAsync();
            // 返回响应是否有效，表示ES服务是否可用
            return response.IsValidResponse;
        }
        catch (Exception ex)
        {
            // 记录健康检查失败的异常信息
            logger.LogError(ex, "Elasticsearch 健康检查失败");
            // 发生异常时返回false，表示服务不可用
            return false;
        }
    }

    /// <summary>
    /// 初始化Elasticsearch索引，包括设置分片、副本、分析器和字段映射
    /// </summary>
    public async Task InitializeIndexAsync()
    {
        // 首先检查索引是否已存在
        var indexExists = await client.Indices.ExistsAsync(IndexName);

        if (indexExists.Exists)
        {
            return;//已存在,直接返回
        }

        // 创建索引请求对象，指定索引名称和配置
        var createIndexRequest = new CreateIndexRequest(IndexName)
        {
            // 配置索引设置
            Settings = new IndexSettings
            {
                NumberOfShards = 1,  // 设置1个分片
                NumberOfReplicas = 0,  // 设置0个副本（适用于开发环境）
                // 配置文本分析器
                Analysis = new IndexSettingsAnalysis
                {
                    // 定义自定义分析器
                    Analyzers = new Analyzers(new Dictionary<string, IAnalyzer>
                    {
                        // 创建名为content_analyzer的自定义分析器
                        ["content_analyzer"] = new CustomAnalyzer
                        {
                            Tokenizer = "standard",  // 使用标准分词器
                            Filter = ["lowercase", "asciifolding", "stop"]  // 应用小写、ASCII转换和停用词过滤
                        }
                    })
                }
            },
            // 配置字段映射
            Mappings = new TypeMapping
            {
                Properties = new Properties
                {
                    // 配置Title字段为文本类型，使用自定义分析器和关键字字段
                    { nameof(Content.Title).ToCamelCase(), new TextProperty
                        {
                            Analyzer = "content_analyzer",  // 使用自定义内容分析器
                            Fields = new Properties
                            {
                                // 添加keyword子字段用于精确匹配和排序
                                { "keyword", new KeywordProperty { IgnoreAbove = 256 } }
                            }
                        }
                    },
                    // 配置JsonContent字段为嵌套类型，用于处理JSON数组
                    { nameof(Content.JsonContent).ToCamelCase(), new NestedProperty
                        {
                            Properties = new Properties
                            {
                                // 配置fieldName字段为关键字类型
                                { nameof(ContentFieldValue.FieldName).ToCamelCase(), new KeywordProperty() },
                                // 配置value字段为文本类型，使用自定义分析器和关键字子字段
                                { nameof(ContentFieldValue.Value).ToCamelCase(), new TextProperty
                                    {
                                        Analyzer = "content_analyzer",
                                        Fields = new Properties
                                        {
                                            { "keyword", new KeywordProperty { IgnoreAbove = 256 } }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    // 配置ContentType字段为关键字类型
                    { nameof(Content.ContentType).ToCamelCase(), new KeywordProperty() },
                    // 配置Slug字段为关键字类型
                    { nameof(Content.Slug).ToCamelCase(), new KeywordProperty() },
                    // 配置Status字段为关键字类型.Published.字符串
                    { nameof(Content.Status).ToCamelCase(), new KeywordProperty() },
                    // 配置CreatedAt字段为日期类型
                    { nameof(Content.CreatedAt).ToCamelCase(), new DateProperty() },
                    // 配置UpdatedAt字段为日期类型
                    { nameof(Content.UpdatedAt).ToCamelCase(), new DateProperty() }
                }
            }
        };

        // 执行创建索引请求
        var response = await client.Indices.CreateAsync(createIndexRequest);

        // 检查索引创建是否成功
        if (!response.IsValidResponse)
        {
            // 记录索引创建失败的错误
            logger.LogError("创建索引失败: {ErrorReason}", response.DebugInformation);
            // 抛出异常，终止程序执行
            throw new InvalidOperationException($"创建索引失败: {response.DebugInformation}");
        }
    }

    public async Task RebuildIndexAsync(IEnumerable<Content> allContents)
    {
        // 删除并重建索引
        try
        {
            // 检查索引是否存在
            var existsResponse = await client.Indices.ExistsAsync(IndexName);
            if (existsResponse.Exists)
            {
                // 如果索引存在，则删除它
                await client.Indices.DeleteAsync(IndexName);
            }
        }
        catch (Exception ex)
        {
            // 记录删除索引过程中的错误
            logger.LogError(ex, "删除索引失败");
            // 索引可能不存在，忽略错误继续执行
        }

        // 重新创建索引并配置索引结构
        await InitializeIndexAsync();

        // 批量添加文档到索引
        if (allContents.Any())
        {
            // 创建批量操作请求对象，并正确初始化Operations集合
            var bulkRequest = new BulkRequest
            {
                Operations = new List<IBulkOperation>()
            };

            // 遍历所有文档，将每篇内容添加到批量请求中
            foreach (var content in allContents)
            {
                try
                {
                    // 为每篇文档创建适合索引的结构
                    var indexContent = new
                    {
                        content.Id,
                        content.Title,
                        content.Slug,
                        content.Status,
                        content.CreatedAt,
                        content.UpdatedAt,
                        content.ContentType,
                        // 将JsonContent解析为对象数组
                        JsonContent = JsonSerializer.Deserialize<List<ContentFieldValue>>(content.JsonContent)
                    };

                    // 为每篇文档创建索引操作
                    var operation = new BulkIndexOperation<object>(indexContent)
                    {
                        Index = IndexName,  // 指定索引名称
                        Id = content.Id.ToString()  // 使用内容ID作为文档ID
                    };
                    // 将操作添加到批量请求中
                    bulkRequest.Operations.Add(operation);
                }
                catch (JsonException ex)
                {
                    logger.LogError(ex, "解析JsonContent失败，内容ID: {Id}", content.Id);
                    // 继续处理下一个内容
                }
            }

            // 执行批量索引请求
            var bulkResponse = await client.BulkAsync(bulkRequest);

            // 检查批量操作是否成功
            if (!bulkResponse.IsValidResponse)
            {
                // 记录批量索引失败的错误信息
                logger.LogError("批量索引创建失败: {ErrorReason}", bulkResponse.DebugInformation);
            }
        }
    }

    public async Task<IPagedList<ContentSearchResult>> SearchContentsAsync(
        string query,
        int page = 1,
        int pageSize = 10,
        string? filter = null,
        string? sort = null,
        bool enableHighlight = true,
        string[]? facets = null)
    {
        // 计算分页起始位置
        var from = (page - 1) * pageSize;

        // 创建搜索请求对象
        var searchRequest = new SearchRequest<ContentSearchResult>(IndexName)
        {
            From = from,  // 设置起始位置
            Size = pageSize,  // 设置返回文档数量
            Query = BuildQuery(query, filter)  // 构建查询条件
        };

        // 处理排序选项
        var sortOptions = BuildSort(sort);
        if (sortOptions != null && sortOptions.Count > 0)
        {
            // 如果有排序选项，设置搜索请求的排序规则
            searchRequest.Sort = sortOptions;
        }

        // 处理高亮显示
        if (enableHighlight)
        {
            // 配置高亮显示选项
            searchRequest.Highlight = new Highlight
            {
                // 指定需要高亮显示的字段及其配置
                Fields = new Dictionary<Field, HighlightField>
                {
                    // 配置标题字段的高亮显示
                    [new Field(nameof(Content.Title).ToCamelCase())] = new HighlightField
                    {
                        PreTags = ["<mark>"],  // 高亮前缀标签
                        PostTags = ["</mark>"]  // 高亮后缀标签
                    },
                    // 配置嵌套字段的高亮显示
                    [new Field($"{nameof(Content.JsonContent).ToCamelCase()}.value")] = new HighlightField
                    {
                        PreTags = ["<mark>"],  // 高亮前缀标签
                        PostTags = ["</mark>"],  // 高亮后缀标签
                        FragmentSize = 150,  // 每个高亮片段的最大长度
                        NumberOfFragments = 3,  // 返回最多3个高亮片段
                        HighlightQuery = new NestedQuery
                        {
                            Path = new Field(nameof(Content.JsonContent).ToCamelCase()),
                            Query = new MatchQuery(new Field($"{nameof(Content.JsonContent).ToCamelCase()}.value"))
                            {
                                Query = query
                            }
                        }
                    }
                }
            };
        }

        // 处理分面搜索
        if (facets != null && facets.Length > 0)
        {
            // 初始化聚合字典
            searchRequest.Aggregations = new Dictionary<string, Aggregation>();

            // 遍历所有请求的分面字段
            foreach (var facet in facets)
            {
                // 根据字段名称添加不同的聚合配置
                if (facet.Equals(nameof(Content.ContentType), StringComparison.OrdinalIgnoreCase))
                {
                    // 添加Category分面聚合
                    searchRequest.Aggregations["contentTypes"] = new TermsAggregation
                    {
                        Field = nameof(Content.ContentType).ToCamelCase(),  // 指定聚合字段
                        Size = 20  // 返回最多20个分面值
                    };
                }
                //else if (facet.Equals(nameof(Content.Author), StringComparison.OrdinalIgnoreCase))
                //{
                //    // 添加Author分面聚合
                //    searchRequest.Aggregations["authors"] = new TermsAggregation
                //    {
                //        Field = nameof(Content.Author).ToCamelCase(),  // 指定聚合字段
                //        Size = 20  // 返回最多20个分面值
                //    };
                //}
                //else if (facet.Equals(nameof(Content.Tags), StringComparison.OrdinalIgnoreCase))
                //{
                //    // 添加Tags分面聚合
                //    searchRequest.Aggregations["tags"] = new TermsAggregation
                //    {
                //        Field = nameof(Content.Tags).ToCamelCase(),  // 指定聚合字段
                //        Size = 30  // 返回最多30个分面值
                //    };
                //}
            }
        }

        // 执行搜索请求
        var searchResponse = await client.SearchAsync<ContentSearchResult>(searchRequest);

        // 检查搜索是否成功
        if (!searchResponse.IsValidResponse)
        {
            // 记录搜索失败的错误
            logger.LogError("搜索失败: {ErrorReason}", searchResponse.DebugInformation);
            // 返回空的搜索结果
            return new PagedList<ContentSearchResult>([], from, pageSize, 0, 0);
        }

        // 获取搜索命中的文档列表
        var hits = searchResponse.Documents.ToList();

        // 处理高亮显示结果
        if (enableHighlight && searchResponse.Hits?.Count > 0)
        {
            // 遍历所有命中的文档
            foreach (var hit in searchResponse.Hits)
            {
                // 查找对应的文档对象并处理高亮内容
                if (hit.Highlight != null && hits.FirstOrDefault(b => b.Id == Guid.Parse(hit.Id!)) is ContentSearchResult content)
                {
                    // 处理标题高亮
                    if (hit.Highlight.TryGetValue(nameof(content.Title).ToCamelCase(), out var titleHighlights) && titleHighlights.Any())
                    {
                        // 将原始标题替换为高亮标题
                        content.Title = titleHighlights.First();
                    }

                    // 处理嵌套字段的高亮
                    var jsonContentHighlightKey = $"{nameof(content.JsonContent).ToCamelCase()}.value";
                    if (hit.Highlight.TryGetValue(jsonContentHighlightKey, out var jsonContentHighlights) && jsonContentHighlights.Any())
                    {
                        // 将原始内容替换为高亮内容片段拼接结果
                        // 注意：这里只是创建一个展示用的字符串，实际的JsonContent结构不会改变
                        // content.JsonContent = string.Join("... ", jsonContentHighlights);

                        // 解析高亮内容片段为Json对象
                        //var highlightJson = JsonSerializer.Deserialize<List<ContentFieldValue>>(jsonContentHighlights.First());


                    }
                }
            }
        }

        // 处理分面聚合结果
        var facetResults = new Dictionary<string, IReadOnlyDictionary<string, int>>();
        if (facets != null && facets.Length > 0 && searchResponse.Aggregations != null)
        {
            // 遍历所有请求的分面字段
            foreach (var facet in facets)
            {
                // 处理不同类型的聚合
                if (facet.Equals(nameof(Content.ContentType), StringComparison.OrdinalIgnoreCase) &&
                    searchResponse.Aggregations.TryGetValue("contentTypes", out var contentTypeAgg))
                {
                    if (contentTypeAgg is MultiTermsAggregate termsAgg)
                    {
                        // 构建分面值和计数的字典
                        var facetDict = new Dictionary<string, int>();
                        foreach (var bucket in termsAgg.Buckets)
                        {
                            // 将每个桶的键和文档计数添加到字典中
                            facetDict[bucket.Key.ToString()!] = (int)bucket.DocCount;
                        }
                        // 将分面结果添加到结果集合
                        facetResults[facet.ToCamelCase()] = facetDict;
                    }
                }
                else if (facet.Equals("fieldNames", StringComparison.OrdinalIgnoreCase) &&
                         searchResponse.Aggregations.TryGetValue("fieldNames", out var fieldNamesAgg))
                {
                    // 处理嵌套聚合结果
                    if (fieldNamesAgg is NestedAggregate nestedAgg &&
                        nestedAgg.Aggregations.TryGetValue("fieldNames", out var fieldNameTermsAgg) &&
                        fieldNameTermsAgg is MultiTermsAggregate fieldNameTerms)
                    {
                        var facetDict = new Dictionary<string, int>();
                        foreach (var bucket in fieldNameTerms.Buckets)
                        {
                            facetDict[bucket.Key.ToString()!] = (int)bucket.DocCount;
                        }
                        facetResults["fieldNames"] = facetDict;
                    }
                }
            }
        }

        // 获取总命中数
        var totalHits = searchResponse.Total;

        // 返回搜索结果对象
        return new PagedList<ContentSearchResult>(searchResponse.Hits!.Select(h => h.Source!).ToList(), from, pageSize, 0, (int)totalHits);
    }

    /// <summary>
    /// 根据查询文本和过滤条件构建Elasticsearch查询
    /// </summary>
    /// <param name="queryText">搜索查询文本</param>
    /// <param name="filter">过滤条件字符串</param>
    /// <returns>构建的查询对象</returns>
    private Query BuildQuery(string queryText, string? filter)
    {
        // 创建布尔查询对象
        var boolQuery = new BoolQuery();

        // 如果查询文本为空，使用匹配所有文档的查询作为主查询
        if (string.IsNullOrWhiteSpace(queryText))
        {
            boolQuery.Must = [new MatchAllQuery()];
        }
        else
        {
            // 添加标准字段查询
            var standardFieldsQuery = new MultiMatchQuery
            {
                Fields = new[] {
                $"{nameof(Content.Title).ToCamelCase()}^2",  // 标题字段权重为2
                $"{nameof(Content.Slug).ToCamelCase()}",     // Slug字段
                $"{nameof(Content.ContentType).ToCamelCase()}", // 文档类型
            },
                Query = queryText,
                Type = TextQueryType.BestFields,
                Operator = Operator.And,
                Fuzziness = new Fuzziness("AUTO")
            };

            // 创建嵌套查询，用于搜索JsonContent中的value字段
            var nestedQuery = new NestedQuery
            {
                Path = new Field(nameof(Content.JsonContent).ToCamelCase()),
                Query = new MatchQuery(new Field($"{nameof(Content.JsonContent).ToCamelCase()}.value"))
                {
                    Query = queryText,
                    Fuzziness = new Fuzziness("AUTO")
                },
                ScoreMode = ChildScoreMode.Avg // 使用平均分数模式
            };

            // 将两个查询添加到Should子句中
            boolQuery.Should = [standardFieldsQuery, nestedQuery];
            // 设置最少匹配条件数为1，即至少匹配一个should子句
            boolQuery.MinimumShouldMatch = 1;
        }

        // 如果有过滤条件，添加过滤
        if (!string.IsNullOrWhiteSpace(filter))
        {
            // field:文档筛选器的优先级高于其他筛选器
            if (filter.StartsWith("field:"))
            {
                // 格式如 "field:Category=技术文章"
                var parts = filter.Substring(6).Split('=');
                if (parts.Length == 2)
                {
                    var fieldName = parts[0].Trim();
                    var fieldValue = parts[1].Trim();

                    // 添加日志输出
                    logger.LogInformation("构建嵌套字段过滤器，字段名: {FieldName}, 字段值: {FieldValue}",
                        fieldName, fieldValue);

                    // 添加针对特定字段名的嵌套查询过滤
                    var nestedFilter = new NestedQuery
                    {
                        Path = new Field(nameof(Content.JsonContent).ToCamelCase()),
                        Query = new BoolQuery
                        {
                            Must = [
                                // 使用小写的 fieldName，与JSON中的字段名一致
                                new TermQuery(new Field($"{nameof(Content.JsonContent).ToCamelCase()}.fieldName"))
                    {
                        Value = fieldName
                    },
                    // 使用小写的 value，与JSON中的字段名一致
                    new MatchQuery(new Field($"{nameof(Content.JsonContent).ToCamelCase()}.value"))
                    {
                        Query = fieldValue
                    }
                            ]
                        }
                    };

                    boolQuery.Filter ??= [];
                    boolQuery.Filter = boolQuery.Filter.Append(nestedFilter).ToArray();
                }
            }

            // 同时支持两种格式：contentType:test 和 contentType = 'test'
            else if (filter.Contains(':'))
            {
                var parts = filter.Split(':', 2);
                if (parts.Length == 2)
                {
                    var field = parts[0].Trim();
                    var value = parts[1].Trim();

                    if (field.Equals("contentType", StringComparison.OrdinalIgnoreCase))
                    {
                        boolQuery.Filter ??= [];
                        boolQuery.Filter = boolQuery.Filter.Append(
                            new TermQuery(nameof(Content.ContentType).ToCamelCase()!) { Value = value }
                        ).ToArray();
                    }
                    else if (field.Equals("status", StringComparison.OrdinalIgnoreCase))
                    {
                        boolQuery.Filter ??= [];
                        boolQuery.Filter = boolQuery.Filter.Append(
                            new TermQuery(nameof(Content.Status).ToCamelCase()!) { Value = value }
                        ).ToArray();
                    }
                }
            }
            else if (filter.Contains("contentType = "))
            {
                // 提取分类值，去除引号
                var value = filter.Replace("contentType = ", "").Trim('\'');
                // 添加分类过滤条件
                boolQuery.Filter ??= [];
                boolQuery.Filter = boolQuery.Filter.Append(
                    new TermQuery(nameof(Content.ContentType).ToCamelCase()!) { Value = value }
                ).ToArray();
            }
            else if (filter.Contains("status = "))
            {
                var value = filter.Replace("status = ", "").Trim('\'');
                // 添加状态过滤条件
                boolQuery.Filter ??= [];
                boolQuery.Filter = boolQuery.Filter.Append(
                    new TermQuery(nameof(Content.Status).ToCamelCase()!) { Value = value }
                ).ToArray();
            }
        }

        return boolQuery;
    }


    /// <summary>
    /// 根据排序字符串构建排序选项
    /// </summary>
    /// <param name="sort">排序字符串</param>
    /// <returns>排序选项列表</returns>
    private List<SortOptions>? BuildSort(string? sort)
    {
        // 如果排序字符串为空，返回null
        if (string.IsNullOrWhiteSpace(sort)) return null;

        // 创建排序选项列表
        var sortOptions = new List<SortOptions>();

        // 根据排序字符串设置不同的排序选项
        if (sort.Contains("createdAt:asc"))
        {
            var op = SortOptions.Field(Field.FromString(nameof(Content.CreatedAt).ToCamelCase())!, new FieldSort { Order = SortOrder.Asc });
            // 按创建时间升序排序
            sortOptions.Add(op);
        }
        else if (sort.Contains("createdAt:desc"))
        {
            var op = SortOptions.Field(Field.FromString(nameof(Content.CreatedAt).ToCamelCase())!, new FieldSort { Order = SortOrder.Desc });
            // 按创建时间降序排序
            sortOptions.Add(op);
        }
        else if (sort.Contains("updatedAt:asc"))
        {
            var op = SortOptions.Field(Field.FromString(nameof(Content.UpdatedAt).ToCamelCase())!, new FieldSort { Order = SortOrder.Asc });
            // 按更新时间升序排序
            sortOptions.Add(op);
        }
        else if (sort.Contains("updatedAt:desc"))
        {
            var op = SortOptions.Field(Field.FromString(nameof(Content.UpdatedAt).ToCamelCase())!, new FieldSort { Order = SortOrder.Desc });
            // 按更新时间降序排序
            sortOptions.Add(op);
        }
        else if (sort.Contains("title:asc"))
        {
            var op = SortOptions.Field(Field.FromString($"{nameof(Content.Title).ToCamelCase()}.keyword")!, new FieldSort { Order = SortOrder.Asc });
            // 按标题升序排序
            sortOptions.Add(op);
        }
        else if (sort.Contains("title:desc"))
        {
            var op = SortOptions.Field(Field.FromString($"{nameof(Content.Title).ToCamelCase()}.keyword")!, new FieldSort { Order = SortOrder.Desc });
            // 按标题降序排序
            sortOptions.Add(op);
        }

        // 如果有排序选项则返回，否则返回null
        return sortOptions.Count > 0 ? sortOptions : null;
    }


}
