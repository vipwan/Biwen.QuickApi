﻿using Biwen.QuickApi.Application.EntityEvents;
using Biwen.QuickApi.DemoWeb.Db.Entity;
using Biwen.QuickApi.DemoWeb.Services;
using FluentValidation;
using MapsterMapper;
using System.ComponentModel;
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
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }

    [AutoDto<User>]
    public partial class UserDto
    {

    }

    public partial class PageModal : BaseRequest<PageModal>
    {
        [FromQuery, Range(0, int.MaxValue), DefaultValue(0)]
        public int? Page { get; set; } = 0;

        [FromQuery, Range(1, 100), DefaultValue(20)]
        public int? PageSize { get; set; } = 20;

        [FromQuery, StringLength(50)]
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
    [ProducesResponseType<UserDto>(200)]
    public class AddUserEndpoint : BaseQuickApi<CreateUserModal>
    {
        public AddUserEndpoint(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public override async ValueTask<IResult> ExecuteAsync(CreateUserModal request, CancellationToken cancellationToken = default)
        {
            var user = _mapper.Map<User>(request);

            //添加
            await _userService.AddAsync(user);

            //Event
            //这里只是为了演示,实际上DbContext已经发布了事件,因为UserDbContext继承了AutoEventDbContext
            await user.PublishAddedAsync();

            //返回插入的对象
            return Results.Ok(user.MapperToUserDto());
        }
    }

    //获取用户列表
    [QuickApi("/db/user/list", Group = "test")]
    [OpenApiMetadata("获取用户列表", "获取用户列表", Tags = ["用户管理"])]
    [ProducesResponseType<UserDto[]>(200)]
    public class GetUserListEndpoint : BaseQuickApi<PageModal, UserDto[]>
    {
        public GetUserListEndpoint(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public override async ValueTask<UserDto[]> ExecuteAsync(PageModal pageModal, CancellationToken cancellationToken = default)
        {
            var users = await _userService.GetPagedListAsync(
                pageModal.Page ?? 0,
                pageModal.PageSize ?? 20,
                predicate: (u) => pageModal.Name == null || u.Name.Contains(pageModal.Name));

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
        public DeleteUserEndpoint(IUserService userService)
        {
            _userService = userService;
        }
        private readonly IUserService _userService;

        public override async ValueTask<IResult> ExecuteAsync(DeleteUserRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userService.GetAsync([request.Id]);

            if (user == null)
            {
                return Results.BadRequest();
            }

            await _userService.DeleteAsync(user);
            //Event
            //这里只是为了演示,实际上DbContext已经发布了事件,因为UserDbContext继承了AutoEventDbContext
            await user.PublishDeletedAsync();

            return Results.Ok();
        }
    }
}