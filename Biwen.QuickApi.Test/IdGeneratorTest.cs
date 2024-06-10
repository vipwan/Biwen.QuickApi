namespace Biwen.QuickApi.Test
{
    public class IdGeneratorTest(ITestOutputHelper testOutputHelper)
    {

        [Fact]
        public async Task IdGenTest()
        {
            var str = IdGenerator.GenerateId();

            testOutputHelper.WriteLine($"Id:{str}");

            Assert.NotNull(str);
            Assert.True((str?.Length ?? 0) > 0);
            await Task.CompletedTask;
        }
    }
}