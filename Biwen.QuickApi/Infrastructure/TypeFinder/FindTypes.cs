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
