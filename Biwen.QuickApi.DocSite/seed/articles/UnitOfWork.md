UnitOfWork
=====================

提供对EntityFrameworkCore的UnitOfWork支持,提供开箱即用的体验.

注册服务
---------------------

假定当前有两个`DbContext`,分别是UserDbContext和IdentityDbContext,我们可以通过以下方式注册UnitOfWork服务.

```csharp
public class DbModular : ModularBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        //添加DbContext
        services.AddDbContext<UserDbContext>(options =>
        {
            options.UseInMemoryDatabase("test");
        });

        services.AddDbContext<IdentityDbContext>(options =>
        {
            options.UseInMemoryDatabase("net8");
        });

        //添加仓储
        services.AddUnitOfWork<UserDbContext, IdentityDbContext>();
    }
}
```
使用UnitOfWork
---------------------

用例使用QuickApi的方式,API中注入`IUnitOfWork<TContext>`即可.
参考一下代码,实现对User表的增删改查:

```csharp
using Biwen.QuickApi.DemoWeb.Db;
using Biwen.QuickApi.DemoWeb.Db.Entity;
using Biwen.QuickApi.UnitOfWork;
using FluentValidation;
using MapsterMapper;
using System.ComponentModel.DataAnnotations;

namespace Biwen.QuickApi.DemoWeb.Apis
{
    #region modal & dto

    [AutoDto<User>(nameof(User.CreateTime), nameof(User.Id))]
    [FromBody]
    public partial class CreateUserModal : BaseRequest<CreateUserModal>
    {
        public CreateUserModal()
        {
            RuleFor(x => x.Email).EmailAddress();
        }
    }

    [AutoDto<User>]
    public partial class UserDto
    {
    }

    public partial class PageModal : BaseRequest<PageModal>
    {
        [FromQuery]
        public int? Page { get; set; } = 0;

        [FromQuery]
        public int? PageSize { get; set; } = 20;

        [FromQuery]
        public string? Name { get; set; } = null;
    }

    [FromBody]
    public partial class DeleteUserRequest : BaseRequest<DeleteUserRequest>
    {
        [Required]
        public int Id { get; set; }
    }

    #endregion

    //添加用户
    [QuickApi("/db/user/add", Group = "test", Verbs = Verb.POST)]
    [OpenApiMetadata("添加用户", "添加用户", Tags = ["用户管理"])]
    [ProducesResponseType<User>(200)]
    public class AddUserEndpoint : BaseQuickApi<CreateUserModal>
    {
        public AddUserEndpoint(IUnitOfWork<UserDbContext> uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        private readonly IUnitOfWork<UserDbContext> _uow;
        private readonly IMapper _mapper;

        public override async ValueTask<IResult> ExecuteAsync(CreateUserModal request, CancellationToken cancellationToken = default)
        {
            var user = _mapper.Map<User>(request);
            await _uow.GetRepository<User>().InsertAsync(user);
            await _uow.SaveChangesAsync();
            //返回插入的对象
            return Results.Ok(user);
        }
    }

    //获取用户列表
    [QuickApi("/db/user/list", Group = "test")]
    [OpenApiMetadata("获取用户列表", "获取用户列表", Tags = ["用户管理"])]
    [ProducesResponseType<UserDto[]>(200)]
    public class GetUserListEndpoint : BaseQuickApi<PageModal, UserDto[]>
    {
        public GetUserListEndpoint(IUnitOfWork<UserDbContext> uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        private readonly IUnitOfWork<UserDbContext> _uow;
        private readonly IMapper _mapper;

        public override async ValueTask<UserDto[]> ExecuteAsync(PageModal pageModal, CancellationToken cancellationToken = default)
        {
            var users = await _uow.GetRepository<User>().GetPagedListAsync(
                pageIndex: pageModal.Page ?? 0,
                pageSize: pageModal.PageSize ?? 20,
                predicate: x => string.IsNullOrEmpty(pageModal.Name) || x.Name.Contains(pageModal.Name),
                orderBy: x => x.OrderByDescending(x => x.CreateTime),
                cancellationToken: cancellationToken
            );

            var dtos = _mapper.Map<List<UserDto>>(users.Items);
            return dtos.ToArray();
        }
    }

    //删除用户
    [QuickApi("/db/user/delete", Group = "test", Verbs = Verb.POST)]
    [OpenApiMetadata("删除用户", "删除用户", Tags = ["用户管理"])]
    [ProducesResponseType(200)]
    public class DeleteUserEndpoint : BaseQuickApi<DeleteUserRequest>
    {
        public DeleteUserEndpoint(IUnitOfWork<UserDbContext> uow)
        {
            _uow = uow;
        }
        private readonly IUnitOfWork<UserDbContext> _uow;

        public override async ValueTask<IResult> ExecuteAsync(DeleteUserRequest request, CancellationToken cancellationToken = default)
        {
            _uow.GetRepository<User>().Delete(request.Id);
            var row = await _uow.SaveChangesAsync();
            if (row > 0)
                return Results.Ok();
            //不存在
            return Results.BadRequest();
        }
    }
}

```


实体事件广播
---------------------

当实体增删改的后,我们可以通过实体事件广播来处理一些业务逻辑,例如:清理缓存,发送邮件,工作流程等.<br/>

如下订阅User实体的事件:
```csharp
namespace Biwen.QuickApi.DemoWeb.Apis.Events
{
    using Biwen.QuickApi.DemoWeb.Db.Entity;
    using Biwen.QuickApi.Service.EntityEvents;
    using System.Threading;
    using System.Threading.Tasks;

    public class UserEvent(ILogger<UserEvent> logger) :
        IEventSubscriber<EntityAdded<User>>,
        IEventSubscriber<EntityDeleted<User>>,
        IEventSubscriber<EntityUpdated<User>>
    {
        public Task HandleAsync(EntityAdded<User> @event, CancellationToken ct)
        {
            logger.LogInformation($"User实体添加,{@event.Entity.Id},{@event.Entity.Name}");
            return Task.CompletedTask;
        }

        public Task HandleAsync(EntityDeleted<User> @event, CancellationToken ct)
        {
            logger.LogInformation($"User实体删除,{@event.Entity.Id},{@event.Entity.Name}");
            return Task.CompletedTask;
        }

        public Task HandleAsync(EntityUpdated<User> @event, CancellationToken ct)
        {
            logger.LogInformation($"User实体更新,{@event.Entity.Id},{@event.Entity.Name}");
            return Task.CompletedTask;
        }
    }
}
```

如下是发布事件:
```csharp
using Biwen.QuickApi.Service.EntityEvents;

await user.PublishDeletedAsync();//删除事件
await user.PublishAddedAsync();//添加事件
await user.PublishUpdatedAsync();//更新事件

```

如果你想一劳永逸,那么DbContext可以继承自`AutoEventDbContext<TDbContext>`,这样DbContext中的实体增删改事件会自动发布.<br/>

如果实体类型不需要自动发布事件,可以通过`[AutoEventIgnore]`特性来忽略.

```csharp
[AutoEventIgnore]
public class Book
{
	public int Id { get; set; }
	public string Name { get; set; }
}

public class User
{
	public int Id { get; set; }
	public string Name { get; set; }
	public DateTime CreateTime { get; set; }
}

public class UserDbContext : AutoEventDbContext<UserDbContext>
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Book> Books { get; set; } = null!;//Book实体不会发布事件

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreateTime).HasDefaultValueSql("getdate()");
        });
    }
}
```




