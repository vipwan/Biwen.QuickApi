using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biwen.QuickApi.Http
{
    /// <summary>
    /// Group HanderBuilder
    /// </summary>
    public interface IQuickApiGroupRouteBuilder
    {
        /// <summary>
        /// 分组
        /// </summary>
        string Group { get; }

        RouteGroupBuilder Builder(RouteGroupBuilder routeBuilder);

        /// <summary>
        /// 执行顺序
        /// </summary>
        int Order { get; }
    }

    /// <summary>
    /// DI: services.AddQuickApiGroupRouteBuilder<QuickApiGroupRouteBuilder>();
    /// </summary>
    public abstract class BaseQuickApiGroupRouteBuilder : IQuickApiGroupRouteBuilder
    {
        public abstract string Group { get; }

        public abstract int Order { get; }

        public virtual RouteGroupBuilder Builder(RouteGroupBuilder builder)
        {
            return builder;
        }
    }
}