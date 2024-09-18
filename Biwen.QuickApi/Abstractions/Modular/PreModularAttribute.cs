// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:12 PreModularAttribute.cs

namespace Biwen.QuickApi.Abstractions.Modular;

/// <summary>
/// 前置模块
/// </summary>
/// <typeparam name="T"></typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PreModularAttribute<T> : Attribute
    where T : ModularBase
{
}

/// <summary>
/// 前置模块
/// </summary>
/// <typeparam name="T"></typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PreModularAttribute<T, T2> : Attribute
    where T : ModularBase
    where T2 : ModularBase
{
}


/// <summary>
/// 前置模块
/// </summary>
/// <typeparam name="T"></typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PreModularAttribute<T, T2, T3> : Attribute
    where T : ModularBase
    where T2 : ModularBase
    where T3 : ModularBase
{
}

/// <summary>
/// 前置模块
/// </summary>
/// <typeparam name="T"></typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PreModularAttribute<T, T2, T3, T4> : Attribute
    where T : ModularBase
    where T2 : ModularBase
    where T3 : ModularBase
    where T4 : ModularBase
{
}

/// <summary>
/// 前置模块
/// </summary>
/// <typeparam name="T"></typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PreModularAttribute<T, T2, T3, T4, T5> : Attribute
    where T : ModularBase
    where T2 : ModularBase
    where T3 : ModularBase
    where T4 : ModularBase
    where T5 : ModularBase
{
}

/// <summary>
/// 前置模块
/// </summary>
/// <typeparam name="T"></typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PreModularAttribute<T, T2, T3, T4, T5, T6> : Attribute
    where T : ModularBase
    where T2 : ModularBase
    where T3 : ModularBase
    where T4 : ModularBase
    where T5 : ModularBase
    where T6 : ModularBase
{
}

/// <summary>
/// 前置模块
/// </summary>
/// <typeparam name="T"></typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PreModularAttribute<T, T2, T3, T4, T5, T6, T7> : Attribute
    where T : ModularBase
    where T2 : ModularBase
    where T3 : ModularBase
    where T4 : ModularBase
    where T5 : ModularBase
    where T6 : ModularBase
    where T7 : ModularBase
{
}