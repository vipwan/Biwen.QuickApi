using Biwen.QuickApi.DemoWeb.Db;
using Biwen.QuickApi.DemoWeb.Db.Entity;
using Biwen.QuickApi.Test.TestBase;
using Biwen.QuickApi.UnitOfWork;

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
        [ClassData(typeof(UserAutoData2))]
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


    class UserAutoData2 : TheoryData<User>
    {
        public UserAutoData2()
        {
            var faker = new Faker<User>("zh_CN")
                .Ignore(x => x.Id)
                .RuleFor(x => x.Email, f => f.Internet.Email("vipwan"))
                .RuleFor(x => x.Name, f => f.Name.FullName())
                .RuleFor(x => x.Age, f => f.Random.Number(1, 100))
                .RuleFor(x => x.Address, f => f.Address.FullAddress())
                .RuleFor(x => x.CreateTime, f => f.Date.Past(1));

            for (var i = 0; i < 5; i++)
            {
                Add(faker.Generate());
            }
        }
    }

}