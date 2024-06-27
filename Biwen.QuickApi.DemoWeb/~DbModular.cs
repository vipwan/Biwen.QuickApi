using Biwen.QuickApi.DemoWeb.Db;
using Biwen.QuickApi.UnitOfWork;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Biwen.QuickApi.DemoWeb
{
    /// <summary>
    /// 配置数据库相关的模块
    /// </summary>
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
}