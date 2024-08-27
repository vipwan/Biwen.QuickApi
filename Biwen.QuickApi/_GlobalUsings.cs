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

global using Biwen.AutoClassGen.Attributes;

global using FindTypes = Biwen.QuickApi.Infrastructure.TypeFinder.FindTypes;
global using ASS = Biwen.QuickApi.Infrastructure.Assemblies;
global using MSDA = System.ComponentModel.DataAnnotations;