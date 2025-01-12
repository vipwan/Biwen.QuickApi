﻿using Biwen.QuickApi.DemoWeb.Db;
using Biwen.QuickApi.DemoWeb.Db.Entity;
using Biwen.QuickApi.Application;
using Biwen.QuickApi.UnitOfWork;

namespace Biwen.QuickApi.DemoWeb.Services
{
    /// <summary>
    /// User Service 接口抽象
    /// </summary>
    public interface IUserService : ICurdService<User>
    {
        //可以定义更多的接口方法
    }

    /// <summary>
    /// User Service
    /// </summary>
    [AutoInject] //注入自身
    [AutoInject<IUserService>] //注入接口
    public class UserService : CurdBuinessServiceBase<User, UserDbContext>, IUserService
    {
        public UserService(IUnitOfWork<UserDbContext> uow, ILogger? logger = null) : base(uow, logger)
        {
        }
    }
}