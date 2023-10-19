
namespace Biwen.QuickApi.DemoWeb.GroupRouteBuilders
{

    /// <summary>
    /// 默认的GroupRouteBuilder
    /// </summary>
    public class DefaultGroupRouteBuilder : IQuickApiGroupRouteBuilder
    {
        public string Group => "admin";

        public int Order => 1;

        public RouteGroupBuilder Builder(RouteGroupBuilder routeBuilder)
        {
            routeBuilder.WithTags("authorization").WithGroupName("admin");
            return routeBuilder;
        }
    }
}