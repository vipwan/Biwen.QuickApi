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

        [Theory]
        [InlineAutoData]//一组自动填充,可以部分填充
        [InlineAutoData]
        //[AutoData]//AutoFixture 完全自动填充参数
        public void ParseUuid7_Test(Guid guid)
        {
            testOutputHelper.WriteLine(guid.ToString());

            var id = Uuid7.Parse(guid.ToString());

            //转换成功后的id不应该为空,且大于Empty
            Assert.True(id > Uuid7.Empty);

            testOutputHelper.WriteLine($"{id.ToGuid()}");

            //Uuid4 显然不等于 Uuid7
            Assert.NotEqual(guid, id.ToGuid());
        }


    }
}