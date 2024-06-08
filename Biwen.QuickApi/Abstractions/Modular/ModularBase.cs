using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Biwen.QuickApi.Abstractions.Modular
{
    public abstract class ModularBase : IStartup
    {
        /// <inheritdoc />
        public virtual int Order { get; } = 0;

        public virtual Func<bool> IsEnable => () => true;

        /// <inheritdoc />
        public virtual void ConfigureServices(IServiceCollection services)
        {
        }

        /// <inheritdoc />
        public virtual void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
        }
    }
}