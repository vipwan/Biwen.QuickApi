namespace Biwen.QuickApi.Infrastructure.DependencyInjection;

internal static class ServiceProviderKeyedServiceExtensions
{
    public static object? GetKeyedService(this IServiceProvider provider, Type serviceType, object? serviceKey)
    {

        ArgumentNullException.ThrowIfNull(provider);

        if (provider is IKeyedServiceProvider keyedServiceProvider)
        {
            return keyedServiceProvider.GetKeyedService(serviceType, serviceKey);
        }

        throw new InvalidOperationException("This service provider doesn't support keyed services.");
    }

}
