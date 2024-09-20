// Licensed to the Biwen.QuickApi.Test under one or more agreements.
// The Biwen.QuickApi.Test licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi.Test Author: vipwa, Github: https://github.com/vipwan
// 
// Modify Date: 2024-09-21 00:58:40 MappingTest.cs

using Biwen.QuickApi.Mapping;
using MapsterMapper;

namespace Biwen.QuickApi.Test;

//测试Mapping
public class MappingTest
{
    [Fact]
    public void IMapperTest()
    {
        var services = new ServiceCollection();
        services.AddMapsterMapper();
        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();

        var user = new User
        {
            Name = "Tom",
            Age = 10,
            Address = "China"
        };

        var userDto = mapper.Map<UserDto>(user);
        Assert.NotNull(userDto);
        Assert.Equal(user.Name, userDto.Name);
        Assert.Equal(user.Age, userDto.Age);
    }

    #region model

    //定义User类型:
    public class User
    {
        public string? Name { get; set; }
        public int Age { get; set; }

        public string? Address { get; set; }
    }

    //定义UserDto类型:
    public class UserDto
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }



    #endregion



}