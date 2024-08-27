using Biwen.QuickApi.Caching;
using Biwen.QuickApi.Caching.Abstractions;
using Biwen.QuickApi.Caching.Internal;

namespace Biwen.QuickApi.Test
{
    public class CachingProxyTest(ITestOutputHelper testOutput)
    {

        [Fact]
        public async Task SampleTest()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddScoped<ITestClass, TestClass>();
            services.AddMemoryCache();
            services.AddSingleton<IProxyCache, MemoryProxyCache>();
            services.TryAddSingleton(typeof(CachingProxyFactory<>));

            var proxy = services.BuildServiceProvider().GetRequiredService<CachingProxyFactory<ITestClass>>();

            //测试拦截TestClass
            //var testClass = new TestClass();
            var decored = proxy.Create();

            //测试拦截效果:
            decored.TestMethod();
            await decored.TestTask();

            //由于命中了缓存,因此返回都是相同:
            var time1 = decored.TestMethod2(1, "123");
            testOutput.WriteLine(time1.ToString());
            await Task.Delay(1000);
            var time2 = decored.TestMethod2(1, "123");
            testOutput.WriteLine(time2.ToString());
            Assert.Equal(time1, time2);


            //测试缓存过期:
            var time3 = await decored.TestTask2(1);
            testOutput.WriteLine(time3.ToString());
            await Task.Delay(6000);
            var time4 = await decored.TestTask2(1);
            testOutput.WriteLine(time4.ToString());
            Assert.NotEqual(time3, time4);

            await Task.CompletedTask;
        }


        [Fact]
        public async Task Test_Disable_Cache()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddScoped<ITestClass, TestClass>();
            services.AddMemoryCache();
            services.AddSingleton<IProxyCache, MemoryProxyCache>();
            services.TryAddSingleton(typeof(CachingProxyFactory<>));
            var proxy = services.BuildServiceProvider().GetRequiredService<CachingProxyFactory<ITestClass>>();
            //测试拦截TestClass
            var decored = proxy.Create();

            //测试禁用缓存
            var time1 = await decored.TestTask3();
            await Task.Delay(1000);
            var time2 = await decored.TestTask3();
            Assert.NotEqual(time1, time2);
        }
    }


    public interface ITestClass
    {
        [AutoCache]
        void TestMethod();

        [AutoCache]
        Task TestTask();

        DateTime TestMethod2(int random, string random2);

        [AutoCache(5)]
        Task<DateTime> TestTask2(int random);

        [AutoCache]
        Task<DateTime> TestTask3();
    }

    public class TestClass : ITestClass
    {
        public void TestMethod()
        {
            Console.WriteLine("Hello World");
        }

        public Task TestTask()
        {
            return Task.CompletedTask;
        }

        [AutoCache]
        public DateTime TestMethod2(int random, string random2)
        {
            return DateTime.Now;
        }

        public Task<DateTime> TestTask2(int random)
        {
            return Task.FromResult(DateTime.Now);
        }

        /// <summary>
        /// 测试禁用缓存
        /// </summary>
        /// <returns></returns>
        [AutoCache(false)]
        public Task<DateTime> TestTask3()
        {
            return Task.FromResult(DateTime.Now);
        }
    }
}