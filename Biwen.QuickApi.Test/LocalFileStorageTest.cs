using Biwen.QuickApi.Serializer;
using Biwen.QuickApi.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Biwen.QuickApi.Test
{
    public class LocalFileStorageTest
    {
        private readonly IFileStorage _storage;
        private readonly ITestOutputHelper _output;

        public LocalFileStorageTest(ITestOutputHelper testOutput)
        {
            _output = testOutput;

            var path = "E:\\Test";
            var service = new ServiceCollection();
            service.AddLogging();
            service.AddSingleton<ISerializer, SystemTextJsonSerializer>();
            service.AddKeyedSingleton<IFileStorage>("local", (sp, _) =>
            {
                return new LocalFileStorage(path, sp);
            });
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

    }
}