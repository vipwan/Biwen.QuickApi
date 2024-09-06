// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:12 OpenApiMetadataAttribute.cs

namespace Biwen.QuickApi.Attributes
{
    using Microsoft.AspNetCore.Builder;

    /// <summary>
    /// 用于标记QuickApi的描述信息,标注<seealso cref="BaseQuickApi.HandlerBuilder(RouteHandlerBuilder)"/>
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

        /// <summary>
        /// OperationId
        /// </summary>
        public string? OperationId { get; set; }

        /// <summary>
        /// 标记为是否已过时.
        /// </summary>
        public bool IsDeprecated { get; set; } = false;


        /// <summary>
        /// Tags
        /// </summary>
        public string[] Tags { get; set; } = [];


    }
}