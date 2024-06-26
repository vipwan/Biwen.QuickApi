using Biwen.QuickApi.Test.UnitOfWork;
using Biwen.QuickApi.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Biwen.QuickApi.Test
{
    public class UnitOfWorkTest(ITestOutputHelper output)
    {
        [Fact]
        public async Task AllUnitOfWorkTest()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<TestDbContext>(options =>
            {
                options.UseInMemoryDatabase("test");
            });
            services.AddUnitOfWork<TestDbContext>();

            var provider = services.BuildServiceProvider();
            var uow = provider.GetRequiredService<IUnitOfWork<TestDbContext>>();

            //add user
            var user = new User
            {
                Name = "test",
                Address = "test address",
                CreatedAt = DateTime.Now,
                Email = "my@sina.com"
            };
            await uow.GetRepository<User>().InsertAsync(user);
            await uow.SaveChangesAsync();

            // select user
            var user2 = await uow.GetRepository<User>().FindAsync(1);
            Assert.NotNull(user2);

            // delete user
            //uow.GetRepository<User>().Delete(user2);
            uow.GetRepository<User>().Delete(1);
            var row = await uow.SaveChangesAsync();

            Assert.Equal(1, row);

            // select user
            user2 = await uow.GetRepository<User>().GetFirstOrDefaultAsync(x => x.Id == 1);
            Assert.Null(user2);

            //批量添加10个用户
            for (int i = 0; i < 10; i++)
            {
                await uow.GetRepository<User>().InsertAsync(new User
                {
                    Name = $"test-{i}",
                    Address = $"test address-{i}",
                    CreatedAt = DateTime.Now
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
                output.WriteLine($"Id:{item.Id},Name:{item.Name}");
            }

            Assert.True(pageList.TotalPages > 0);
        }
    }
}