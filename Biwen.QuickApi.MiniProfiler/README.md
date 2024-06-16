### 使用方式:

#### Blazor视图中使用:

请注意,模块是否注册需要判断 `IOptions<WrapMiniProfilerOptions>`的`Enabled`属性.

```csharp
@using Microsoft.Extensions.Options
@using StackExchange.Profiling
@inject IOptions<MiniProfilerOptions> Options
@inject IOptions<WrapMiniProfilerOptions> WrapOptions

@if (WrapOptions.Value.Enabled)
{
    <div class="footer">
        @{
            var html = MiniProfiler.Current?.RenderIncludes(HttpContextAccessor.HttpContext!, Options.Value.PopupRenderPosition);
        }
        @((MarkupString)html?.Value!)
    </div>
}
```

#### MVC视图中使用:
```csharp
@using StackExchange.Profiling
@addTagHelper *, MiniProfiler.AspNetCore.Mvc
@inject IOptions<WrapMiniProfilerOptions> WrapOptions

@if(WrapOptions.Value.Enabled)
{
    <mini-profiler />
}

```

#### 配置Profiler

`MiniProfiler:Enabled`表示是否启用MiniProfiler,`MiniProfiler:PopupRenderPosition`表示MiniProfiler的位置.
其他配置项请参考MiniProfiler的配置项.


```json
{
  "$schema": "../quickapi-schema.json",

  "BiwenQuickApi": {
    "QuickApi": {
      "RoutePrefix": "quick",
      "EnableAntiForgeryTokens": true,
      "EnablePubSub": true,
      "EnableScheduling": true,
      "UseQuickApiExceptionResultBuilder": true
    },

    "MiniProfiler": {
      "Enabled": true,
      "PopupRenderPosition": "BottomLeft",
      "PopupShowTrivial": true
    }
  }
}
```


