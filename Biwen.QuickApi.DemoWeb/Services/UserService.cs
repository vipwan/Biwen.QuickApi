using Biwen.QuickApi.DemoWeb.Db;
using Biwen.QuickApi.DemoWeb.Db.Entity;
using Biwen.QuickApi.Service;
using Biwen.QuickApi.UnitOfWork;

namespace Biwen.QuickApi.DemoWeb.Services
{
    /// <summary>
    /// User Service
    /// </summary>
    [AutoInject]
    public class UserService : CurdBuinessServiceBase<User, UserDbContext>
    {
        public UserService(IUnitOfWork<UserDbContext> uow, ILogger? logger = null) : base(uow, logger)
        {
        }
    }
}