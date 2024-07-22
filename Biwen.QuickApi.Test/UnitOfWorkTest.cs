using Biwen.QuickApi.DemoWeb.Db;
using Biwen.QuickApi.DemoWeb.Db.Entity;
using Biwen.QuickApi.Test.TestBase;
using Biwen.QuickApi.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Biwen.QuickApi.Test
{
    public class UnitOfWorkTest : IClassFixture<QuickApiTestFactory>
    {

        private readonly ITestOutputHelper _output;
        private readonly QuickApiTestFactory _api;

        public UnitOfWorkTest(QuickApiTestFactory api, ITestOutputHelper output)
        {
            _output = output;
            _api = api;
        }

        [Theory]
        [UserAutoData]
        [UserAutoData]
        public async Task AllUnitOfWorkTest(User user)
        {
            using var scope = _api.Services.CreateScope();

            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork<UserDbContext>>();

            //add user
            await uow.GetRepository<User>().InsertAsync(user);
            await uow.SaveChangesAsync();

            _output.WriteLine($"User Id:{user.Id}");

            // inserted user
            Assert.True(user.Id > 0);

            // delete user
            //uow.GetRepository<User>().Delete(user2);
            uow.GetRepository<User>().Delete(user.Id);
            var row = await uow.SaveChangesAsync();

            Assert.Equal(1, row);

            // select user
            var user2 = await uow.GetRepository<User>().GetFirstOrDefaultAsync(x => x.Id == 1);
            Assert.Null(user2);

            //批量添加10个用户
            for (int i = 0; i < 10; i++)
            {
                await uow.GetRepository<User>().InsertAsync(new User
                {
                    Name = $"test-{i}",
                    Address = $"test address-{i}",
                    CreateTime = DateTime.Now
                });
            }
            var row2 = await uow.SaveChangesAsync();
            //验证是否插入10条
            Assert.Equal(10, row2);

            //分页查询:
            var pageList = await uow.GetRepository<User>().GetPagedListAsync(
                orderBy: u => u.OrderByDescending(u => u.Id),
                pageIndex: 0,
                pageSize: 5);

            foreach (var item in pageList.Items)
            {
                _output.WriteLine($"Id:{item.Id},Name:{item.Name}");
            }

            Assert.True(pageList.TotalPages > 0);
        }

    }

    /// <summary>
    /// 生成符合要求的用户数据
    /// </summary>
    class UserAutoDataAttribute : InlineAutoDataAttribute
    {
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            var fixture = new Fixture();

            var user = fixture.Build<User>()
                 //.With(x => x.Id, 0)
                 .Without(x => x.Id) //ID需要排除因为EFCore需要插入时自动生成
                 .With(x => x.Email, $"{Uuid7.NewUuid7()}@example.com") //邮箱地址,需要照规则生成
                 .Create();

            yield return new object[] { user };
        }
    }
}