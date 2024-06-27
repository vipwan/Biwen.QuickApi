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