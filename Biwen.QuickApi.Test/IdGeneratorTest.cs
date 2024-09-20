// Licensed to the Biwen.QuickApi.Test under one or more agreements.
// The Biwen.QuickApi.Test licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi.Test Author: vipwa, Github: https://github.com/vipwan
// 
// Modify Date: 2024-09-21 00:58:20 IdGeneratorTest.cs

namespace Biwen.QuickApi.Test;

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

#if !NET9_0_OR_GREATER

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
    [ClassData(typeof(AutoGuid))]
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

#endif
    class AutoGuid : TheoryData<Guid>
    {
        public AutoGuid()
        {
            Add(Guid.NewGuid());
            Add(Guid.NewGuid());
            Add(Guid.NewGuid());
        }
    }
}