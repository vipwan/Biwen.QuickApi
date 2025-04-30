1. 添加ES客户端:

```csharp
//add es
builder.Services.AddSingleton(sp =>
{
    return new ElasticsearchClient(new Uri(builder.Configuration["ElasticSearch"]!));
});
//添加 es 索引服务
builder.Services.AddScoped<IContentSearchService, ElasticsearchService>();
```