using Biwen.QuickApi.Infrastructure;
using Biwen.QuickApi.Infrastructure.Locking.DistributedLock;
using Medallion.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.AsyncState;

namespace Biwen.QuickApi.Test
{
    public class AsyncLocalTest(ITestOutputHelper testOutputHelper)
    {
        /// <summary>
        /// 测试AsyncLocal
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task TestAsyncLocal()
        {
            AsyncLocal<string> asyncLocal = new();

            Parallel.For(1, 5, async (o) =>
            {
                asyncLocal.Value = $"Hello {o} {Environment.CurrentManagedThreadId}";
                testOutputHelper.WriteLine(asyncLocal.Value);
                //子线程
                _ = Task.Run(() =>
                  {
                      testOutputHelper.WriteLine($"child thread: {asyncLocal.Value}");
                      Assert.Equal(asyncLocal.Value, $"Hello {o} {Environment.CurrentManagedThreadId}");
                  });

                await Task.Delay(500);
            });

            await Task.Delay(1000);

            // 由于AsyncLocal是线程本地的，所以这里的值是null
            testOutputHelper.WriteLine(asyncLocal.Value ?? "空!");
            Assert.Null(asyncLocal.Value);
        }

        /// <summary>
        /// 测试AsyncState & AsyncContext
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task TestAsyncState()
        {
            var services = new ServiceCollection();
            services.AddAsyncState();

            var provider = services.BuildServiceProvider();

            var init = provider.GetRequiredService<IAsyncState>();
            var state = provider.GetRequiredService<IAsyncContext<User>>();
            init.Initialize();
            state.Set(new User { ThreadId = Environment.CurrentManagedThreadId, Name = "Hello" });


            var raw = state.Get();
            testOutputHelper.WriteLine($"设置前:" + raw?.ToString());
            testOutputHelper.WriteLine(Environment.NewLine);

            //模拟多线程操作设置state
            Parallel.For(1, 10, (o) =>
               {
                   //子线程
                   _ = Task.Run(() =>
                     {

                         Task.Delay(Random.Shared.Next(10, 500));

                         var user = state.Get();
                         testOutputHelper.WriteLine($"{o}设置前:" + user?.ToString());

                         var userNew = new User { ThreadId = Environment.CurrentManagedThreadId, Name = Random.Shared.NextDouble().ToString() };
                         state.Set(userNew);
                         var getUser = state.Get();
                         testOutputHelper.WriteLine($"{o}设置后 thread: {getUser}");

                         //因为是线程本地的，所以这里肯定是相等的,实际上存储的是线程的上下文
                         Assert.Equal(user?.ThreadId, userNew.ThreadId);
                     });
               });

            await Task.Delay(600);

            var result = state.Get();
            testOutputHelper.WriteLine("最终结果:" + result?.ToString());
            Assert.NotNull(result);


        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TestAsyncContextService(bool inHttpContext)
        {
            var services = new ServiceCollection();
            services.AddAsyncStateHttpContext();
            services.AddSingleton(typeof(AsyncContextService<>));
            var provider = services.BuildServiceProvider();

            var _httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
            if (inHttpContext)
            {
                _httpContextAccessor.HttpContext = new DefaultHttpContext();
            }

            var asyncContextService = provider.GetRequiredService<AsyncContextService<User>>();
            var user = new User { ThreadId = Environment.CurrentManagedThreadId, Name = "Hello" };

            asyncContextService.Set(user);

            var raw = asyncContextService.Get();
            testOutputHelper.WriteLine($"设置前:" + raw?.ToString());
            testOutputHelper.WriteLine(Environment.NewLine);

            Assert.NotNull(raw);
        }


        [Fact]
        public async Task TestDistributedLock()
        {
            var services = new ServiceCollection();

            services.AddDistributedLock();//localhost:6379

            var provider = services.BuildServiceProvider();
            var lockProvider = provider.GetRequiredService<IDistributedLockProvider>();

            var key = "test-1";

            _ = Task.Run(async () =>
            {
                using var distributedLock = await lockProvider.TryAcquireLockAsync(key, TimeSpan.FromSeconds(10));
                // 模拟一个长时间的任务
                await Task.Delay(TimeSpan.FromSeconds(3));
            });

            await Task.Delay(500);

            //当前线程尝试获取锁, 由于上一个线程持有锁, 所以获取锁失败
            using var distributedLock2 = await lockProvider.TryAcquireLockAsync(key, TimeSpan.FromSeconds(1));
            Assert.Null(distributedLock2);

            await Task.Delay(3 * 1000);
            //上一个线程释放锁后, 当前线程获取锁成功
            using var distributedLock3 = await lockProvider.TryAcquireLockAsync(key, TimeSpan.FromSeconds(1));
            Assert.NotNull(distributedLock3);

        }




        class User
        {
            public int ThreadId { get; set; }


            public string? Name { get; set; }


            public override string ToString()
            {
                return $"ThreadId: {ThreadId}, Name: {Name}";
            }

        }


    }
}