namespace Biwen.QuickApi.Attributes
{
    using NSwag.Annotations;
    using Microsoft.AspNetCore.Builder;

    /// <summary>
    /// 用于标记QuickApi的描述信息,等效<see cref="OpenApiOperationAttribute"/>标注<seealso cref="BaseQuickApi.HandlerBuilder(RouteHandlerBuilder)"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class QuickApiSummaryAttribute : Attribute
    {
        public QuickApiSummaryAttribute(string? summary, string description)
        {
            Summary = summary;
            Description = description;
        }

        /// <summary>
        /// Summary
        /// </summary>
        public string? Summary { get; set; }
        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// OperationId
        /// </summary>
        public string? OperationId { get; set; }

        /// <summary>
        /// 标记为是否已过时.
        /// </summary>
        public bool IsDeprecated { get; set; } = false;


    }
}