// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:46:34 InAssemblyFinder.cs

using System.Collections;
namespace Biwen.QuickApi.Infrastructure.TypeFinder
{
#pragma warning disable GEN031 // 使用[AutoGen]自动生成
    internal interface IInAssemblyFinder : IEnumerable<Type>
    {
        /// <summary>
        /// Filters types that have a parameterless constructor.
        /// </summary>
        IInAssemblyFinder WithParameterlessConstructor { get; }

        /// <summary>
        /// Excludes some hard-coded types from the result.
        /// </summary>
        /// <param name="types">The types to exclude.</param>
        IInAssemblyFinder Excluding(params Type[] types);

        /// <summary>
        /// Filters types that inherit or implement <paramref name="type"/>. This is checked using <see cref="Type.IsAssignableFrom(Type)"/>, so both class and interface types can be used.
        /// </summary>
        /// <param name="type">The type of the class or interface types must inherit.</param>
        IInAssemblyFinder ThatInherit(Type type);

        /// <summary>
        /// Filters types that inherit or implement <paramref name="type"/>. This is checked using <see cref="Type.IsAssignableFrom(Type)"/>, so both class and interface types can be used.
        /// </summary>
        /// <typeparam name="T">The type of the class or interface types must inherit.</typeparam>
        IInAssemblyFinder ThatInherit<T>();

        /// <summary>
        /// Filters types that inherit the generic type <paramref name="genericType"/>, for example <see cref="List{T}"/>.
        /// </summary>
        /// <param name="genericType">The generic type that types must inherit, for example: <code>typeof(List&lt;&gt;)</code></param>
        IInAssemblyFinder ThatInheritGenericType(Type genericType);

        /// <summary>
        /// Filters types whose <see cref="MemberInfo.Name"/> matches the specified regex pattern.
        /// </summary>
        /// <param name="regex">The regex pattern to match against.</param>
        IInAssemblyFinder WhoseNameMatches(string regex);

        /// <summary>
        /// Filters types whose <see cref="Type.FullName"/> matches the specified regex pattern.
        /// </summary>
        /// <param name="regex">The regex pattern to match against.</param>
        IInAssemblyFinder WhoseFullNameMatches(string regex);

        /// <summary>
        /// Filters types whose <see cref="Type.Namespace"/> exactly matches <paramref name="ns"/>.
        /// </summary>
        /// <param name="ns">The namespace that types must have.</param>
        IInAssemblyFinder InNamespace(string ns);

        /// <summary>
        /// Filters types that have the <paramref name="attrType"/> attribute.
        /// </summary>
        /// <param name="attrType">The attribute that types must define.</param>
        IInAssemblyFinder WithAttribute(Type attrType);

        /// <summary>
        /// Filters types that have the <typeparamref name="T"/> attribute.
        /// </summary>
        /// <typeparam name="T">The attribute that types must define.</typeparam>
        IInAssemblyFinder WithAttribute<T>() where T : Attribute;
    }
#pragma warning restore GEN031 // 使用[AutoGen]自动生成

    internal class InAssemblyFinder : IInAssemblyFinder
    {
#pragma warning disable CA1859 // 尽可能使用具体类型以提高性能
        private IRule? MainRule;
#pragma warning restore CA1859 // 尽可能使用具体类型以提高性能
        private bool IsFrozen;

        private readonly Assembly[] Assemblies;

        public InAssemblyFinder(Assembly[] assemblies)
        {
            this.Assemblies = assemblies ?? throw new ArgumentNullException(nameof(assemblies));
        }

        private void CheckFrozen()
        {
            if (IsFrozen)
                throw new InvalidOperationException("Cannot modify after enumerating");
        }

        public IInAssemblyFinder WithRule(IRule rule)
        {
            CheckFrozen();
            MainRule = new CombinedRule(MainRule, rule);

            return this;
        }

        public IInAssemblyFinder Excluding(params Type[] types) => WithRule(new ExcludeTypesRule(types));

        public IInAssemblyFinder ThatInherit(Type type) => WithRule(new InheritanceRule(type));

        public IInAssemblyFinder ThatInherit<T>() => ThatInherit(typeof(T));

        public IInAssemblyFinder ThatInheritGenericType(Type genericType) => WithRule(new GenericSubclassRule(genericType));

        public IInAssemblyFinder WhoseNameMatches(string regex) => WithRule(new NameRegexRule(regex, false));

        public IInAssemblyFinder WhoseFullNameMatches(string regex) => WithRule(new NameRegexRule(regex, true));

        public IInAssemblyFinder InNamespace(string ns) => WithRule(new InNamespaceRule(ns));

        public IInAssemblyFinder WithParameterlessConstructor => WithRule(new ParameterlessCtorRule());

        public IInAssemblyFinder WithAttribute(Type attrType) => WithRule(new HasAttributeRule(attrType));

        public IInAssemblyFinder WithAttribute<T>() where T : Attribute => WithAttribute(typeof(T));

        IEnumerator<Type> IEnumerable<Type>.GetEnumerator()
        {
            IsFrozen = true;

            foreach (var ass in Assemblies)
            {
                foreach (var type in ass.GetTypes().Where(x => x.GetCustomAttribute<SuppressTypeAttribute>() is null))
                {
                    if (MainRule?.Complies(type) ?? true)
                    {
                        yield return type;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<Type>)this).GetEnumerator();
    }
}