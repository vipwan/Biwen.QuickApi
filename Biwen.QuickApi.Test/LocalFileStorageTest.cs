// Licensed to the Biwen.QuickApi.Test under one or more agreements.
// The Biwen.QuickApi.Test licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi.Test Author: vipwa, Github: https://github.com/vipwan
// 
// Modify Date: 2024-09-21 00:58:26 LocalFileStorageTest.cs

using Biwen.QuickApi.Serializer;
using Biwen.QuickApi.Storage;

namespace Biwen.QuickApi.Test;

public class LocalFileStorageTest
{
    private readonly IFileStorage _storage;
    private readonly ITestOutputHelper _output;

    public LocalFileStorageTest(ITestOutputHelper testOutput)
    {
        _output = testOutput;

        var path = "E:\\Test";//请注意这里的物理路径根据实际情况修改
        var service = new ServiceCollection();
        service.AddLogging();
        service.AddSystemTextJsonSerializer();
        service.AddKeyedLocalFileStorage("local", path);
        //local
        _storage = service.BuildServiceProvider().GetRequiredKeyedService<IFileStorage>("local");
    }

    /// <summary>
    /// 测试文件是否存在
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task FileExistTest()
    {
        var random = Random.Shared.Next(1000, 9999);
        //文件不存在
        var exist = await _storage.ExistsAsync($"test-{random}.json");
        Assert.False(exist);
        //创建文件
        await _storage.SaveFileAsync($"test-{random}.json", "hello world");
        exist = await _storage.ExistsAsync($"test-{random}.json");
        Assert.True(exist);
        //最后删除文件
        await _storage.DeleteFileAsync($"test-{random}.json");
    }

    [Fact]
    public async Task FileCopyTest()
    {
        var random = Random.Shared.Next(1000, 9999);
        //创建文件
        await _storage.SaveFileAsync($"test-{random}.json", "hello world");
        var exist = await _storage.ExistsAsync($"test-{random}.json");
        Assert.True(exist);
        //复制文件
        await _storage.CopyFileAsync($"test-{random}.json", $"test-{random}-copy.json");
        exist = await _storage.ExistsAsync($"test-{random}-copy.json");
        Assert.True(exist);
        //最后删除文件
        await _storage.DeleteFilesAsync($"test-{random}.json");
        await _storage.DeleteFilesAsync($"test-{random}-copy.json");
    }


    [Fact]
    public async Task GetFileInfoTest()
    {
        var random = Random.Shared.Next(1000, 9999);
        //创建文件
        await _storage.SaveFileAsync($"test-{random}.json", "hello world");
        var fileInfo = await _storage.GetFileInfoAsync($"test-{random}.json");
        Assert.NotNull(fileInfo);

        _output.WriteLine($"FileName:{fileInfo.Path}");
        _output.WriteLine($"Length:{fileInfo.Size}");
        _output.WriteLine($"Created:{fileInfo.Created}");
        _output.WriteLine($"Modified:{fileInfo.Modified}");

        //最后删除文件
        await _storage.DeleteFilesAsync($"test-{random}.json");
    }

    [Fact]
    public async Task RenameFileTest()
    {
        var random = Random.Shared.Next(1000, 9999);
        //创建文件
        await _storage.SaveFileAsync($"test-{random}.json", "hello world");
        var exist = await _storage.ExistsAsync($"test-{random}.json");
        Assert.True(exist);
        //重命名文件
        await _storage.RenameFileAsync($"test-{random}.json", $"test-{random}-rename.json");
        exist = await _storage.ExistsAsync($"test-{random}-rename.json");
        Assert.True(exist);
        //最后删除文件
        await _storage.DeleteFilesAsync($"test-{random}-rename.json");

    }

    [Fact]
    public async Task GetPagedFileListTest()
    {
        var random = Random.Shared.Next(1000, 9999);
        var random2 = Random.Shared.Next(10000, 10000);
        //创建文件
        await _storage.SaveFileAsync($"test-{random}.json", "hello world");
        await _storage.SaveFileAsync($"test-{random2}.json", "hello world");

        var list = await _storage.GetPagedFileListAsync(1);
        Assert.NotNull(list);
        Assert.True(list.Files.Count > 0);
        //打印第一页文件信息
        foreach (var file in list.Files)
        {
            _output.WriteLine($"FileName:{file.Path}");
            _output.WriteLine($"Length:{file.Size}");
            _output.WriteLine($"Created:{file.Created}");
            _output.WriteLine($"Modified:{file.Modified}");
        }

        _output.WriteLine(Environment.NewLine);

        //还有更多
        Assert.True(list.HasMore);

        var loadNextPage = await list.NextPageAsync();
        Assert.True(loadNextPage);
        //打印第二页文件信息
        foreach (var file in list.Files)
        {
            _output.WriteLine($"FileName:{file.Path}");
            _output.WriteLine($"Length:{file.Size}");
            _output.WriteLine($"Created:{file.Created}");
            _output.WriteLine($"Modified:{file.Modified}");
        }

        //最后删除文件
        await _storage.DeleteFilesAsync($"test-{random}.json");
        await _storage.DeleteFilesAsync($"test-{random2}.json");
    }


    [Fact]
    public Task DiTest()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddKeyedLocalFileStorage("local", "E:\\Test");
        services.AddKeyedLocalFileStorage("local2", "E:\\Test");
        services.AddKeyedLocalFileStorage("local2", "E:\\Test");//不会注入

        var provider = services.BuildServiceProvider();
        var storage = provider.GetRequiredKeyedService<IFileStorage>("local");
        var storage2 = provider.GetRequiredKeyedService<IFileStorage>("local2");
        Assert.NotNull(storage);
        Assert.NotNull(storage2);

        //重名只能注入一个
        var all = services.Where(x => x.IsKeyedService && x.ServiceType == typeof(IFileStorage));
        Assert.Equal(2, all.Count());

        return Task.CompletedTask;

    }

}