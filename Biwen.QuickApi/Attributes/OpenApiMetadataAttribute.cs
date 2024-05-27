namespace Biwen.QuickApi.Attributes
{
    using Microsoft.AspNetCore.Builder;

    /// <summary>
    /// 用于标记QuickApi的描述信息,等效<see cref="OpenApiOperationAttribute"/>标注<seealso cref="BaseQuickApi.HandlerBuilder(RouteHandlerBuilder)"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class OpenApiMetadataAttribute(string? summary, string? description) : Attribute
    {
        /// <summary>
        /// Summary
        /// </summary>
        public string? Summary { get; set; } = summary;
        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; set; } = description;

        ///// <summary>
        ///// OperationId
        ///// </summary>
        //public string? OperationId { get; set; }

        /// <summary>
        /// 标记为是否已过时.
        /// </summary>
        public bool IsDeprecated { get; set; } = false;


        /// <summary>
        /// Tags
        /// </summary>
        public string[] Tags = [];


    }
}