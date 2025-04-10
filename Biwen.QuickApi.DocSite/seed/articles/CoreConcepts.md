# 核心概念

本文档介绍 Biwen.QuickApi 框架的核心概念和主要组件。

## 主要组件

### 1. QuickApi

`QuickApi` 是框架的核心组件，用于快速创建 API 端点。它提供了以下特性：

- 基于属性的路由配置
- 自动请求参数绑定
- 内置验证支持
- 统一的响应格式

示例：

```csharp
[QuickApi("users")]
public class UserApi : BaseQuickApi
{
    private readonly IUserService _userService;

    public UserApi(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<UserDto> Get(int id)
    {
        return await _userService.GetUserAsync(id);
    }

    [HttpPost]
    public async Task<Result> Create([FromBody] CreateUserDto dto)
    {
        await _userService.CreateUserAsync(dto);
        return Result.Success();
    }
}
```

### 2. QuickEndpoint

`QuickEndpoint` 是更灵活的 API 端点实现，允许你完全控制请求处理流程。

示例：

```csharp
[QuickEndpoint("custom")]
public class CustomEndpoint : BaseQuickEndpoint
{
    public override async Task HandleAsync(HttpContext context)
    {
        // 自定义处理逻辑
        await context.Response.WriteAsJsonAsync(new { message = "Hello" });
    }
}
```

### 3. 中间件

框架提供了多个内置中间件：

- `UseBiwenQuickApis`: 启用 QuickApi 功能
- `UseQuickApiExceptionHandler`: 统一异常处理
- `UseQuickApiValidation`: 请求验证

### 4. 依赖注入

框架支持 ASP.NET Core 的依赖注入系统，可以方便地注入服务：

```csharp
public class UserApi : BaseQuickApi
{
    private readonly IUserService _userService;
    private readonly ILogger<UserApi> _logger;

    public UserApi(IUserService userService, ILogger<UserApi> logger)
    {
        _userService = userService;
        _logger = logger;
    }
}
```

## 最佳实践

1. **使用异步方法**
   - 所有 API 方法都应该使用 `async/await`
   - 避免阻塞操作

2. **依赖注入**
   - 使用构造函数注入
   - 避免服务定位器模式

3. **错误处理**
   - 使用框架的异常处理中间件
   - 返回适当的 HTTP 状态码

4. **验证**
   - 使用数据注解或 FluentValidation
   - 在模型层面进行验证

## 下一步

- [API 文档](QuickApi.md)
- [请求绑定](ReqBinder.md)
- [验证](Validation.md) 