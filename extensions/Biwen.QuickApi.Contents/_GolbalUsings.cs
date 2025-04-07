// Licensed to the MySurvey.Core under one or more agreements.
// The MySurvey.Core licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

global using Microsoft.Extensions.Logging;

global using System.ComponentModel;
global using System.ComponentModel.DataAnnotations;

global using Biwen.QuickApi;
global using Biwen.QuickApi.Events;
global using Biwen.QuickApi.UnitOfWork;
global using Biwen.AutoClassGen.Attributes;

global using Microsoft.Extensions.DependencyInjection;

#if !NET9_0_OR_GREATER

//兼容NET9的Lock对象
global using Lock = System.Object;
#endif
