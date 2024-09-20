// Licensed to the Biwen.QuickApi.Test under one or more agreements.
// The Biwen.QuickApi.Test licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi.Test Author: vipwa, Github: https://github.com/vipwan
// 
// Modify Date: 2024-09-21 00:58:55 QuickApiTestFactory.cs

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace Biwen.QuickApi.Test.TestBase;

/// <summary>
/// 使用WebApplicationFactory创建测试服务
/// </summary>
public class QuickApiTestFactory : WebApplicationFactory<DemoWeb.Program>, IAsyncLifetime
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(builder =>
        {
            //builder.AddDbContext<TestDbContext>(options =>
            //{
            //    options.UseInMemoryDatabase($"test-{Random.Shared.Next()}");
            //});
            //builder.AddUnitOfWork<TestDbContext>();
            //可以附加测试服务,或者移除服务
            //builder.Remove
        });
    }

    public new Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }
}