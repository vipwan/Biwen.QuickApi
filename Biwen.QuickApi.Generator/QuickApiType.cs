// <copyright file="QuickApiType.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Biwen.QuickApi.SourceGenerator
{
    using System.Collections.Generic;

    internal class QuickApiType
    {
        public QuickApiType()
        {
        }

        public const string TypeName = "QuickApi";

        /// <summary>
        /// AutoAnnotation.
        /// </summary>
        public const string AutoAnnotationName = "AutoAnnotation";

        public const string JustAsServiceTypeName = "JustAsService";

        public string Route { get; set; } = string.Empty;

        public string Group { get; set; } = string.Empty;

        public string Policy { get; set; } = string.Empty;

        public List<string> Verbs { get; set; } = new();
    }
}