// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:46:29 FindTypes.cs

namespace Biwen.QuickApi.Infrastructure.TypeFinder
{
    [SuppressType]
    internal static class FindTypes
    {
        public static IInAssemblyFinder InAssembly(Assembly assembly) => new InAssemblyFinder(new[] { assembly });

        public static IInAssemblyFinder InAssemblies(params Assembly[] assemblies) => new InAssemblyFinder(assemblies);

        public static IInAssemblyFinder InCurrentAssembly => InAssembly(Assembly.GetCallingAssembly());

        public static IInAssemblyFinder InAllAssemblies => InAssemblies(AppDomain.CurrentDomain.GetAssemblies());
    }
}
