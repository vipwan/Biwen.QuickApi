# OpenApi 集成

Biwen.QuickApi 提供了完整的 OpenApi (Swagger) 集成支持，可以自动生成 API 文档。

## 基本配置

### 1. 安装包

```bash
dotnet add package Biwen.QuickApi.OpenApi
```

### 2. 配置服务

在 `Program.cs` 中配置 OpenApi：

```csharp
var builder = WebApplication.CreateBuilder(args);

// 添加 OpenApi 服务
builder.Services.AddOpenApi(options =>
{
    options.Title = "My API";
    options.Version = "v1";
    options.Description = "My API Description";
});

var app = builder.Build();

// 使用 OpenApi 中间件
app.UseOpenApi();

app.Run();
```

## 使用方式

### 1. API 文档注释

使用 XML 注释为 API 添加文档：

```csharp
/// <summary>
/// 用户 API
/// </summary>
[QuickApi("users")]
public class UserApi : BaseQuickApi
{
    /// <summary>
    /// 获取用户信息
    /// </summary>
    /// <param name="id">用户 ID</param>
    /// <returns>用户信息</returns>
    public async Task<UserDto> Get(int id)
    {
        // ...
    }
}
```

### 2. 模型文档

为 DTO 和模型添加文档：

```csharp
/// <summary>
/// 用户信息
/// </summary>
public class UserDto
{
    /// <summary>
    /// 用户 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Name { get; set; }
}
```

## 高级特性

### 1. 分组支持

可以将 API 分组显示：

```csharp
[ApiGroup("User Management")]
[QuickApi("users")]
public class UserApi : BaseQuickApi
{
    // ...
}
```

### 2. 安全配置

配置 API 安全要求：

```csharp
[ApiSecurity(SecuritySchemeType.Bearer)]
[QuickApi("secure")]
public class SecureApi : BaseQuickApi
{
    // ...
}
```

### 3. 响应类型

指定 API 响应类型：

```csharp
[ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
public async Task<UserDto> Get(int id)
{
    // ...
}
```

## 最佳实践

1. **文档完整性**
   - 为所有 API 添加文档注释
   - 描述所有参数和返回值
   - 提供示例值

2. **版本控制**
   - 使用 API 版本控制
   - 保持向后兼容性
   - 清晰标记废弃的 API

3. **安全考虑**
   - 正确配置安全方案
   - 保护敏感信息
   - 使用适当的认证方式

## 示例

### 1. 完整的 API 示例

```csharp
/// <summary>
/// 用户管理 API
/// </summary>
[ApiGroup("User Management")]
[ApiSecurity(SecuritySchemeType.Bearer)]
[QuickApi("users")]
public class UserApi : BaseQuickApi
{
    private readonly IUserService _userService;

    public UserApi(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// 获取用户信息
    /// </summary>
    /// <param name="id">用户 ID</param>
    /// <returns>用户信息</returns>
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<UserDto> Get(int id)
    {
        return await _userService.GetUserAsync(id);
    }

    /// <summary>
    /// 创建用户
    /// </summary>
    /// <param name="dto">用户创建信息</param>
    /// <returns>创建结果</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Result), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<Result> Create([FromBody] CreateUserDto dto)
    {
        await _userService.CreateUserAsync(dto);
        return Result.Success();
    }
}
```

## 下一步

- [API 文档](QuickApi.md)
- [请求绑定](ReqBinder.md)
- [验证](Validation.md) 