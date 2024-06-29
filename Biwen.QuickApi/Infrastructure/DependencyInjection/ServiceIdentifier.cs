namespace Biwen.QuickApi.Infrastructure.DependencyInjection;

public readonly struct ServiceIdentifier : IEquatable<ServiceIdentifier>
{
    public object? ServiceKey { get; }

    public Type ServiceType { get; }

    public ServiceIdentifier(Type serviceType)
    {
        ServiceType = serviceType;
    }

    public ServiceIdentifier(object? serviceKey, Type serviceType)
    {
        ServiceKey = serviceKey;
        ServiceType = serviceType;
    }

    public bool Equals(ServiceIdentifier other)
    {
        if (ServiceKey == null && other.ServiceKey == null)
        {
            return ServiceType == other.ServiceType;
        }
        else if (ServiceKey != null && other.ServiceKey != null)
        {
            return ServiceType == other.ServiceType && ServiceKey.Equals(other.ServiceKey);
        }
        return false;
    }

    public override bool Equals(object? obj)
    {
        return obj is ServiceIdentifier && Equals((ServiceIdentifier)obj);
    }

    public override int GetHashCode()
    {
        if (ServiceKey == null)
        {
            return ServiceType.GetHashCode();
        }
        unchecked
        {
            return (ServiceType.GetHashCode() * 397) ^ ServiceKey.GetHashCode();
        }
    }
}
