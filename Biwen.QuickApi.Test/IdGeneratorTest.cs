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


        [Fact]
        public void Uuid7_Test()
        {    
            testOutputHelper.WriteLine(Uuid7.NewUuid4().ToString());

            var id1 = Uuid7.NewUuid7();
            var id2 = Uuid7.NewUuid7();

            testOutputHelper.WriteLine($"{id1} {id2}");

            Assert.True(id1 < id2);

        }

    }
}