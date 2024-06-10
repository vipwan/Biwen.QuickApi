namespace Biwen.QuickApi.Test
{
    public class AsyncLocalTest(ITestOutputHelper testOutputHelper)
    {
        static AsyncLocal<string> asyncLocal = new();

        [Fact]
        public async Task TestAsyncLocal()
        {
            Parallel.For(1, 5, async (o) =>
            {
                asyncLocal.Value = $"Hello {o}";
                testOutputHelper.WriteLine(asyncLocal.Value);
                //子线程
                _ = Task.Run(() =>
                  {
                      testOutputHelper.WriteLine($"child thread: {asyncLocal.Value}");
                      Assert.Equal(asyncLocal.Value, $"Hello {o}");
                  });

                await Task.Delay(500);
            });

            await Task.Delay(1000);

            // 由于AsyncLocal是线程本地的，所以这里的值是null
            Assert.Null(asyncLocal.Value);
        }
    }
}