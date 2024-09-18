// Licensed to the Biwen.QuickApi under one or more agreements.
// The Biwen.QuickApi licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi Author: 万雅虎 Github: https://github.com/vipwan
// Modify Date: 2024-09-06 16:46:39 Rule.cs

using System.Text.RegularExpressions;

namespace Biwen.QuickApi.Infrastructure.TypeFinder;

internal interface IRule
{
    bool Complies(Type type);
}

internal sealed class CombinedRule : IRule
{
    private readonly IRule? RuleA;
    private readonly IRule RuleB;

    public CombinedRule(IRule? ruleA, IRule ruleB)
    {
        this.RuleA = ruleA;
        this.RuleB = ruleB ?? throw new ArgumentNullException(nameof(ruleB));
    }

    public bool Complies(Type type) => (RuleA?.Complies(type) ?? true) && RuleB.Complies(type);
}

internal sealed class ExcludeTypesRule : IRule
{
    private readonly IList<Type> Excluded;

    public ExcludeTypesRule(IList<Type> excluded)
    {
        this.Excluded = excluded ?? throw new ArgumentNullException(nameof(excluded));
    }

    public bool Complies(Type type) => !Excluded.Contains(type);
}

internal sealed class InheritanceRule : IRule
{
    private readonly Type BaseType;

    public InheritanceRule(Type baseType)
    {
        this.BaseType = baseType ?? throw new ArgumentNullException(nameof(baseType));
    }

    public bool Complies(Type type) => BaseType.IsAssignableFrom(type) && type != BaseType;
}

internal sealed class NameRegexRule : IRule
{
    private readonly Regex Regex;
    private readonly bool MatchFullName;

    public NameRegexRule(string regex, bool matchFullName)
    {
        this.Regex = new Regex(regex);
        this.MatchFullName = matchFullName;
    }

    public bool Complies(Type type) => Regex.IsMatch(MatchFullName ? type.FullName! : type.Name);
}

internal sealed class InNamespaceRule : IRule
{
    private readonly string Namespace;

    public InNamespaceRule(string @namespace)
    {
        this.Namespace = @namespace ?? throw new ArgumentNullException(nameof(@namespace));
    }

    public bool Complies(Type type) => type.Namespace == Namespace;
}

internal sealed class ParameterlessCtorRule : IRule
{
    public bool Complies(Type type) => type.GetConstructor(Type.EmptyTypes) != null;
}

internal sealed class GenericSubclassRule : IRule
{
    private readonly Type GenericType;

    public GenericSubclassRule(Type genericType)
    {
        this.GenericType = genericType ?? throw new ArgumentNullException(nameof(genericType));
    }

    public bool Complies(Type type)
    {
        while (type != null && type != typeof(object))
        {
            var cur = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
            if (GenericType == cur)
                return true;

            type = type.BaseType!;
        }

        return false;
    }
}

internal sealed class HasAttributeRule : IRule
{
    private readonly Type AttributeType;

    public HasAttributeRule(Type attributeType)
    {
        this.AttributeType = attributeType;
    }

    public bool Complies(Type type) => type.IsDefined(AttributeType, true);
}