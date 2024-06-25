using System.Text;

namespace Biwen.QuickApi.Serializer;

/// <summary>
/// 序列化器
/// </summary>
public interface ISerializer
{
    /// <summary>
    /// 反序列化
    /// </summary>
    /// <param name="data"></param>
    /// <param name="objectType"></param>
    /// <returns></returns>
    object Deserialize(Stream data, Type objectType);
    /// <summary>
    /// 序列化
    /// </summary>
    /// <param name="value"></param>
    /// <param name="output"></param>
    void Serialize(object value, Stream output);
}

#pragma warning disable GEN031 // 使用[AutoGen]自动生成
public interface ITextSerializer : ISerializer { }
#pragma warning restore GEN031 // 使用[AutoGen]自动生成

/// <summary>
/// Default serializer STJ
/// </summary>
public static class DefaultSerializer
{
    /// <summary>
    /// 默认序列化器
    /// </summary>
    public static ISerializer Instance { get; set; } = new SystemTextJsonSerializer();
}

public static class SerializerExtensions
{
    public static T Deserialize<T>(this ISerializer serializer, Stream data)
    {
        return (T)serializer.Deserialize(data, typeof(T));
    }

    public static T Deserialize<T>(this ISerializer serializer, byte[] data)
    {
        return (T)serializer.Deserialize(new MemoryStream(data), typeof(T));
    }

    public static object Deserialize(this ISerializer serializer, byte[] data, Type objectType)
    {
        return serializer.Deserialize(new MemoryStream(data), objectType);
    }

    public static T Deserialize<T>(this ISerializer serializer, string data)
    {
        byte[] bytes;
        if (data == null)
            bytes = Array.Empty<byte>();
        else if (serializer is ITextSerializer)
            bytes = Encoding.UTF8.GetBytes(data);
        else
            bytes = Convert.FromBase64String(data);

        return (T)serializer.Deserialize(new MemoryStream(bytes), typeof(T));
    }

    public static object Deserialize(this ISerializer serializer, string data, Type objectType)
    {
        byte[] bytes;
        if (data == null)
            bytes = Array.Empty<byte>();
        else if (serializer is ITextSerializer)
            bytes = Encoding.UTF8.GetBytes(data);
        else
            bytes = Convert.FromBase64String(data);

        return serializer.Deserialize(new MemoryStream(bytes), objectType);
    }

    public static string? SerializeToString<T>(this ISerializer serializer, T value)
    {
        if (value == null)
            return null;

        var bytes = serializer.SerializeToBytes(value);
        if (serializer is ITextSerializer)
            return Encoding.UTF8.GetString(bytes);

        return Convert.ToBase64String(bytes);
    }

    public static byte[] SerializeToBytes<T>(this ISerializer serializer, T value)
    {
        if (value == null)
            return null!;

        var stream = new MemoryStream();
        serializer.Serialize(value, stream);

        return stream.ToArray();
    }
}
