// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:38 _GlobalUsings.cs

global using FluentValidation;
global using FluentValidation.Results;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Options;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Localization;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using System;
global using System.Collections.Concurrent;
global using System.Collections.Generic;
global using System.ComponentModel;
global using System.Linq;
global using System.Reflection;

global using Biwen.QuickApi;
global using Biwen.QuickApi.Abstractions;
global using Biwen.QuickApi.Abstractions.Modular;
global using Biwen.QuickApi.Attributes;
global using Biwen.QuickApi.Caching;
global using Biwen.QuickApi.Infrastructure.DependencyInjection;
global using Biwen.QuickApi.Metadata;
global using Biwen.QuickApi.Infrastructure;

global using FindTypes = Biwen.QuickApi.Infrastructure.TypeFinder.FindTypes;
global using ASS = Biwen.QuickApi.Infrastructure.Assemblies;
global using MSDA = System.ComponentModel.DataAnnotations;

#if !NET9_0_OR_GREATER

//兼容NET9的Lock对象
global using Lock = System.Object;
#endif
