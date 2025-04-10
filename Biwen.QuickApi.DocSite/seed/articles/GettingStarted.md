# 快速入门

Biwen.QuickApi 是一个基于 ASP.NET Core 的快速 API 开发框架，它提供了一种简单、高效的方式来构建 Web API。

## 安装

使用 NuGet 包管理器安装 Biwen.QuickApi：

```bash
dotnet add package Biwen.QuickApi
```

## 基本使用

1. 在 `Program.cs` 中注册服务：

```csharp
var builder = WebApplication.CreateBuilder(args);

// 添加 Biwen.QuickApi 服务
builder.Services.AddBiwenQuickApis(options =>
{
    // 配置选项
    options.UseQuickApiExceptionResultBuilder = true;
});

var app = builder.Build();

// 使用 Biwen.QuickApi 中间件
app.UseBiwenQuickApis();

app.Run();
```

2. 创建你的第一个 API：

```csharp
[QuickApi("hello")]
public class HelloApi : BaseQuickApi
{
    public string Get()
    {
        return "Hello, Biwen.QuickApi!";
    }
}
```

3. 访问 API：
```
GET /api/hello
```

## 更多示例

查看 [示例项目](https://github.com/vipwan/Biwen.QuickApi/tree/main/Biwen.QuickApi.DemoWeb) 获取更多使用示例。

## 下一步

- [核心概念](CoreConcepts.md)
- [系统配置](BiwenQuickApiOptions.md)
- [API 文档](QuickApi.md) 