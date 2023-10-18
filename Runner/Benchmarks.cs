using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text;

namespace Runner
{
    /// <summary>
    /// launchCount:运行次数,iterationCount:应执行多少次目标迭代,invocationCount:每次迭代执行多少次目标方法
    /// 总共调用的次数=launchCount*iterationCount*invocationCount
    /// </summary>
    [MemoryDiagnoser, SimpleJob(RuntimeMoniker.Net70, launchCount: 1, warmupCount: 1, iterationCount: 5, invocationCount: 1000)]
    public class Benchmarks
    {
        /// <summary>
        /// 
        /// </summary>
        static HttpClient ApiClient1 { get; } = new WebApplicationFactory<BenchmarkTestWeb.Program>().CreateClient();
        static HttpClient ApiClient2 { get; } = new WebApplicationFactory<BenchmarkTestWeb.Program>().CreateClient();
        static HttpClient ApiClient3 { get; } = new WebApplicationFactory<BenchmarkTestWeb.Program>().CreateClient();
        static HttpClient ApiClient4 { get; } = new WebApplicationFactory<BenchmarkTestWeb.Program>().CreateClient();

        //success
        static readonly StringContent _payload = new(
            @"{
              ""userName"": ""vipwan"",
              ""password"": ""12345678"",
              ""email"": ""vipwms@ms.co.ltd"",
              ""phone"": ""123456"",
              ""address"": ""ShenZhen China"",
              ""remark"": ""this is a QuickApi Test""
            }",
            Encoding.UTF8,
            "application/json");

        //with validate error
        static readonly StringContent _payload2 = new(
            @"{
              ""userName"": ""vipwan"",
              ""password"": ""12345678"",
              ""email"": ""vipwmsms.co.ltd"",
              ""phone"": ""123456abc"",
              ""address"": ""ShenZhen China"",
              ""remark"": ""this is a QuickApi Test""
            }",
            Encoding.UTF8,
            "application/json");

        //with invoke error
        static readonly StringContent _payload3 = new(
            @"{}",
            Encoding.UTF8,
            "application/json");


        [Benchmark(Baseline = true)]
        public async Task WebApi()
        {
            var msg = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new($"{ApiClient1.BaseAddress}My"),
                Content = _payload
            };
            await ApiClient1.SendAsync(msg);


            var msg2 = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new($"{ApiClient1.BaseAddress}My"),
                Content = _payload2
            };
            await ApiClient1.SendAsync(msg2);

            var msg3 = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new($"{ApiClient1.BaseAddress}My"),
                Content = _payload3
            };
            await ApiClient1.SendAsync(msg3);



        }

        [Benchmark]
        public async Task Minimal()
        {
            var msg = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new($"{ApiClient2.BaseAddress}my-minimal"),
                Content = _payload
            };
            await ApiClient2.SendAsync(msg);

            var msg2 = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new($"{ApiClient2.BaseAddress}my-minimal"),
                Content = _payload2
            };
            await ApiClient2.SendAsync(msg2);

            var msg3 = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new($"{ApiClient2.BaseAddress}my-minimal"),
                Content = _payload3
            };
            await ApiClient2.SendAsync(msg3);



        }
        [Benchmark]
        public async Task QA_Gen()
        {
            var msg = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new($"{ApiClient3.BaseAddress}gen/my-quickapi"),
                Content = _payload
            };

            await ApiClient3.SendAsync(msg);

            var msg2 = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new($"{ApiClient3.BaseAddress}gen/my-quickapi"),
                Content = _payload2
            };

            await ApiClient3.SendAsync(msg2);

            var msg3 = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new($"{ApiClient3.BaseAddress}gen/my-quickapi"),
                Content = _payload3
            };

            await ApiClient3.SendAsync(msg3);


        }
        [Benchmark]
        public async Task QA()
        {
            var msg = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new($"{ApiClient4.BaseAddress}my-quickapi"),
                Content = _payload
            };
            await ApiClient4.SendAsync(msg);

            var msg2 = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new($"{ApiClient4.BaseAddress}my-quickapi"),
                Content = _payload2
            };
            await ApiClient4.SendAsync(msg2);

            var msg3 = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new($"{ApiClient4.BaseAddress}my-quickapi"),
                Content = _payload3
            };
            await ApiClient4.SendAsync(msg3);

        }

    }
}