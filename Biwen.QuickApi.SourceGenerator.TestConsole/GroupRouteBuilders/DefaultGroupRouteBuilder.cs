
using Biwen.QuickApi.Http;

namespace Biwen.QuickApi.SourceGenerator.TestConsole.GroupRouteBuilders
{

    /// <summary>
    /// 默认的GroupRouteBuilder
    /// </summary>
    public class DefaultGroupRouteBuilder : IQuickApiGroupRouteBuilder
    {
        public string Group => string.Empty;

        public int Order => 1;

        public RouteGroupBuilder Builder(RouteGroupBuilder routeBuilder)
        {
            routeBuilder.WithTags("Def");
            return routeBuilder;
        }
    }
}