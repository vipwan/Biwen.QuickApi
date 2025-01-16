// Licensed to the Biwen.QuickApi.FeatureManagement under one or more agreements.
// The Biwen.QuickApi.FeatureManagement licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi.FeatureManagement Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2025-01-16 15:52:06 FeatureRouteHandlerBuilderExtensions.cs

using Microsoft.AspNetCore.Builder;
using Microsoft.FeatureManagement.Mvc;

namespace Microsoft.AspNetCore.Http;

public static class FeatureRouteHandlerBuilderExtensions
{
    /// <summary>
    /// Adds a feature gate to the endpoint.
    /// </summary>
    public static TBuilder WithFeature<TBuilder>(this TBuilder builder, string feature) where TBuilder : IEndpointConventionBuilder
    {
        return builder.WithMetadata(new FeatureGateAttribute(feature));
    }
}
