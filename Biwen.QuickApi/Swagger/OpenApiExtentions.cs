using Microsoft.AspNetCore.Builder;

namespace Microsoft.AspNetCore.Http;
public static class OpenApiExtentions
{
    /// <summary>
    /// 添加NSwag的 Example
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="example"></param>
    /// <returns></returns>
    public static RouteHandlerBuilder WithExamples(this RouteHandlerBuilder builder, ICollection<object?> examples)
    {
        builder.WithMetadata(new QuickApiExampleMetadata(examples));
        return builder;
    }
}