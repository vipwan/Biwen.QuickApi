namespace Biwen.QuickApi.Abstractions.Modular
{
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

}