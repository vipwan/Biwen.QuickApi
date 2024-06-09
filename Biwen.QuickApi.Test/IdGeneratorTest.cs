namespace Biwen.QuickApi.Test
{
    public class IdGeneratorTest
    {
        [Fact]
        public async Task IdGenTest()
        {
            var str = IdGenerator.GenerateId();
            Assert.NotNull(str);
            Assert.True((str?.Length ?? 0) > 0);

            await Task.CompletedTask;
        }
    }
}