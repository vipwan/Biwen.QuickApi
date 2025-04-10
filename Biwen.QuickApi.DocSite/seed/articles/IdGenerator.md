# ID 生成器

Biwen.QuickApi 提供了高性能的 ID 生成功能，支持多种 ID 生成策略。

## 基本概念

### 1. ID 生成器配置

在 `Program.cs` 中配置 ID 生成器：

```csharp
builder.Services.AddIdGenerator(options =>
{
    options.DefaultStrategy = IdGeneratorStrategy.Snowflake;
    options.WorkerId = 1;
    options.DatacenterId = 1;
});
```

### 2. 使用方式

注入 `IIdGenerator` 服务使用：

```csharp
public class UserService
{
    private readonly IIdGenerator _idGenerator;

    public UserService(IIdGenerator idGenerator)
    {
        _idGenerator = idGenerator;
    }

    public async Task<User> CreateUserAsync(CreateUserDto dto)
    {
        var userId = _idGenerator.NextId();
        // 使用生成的 ID 创建用户
    }
}
```

## 使用方式

### 1. 基本使用

```csharp
public class OrderApi : BaseQuickApi
{
    private readonly IIdGenerator _idGenerator;

    public OrderApi(IIdGenerator idGenerator)
    {
        _idGenerator = idGenerator;
    }

    public async Task<Result> Create([FromBody] CreateOrderDto dto)
    {
        var orderId = _idGenerator.NextId();
        // 使用生成的 ID 创建订单
        return Result.Success();
    }
}
```

### 2. 配置选项

可以在 `appsettings.json` 中配置 ID 生成器选项：

```json
{
  "IdGenerator": {
    "DefaultStrategy": "Snowflake",
    "WorkerId": 1,
    "DatacenterId": 1,
    "SequenceBits": 12,
    "WorkerIdBits": 5,
    "DatacenterIdBits": 5
  }
}
```

## 高级特性

### 1. 多种生成策略

支持多种 ID 生成策略：

```csharp
// 雪花算法
var snowflakeId = _idGenerator.NextId(IdGeneratorStrategy.Snowflake);

// UUID
var uuid = _idGenerator.NextId(IdGeneratorStrategy.Uuid);

// 自增序列
var sequenceId = _idGenerator.NextId(IdGeneratorStrategy.Sequence);
```

### 2. 自定义策略

可以实现自定义的 ID 生成策略：

```csharp
public class CustomIdGenerator : IIdGeneratorStrategy
{
    public long NextId()
    {
        // 实现自定义 ID 生成逻辑
        return GenerateCustomId();
    }
}

// 注册自定义策略
builder.Services.AddSingleton<IIdGeneratorStrategy, CustomIdGenerator>();
```

## 最佳实践

1. **ID 设计**
   - 选择合适的 ID 生成策略
   - 考虑 ID 的可读性和排序性
   - 避免 ID 冲突

2. **性能优化**
   - 使用高效的 ID 生成算法
   - 避免频繁的 ID 生成
   - 监控 ID 生成性能

3. **分布式考虑**
   - 确保分布式环境下的 ID 唯一性
   - 合理配置 WorkerId 和 DatacenterId
   - 处理时钟回拨问题

## 示例

### 1. 订单系统示例

```csharp
public class OrderService
{
    private readonly IIdGenerator _idGenerator;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IIdGenerator idGenerator, ILogger<OrderService> logger)
    {
        _idGenerator = idGenerator;
        _logger = logger;
    }

    public async Task<Order> CreateOrderAsync(CreateOrderDto dto)
    {
        try
        {
            _logger.LogInformation("开始创建订单");
            var orderId = _idGenerator.NextId();
            var order = new Order
            {
                Id = orderId,
                // 其他订单属性
            };
            await _orderRepository.AddAsync(order);
            _logger.LogInformation("订单创建成功，ID: {OrderId}", orderId);
            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建订单失败");
            throw;
        }
    }
}
```

### 2. 分布式系统示例

```csharp
public class DistributedService
{
    private readonly IIdGenerator _idGenerator;

    public DistributedService(IIdGenerator idGenerator)
    {
        _idGenerator = idGenerator;
    }

    public async Task<Result> CreateResourceAsync()
    {
        // 使用雪花算法生成分布式 ID
        var resourceId = _idGenerator.NextId(IdGeneratorStrategy.Snowflake);
        // 创建资源
        return Result.Success();
    }
}
```

## 下一步

- [分布式锁](DistributedLock.md)
- [UnitOfWork](UnitOfWork.md)
- [审计](Auditing.md) 