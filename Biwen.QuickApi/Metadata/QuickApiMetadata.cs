// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:47:50 QuickApiMetadata.cs

namespace Biwen.QuickApi.Metadata;

/// <summary>
/// Metadata for QuickApi
/// </summary>
public class QuickApiMetadata(Type? quickApiType, QuickApiAttribute? quickApiAttribute = null)
{

    //public QuickApiMetadata(
    //    Type? quickApiType,
    //    IEndpointSummaryMetadata endpointSummaryMetadata,
    //    IEndpointDescriptionMetadata? endpointDescriptionMetadata)
    //{
    //    QuickApiType = quickApiType;
    //    EndpointSummaryMetadata = endpointSummaryMetadata;
    //    EndpointDescriptionMetadata = endpointDescriptionMetadata;
    //}

    public Type? QuickApiType { get; set; } = quickApiType;

    /// <summary>
    /// 源代码生成器的metadata可能该项为null.
    /// </summary>
    public QuickApiAttribute? QuickApiAttribute { get; set; } = quickApiAttribute;

    //public IEndpointSummaryMetadata? EndpointSummaryMetadata { get; set; }

    //public IEndpointDescriptionMetadata? EndpointDescriptionMetadata { get; set; }
}



/// <summary>
/// Metadata for QuickApiEndpoint
/// </summary>
public class QuickApiEndpointMetadata
{

}