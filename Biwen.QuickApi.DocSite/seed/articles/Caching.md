缓存
=====================

系统默认注册了`MemoryCache`,`DistributedMemoryCache`,`OutputCache`,`ResponseCaching` <br/>
你可以直接使用这些缓存组件,如果需要`Redis`缓存等分布式缓存可以自行集成。

MemoryCache用例
--------------------
在Razor Pages使用MemoryCache的用例

```csharp
public class IndexModel : PageModel
{
    private readonly IMemoryCache _memoryCache;

    public IndexModel(IMemoryCache memoryCache) =>
        _memoryCache = memoryCache;

    // ...
}
```

DistributedCache用例
--------------------
在Razor Pages使用DistributedCache的用例

```csharp
public class IndexModel : PageModel
{
    private readonly IDistributedCache _cache;

    public IndexModel(IDistributedCache cache)
    {
        _cache = cache;
    }

    public string? CachedTimeUTC { get; set; }
    public string? ASP_Environment { get; set; }

    public async Task OnGetAsync()
    {
        CachedTimeUTC = "Cached Time Expired";
        var encodedCachedTimeUTC = await _cache.GetAsync("cachedTimeUTC");

        if (encodedCachedTimeUTC != null)
        {
            CachedTimeUTC = Encoding.UTF8.GetString(encodedCachedTimeUTC);
        }

        ASP_Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (String.IsNullOrEmpty(ASP_Environment))
        {
            ASP_Environment = "Null, so Production";
        }
    }

    public async Task<IActionResult> OnPostResetCachedTime()
    {
        var currentTimeUTC = DateTime.UtcNow.ToString();
        byte[] encodedCurrentTimeUTC = Encoding.UTF8.GetBytes(currentTimeUTC);
        var options = new DistributedCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromSeconds(20));
        await _cache.SetAsync("cachedTimeUTC", encodedCurrentTimeUTC, options);

        return RedirectToPage();
    }
}
```

OutputCache用例
--------------------
在Minimal API中使用OutputCache的用例


```csharp
app.MapGet("/cached", Gravatar.WriteGravatar).CacheOutput();
app.MapGet("/attribute", [OutputCache] (context) => Gravatar.WriteGravatar(context));
```

ResponseCaching用例
--------------------

Web API控制器的示例使用ResponseCaching特性来缓存响应。在此示例中，响应缓存的位置设置为None，以便在开发期间禁用缓存。


```csharp
[Route("api/[controller]/ticks")]
[HttpGet]
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
public ContentResult GetTimeTicks() => Content(
                  DateTime.Now.Ticks.ToString());
```

缓存配置
---------------------

你可以通过`AddOutputCache`和`AddResponseCaching`方法自定义缓存配置项,如下所示:


```csharp
//自定义OutputCache配置项
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder
        .With(c => c.HttpContext.Request.Path.StartsWithSegments("/blog"))
        .Tag("tag-blog"));
    options.AddBasePolicy(builder => builder.Tag("tag-all"));
    options.AddPolicy("Query", builder => builder.SetVaryByQuery("culture"));
    options.AddPolicy("NoCache", builder => builder.NoCache());
    options.AddPolicy("NoLock", builder => builder.SetLocking(false));
});

//自定义ResponseCaching配置项

builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 1024;//缓存正文大小小于或等于 1,024 字节的响应
    options.UseCaseSensitivePaths = true;//区分大小写的路径
});

builder.Services.AddControllers(options =>
{
    options.CacheProfiles.Add("Default30",
        new CacheProfile()
        {
            Duration = 30
        });
});
```
```csharp
[ApiController]
[ResponseCache(CacheProfileName = "Default30")]
public class Time2Controller : ControllerBase
{
    [Route("api/[controller]")]
    [HttpGet]
    public ContentResult GetTime() => Content(
                      DateTime.Now.Millisecond.ToString());

    [Route("api/[controller]/ticks")]
    [HttpGet]
    public ContentResult GetTimeTicks() => Content(
                      DateTime.Now.Ticks.ToString());
}
```

> [!NOTE]
> 请注意,实例只是简单的介绍了一些常规的配置,更多帮助可以参考:[MSDN文档](https://learn.microsoft.com/zh-cn/aspnet/core/performance/caching/output?view=aspnetcore-9.0)






API文档
---------------------

相关API文档:
[参考文档](https://learn.microsoft.com/zh-cn/aspnet/core/performance/caching/overview?view=aspnetcore-9.0)