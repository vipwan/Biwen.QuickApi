using System.Linq.Expressions;

namespace Biwen.QuickApi;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册允许延迟加载的服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AllowLazy(this IServiceCollection services)
    {
        var lastRegistration = services.Last();
        var serviceType = lastRegistration.ServiceType;

        // The constructor for Lazy<T> expects a Func<T> which is hard to create dynamically.
        var lazyServiceType = typeof(Lazy<>).MakeGenericType(serviceType);

        // Create a typed MethodInfo for `serviceProvider.GetRequiredService<T>`,
        // where T has been resolved to the required ServiceType
        var getRequiredServiceMethod = typeof(ServiceProviderServiceExtensions).GetMethod(
            nameof(ServiceProviderServiceExtensions.GetRequiredService),
            1,
            [typeof(IServiceProvider)]
        );

        var getRequiredServiceMethodTyped = getRequiredServiceMethod?.MakeGenericMethod(serviceType);

        // Now create a lambda expression equivalent to:
        //
        //     serviceProvider => serviceProvider.GetRequiredService<T>();
        //
        var parameterExpr = Expression.Parameter(typeof(IServiceProvider), "serviceLocator");
        var lambda = Expression.Lambda(
            Expression.Call(null, getRequiredServiceMethodTyped!, parameterExpr),
            parameterExpr
        );

        var lambdaCompiled = lambda.Compile();

        services.Add(new ServiceDescriptor(lazyServiceType,
            serviceProvider => Activator.CreateInstance(lazyServiceType, lambdaCompiled.DynamicInvoke(serviceProvider))!,
            lastRegistration.Lifetime));

        return services;
    }
}
