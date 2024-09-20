// Licensed to the Biwen.QuickApi.Test under one or more agreements.
// The Biwen.QuickApi.Test licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi.Test Author: vipwa, Github: https://github.com/vipwan
// 
// Modify Date: 2024-09-21 00:58:01 DecorateTest.cs

using Biwen.AutoClassGen.Attributes;
using Microsoft.Extensions.Logging;

namespace Biwen.QuickApi.Test;


public class DecorateTest(ITestOutputHelper testOutput)
{
    [Fact]
    public async Task TestDecorate()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        services.Decorate<ITestService, DecorateTestService>();
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<ITestService>();
        var result = await service.SayHello();

        testOutput.WriteLine(result);

        Assert.Equal("Decorated: hello world", result);
    }

    [Fact]
    public async Task TestDecorate_with_di_params()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();

        //通过注入的方式:
        services.Decorate<ITestService, DecorateTestService2>();
        services.AddLogging(builder => builder.AddConsole());


        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<ITestService>();
        var result = await service.SayHello();
        testOutput.WriteLine(result);
        Assert.Equal("Decorated2: hello world", result);
    }


    [Fact]
    public async Task TestDecorate_with_input_params()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();

        //通过传参的方式:
        ILogger<DecorateTestService> logger = new LoggerFactory().CreateLogger<DecorateTestService>();

        services.Decorate<ITestService, DecorateTestService2>(logger);

        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<ITestService>();
        var result = await service.SayHello();
        testOutput.WriteLine(result);
        Assert.Equal("Decorated2: hello world", result);
    }

    [Fact]
    public async Task TestDecorate_With_autogen()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        //使用源生成器模式
        services.AddAutoDecor();

        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<ITestService>();
        var result = await service.SayHello();
        testOutput.WriteLine(result);
        Assert.Equal("Decorated3: hello world", result);

    }

    [AutoDecor<DecorateTestService3>]
    public interface ITestService
    {
        Task<string?> SayHello();
    }

    public class TestService : ITestService
    {
        public async Task<string?> SayHello()
        {
            await Task.CompletedTask;
            return "hello world";
        }
    }

    /// <summary>
    /// 不含其他注入的情况
    /// </summary>
    /// <param name="inner"></param>
    public class DecorateTestService(ITestService inner) : ITestService
    {
        public async Task<string?> SayHello()
        {
            var old = await inner.SayHello();
            return $"Decorated: {old}";
        }
    }

    /// <summary>
    /// 其他参数注入的情况
    /// </summary>
    /// <param name="inner"></param>
    /// <param name="logger"></param>
    public class DecorateTestService2(ITestService inner, ILogger<DecorateTestService> logger) : ITestService
    {
        public async Task<string?> SayHello()
        {
            var old = await inner.SayHello();
            logger.LogInformation(old);
            return $"Decorated2: {old}";
        }
    }

    public class DecorateTestService3(ITestService inner) : ITestService
    {
        public async Task<string?> SayHello()
        {
            var old = await inner.SayHello();
            return $"Decorated3: {old}";
        }
    }

}