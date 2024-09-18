// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:47:10 MapsterModular.cs

namespace Biwen.QuickApi.Mapping.Mapster;

[CoreModular]
internal class MapsterModular : ModularBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddMapsterMapper();
    }
}