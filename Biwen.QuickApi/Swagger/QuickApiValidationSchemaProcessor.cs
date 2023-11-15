using Biwen.QuickApi.Swagger.ValidationProcessor.Extensions;
using Biwen.QuickApi.Swagger.ValidationProcessor;
using FluentValidation.Internal;
using FluentValidation.Validators;
using NJsonSchema.Generation;
using NJsonSchema;
using System.Collections.ObjectModel;

namespace Biwen.QuickApi.Swagger
{
    internal sealed class QuickApiValidationSchemaProcessor : ISchemaProcessor
    {
        static readonly FluentValidationRule[] _rules = CreateDefaultRules();
        static readonly Type _iReqValidatorType = typeof(IReqValidator<>);

        readonly Dictionary<string, IValidator> _childAdaptorValidators = new();

        public QuickApiValidationSchemaProcessor()
        {
        }

        public void Process(SchemaProcessorContext context)
        {
            var tRequest = context.ContextualType;
            if (tRequest == null)
            {
                return;
            }

            if (tRequest?.Type.IsClass is true && tRequest?.Type.IsPublic is true && tRequest?.Type != typeof(string))
            {
                if (tRequest?.Type.GetProperties().Length == 0)
                {
                    return;
                }
                if (tRequest?.Type.IsAbstract is true)
                {
                    return;
                }
                if (tRequest?.Type.GetInterface(_iReqValidatorType.Name) is not null)
                {
                    try
                    {
                        //dynamic
                        var validator = (dynamic)Activator.CreateInstance(tRequest.Type)!;
                        ApplyValidator(context.Schema, (IValidator)validator.RealValidator, string.Empty);
                    }
                    catch
                    {
                        //todo:
                    }
                }
            }
        }

        void ApplyValidator(JsonSchema schema, IValidator validator, string propertyPrefix)
        {
            // Create dict of rules for this validator
            var rulesDict = validator.GetDictionaryOfRules();
            ApplyRulesToSchema(schema, rulesDict, propertyPrefix);
            ApplyRulesFromIncludedValidators(schema, validator);
        }

        void ApplyRulesToSchema(JsonSchema? schema,
                                        ReadOnlyDictionary<string, List<IValidationRule>> rulesDict,
                                        string propertyPrefix)
        {
            if (schema is null)
                return;

            // Add properties from current schema/class
            if (schema.ActualProperties != null)
            {
                foreach (var schemaProperty in schema.ActualProperties.Keys)
                    TryApplyValidation(schema, rulesDict, schemaProperty, propertyPrefix);
            }

            // Add properties from base class
            ApplyRulesToSchema(schema.InheritedSchema, rulesDict, propertyPrefix);
        }

        void ApplyRulesFromIncludedValidators(JsonSchema schema, IValidator validator)
        {
            if (validator is not IEnumerable<IValidationRule> rules) return;

            // Note: IValidatorDescriptor doesn't return IncludeRules so we need to get validators manually.
            var childAdapters = rules
               .Where(rule => rule.HasNoCondition() && rule is IIncludeRule)
               .SelectMany(includeRule => includeRule.Components.Select(c => c.Validator))
               .Where(x => x.GetType().IsGenericType && x.GetType().GetGenericTypeDefinition() == typeof(ChildValidatorAdaptor<,>))
               .ToList();

            foreach (var adapter in childAdapters)
            {
                var adapterMethod = adapter.GetType().GetMethod("GetValidator");
                if (adapterMethod == null) continue;

                // Create validation context of generic type
                var validationContext = Activator.CreateInstance(
                    adapterMethod.GetParameters().First().ParameterType, new object[] { null! }
                );

                if (adapterMethod.Invoke(adapter, new[] { validationContext, null! }) is not IValidator includeValidator)
                {
                    break;
                }

                ApplyRulesToSchema(schema, includeValidator.GetDictionaryOfRules(), string.Empty);
                ApplyRulesFromIncludedValidators(schema, includeValidator);
            }
        }

        void TryApplyValidation(JsonSchema schema,
                                        ReadOnlyDictionary<string, List<IValidationRule>> rulesDict,
                                        string propertyName,
                                        string parameterPrefix)
        {
            // Build the full propertyname with composition route: request.child.property
            var fullPropertyName = $"{parameterPrefix}{propertyName}";

            // Try get a list of valid rules that matches this property name
            if (rulesDict.TryGetValue(fullPropertyName, out var validationRules))
            {
                // Go through each rule and apply it to the schema
                foreach (var validationRule in validationRules)
                    ApplyValidationRule(schema, validationRule, propertyName);
            }

            // If the property is a child object, recursively apply validation to it adding prefix as we go down one level
            var property = schema.ActualProperties[propertyName];
            var propertySchema = property.ActualSchema;
            if (propertySchema.ActualProperties is not null && propertySchema.ActualProperties.Count > 0 && propertySchema != schema)
                ApplyRulesToSchema(propertySchema, rulesDict, $"{fullPropertyName}.");
        }

        void ApplyValidationRule(JsonSchema schema, IValidationRule validationRule, string propertyName)
        {
            foreach (var ruleComponent in validationRule.Components)
            {
                var propertyValidator = ruleComponent.Validator;

                // 1. If the propertyValidator is a ChildValidatorAdaptor we need to get the underlying validator
                // i.e. for RuleFor().SetValidator() or RuleForEach().SetValidator()
                if (propertyValidator.Name == "ChildValidatorAdaptor")
                {
                    // Get underlying validator using reflection
                    var validatorTypeObj = propertyValidator.GetType()
                        .GetProperty("ValidatorType")
                        ?.GetValue(propertyValidator);
                    // Check if something went wrong
                    if (validatorTypeObj is not Type validatorType)
                        throw new InvalidOperationException("ChildValidatorAdaptor.ValidatorType is null");

                    // Retrieve or create an instance of the validator
                    if (!_childAdaptorValidators.TryGetValue(validatorType.FullName!, out var childValidator))
                        childValidator = _childAdaptorValidators[validatorType.FullName!] = (IValidator)Activator.CreateInstance(validatorType)!;

                    // Apply the validator to the schema. Again, recursively
                    var childSchema = schema.ActualProperties[propertyName].ActualSchema;
                    // Check if it is an array (RuleForEach()). In this case we need to apply validator to an Item Schema
                    childSchema = childSchema.Type == JsonObjectType.Array ? childSchema?.Item?.ActualSchema : childSchema;
                    ApplyValidator(childSchema!, childValidator, string.Empty);

                    continue;
                }

                // 2. Normal property validator processing
                foreach (var rule in _rules)
                {
                    if (!rule.Matches(propertyValidator))
                        continue;

                    try
                    {
                        rule.Apply(new RuleContext(schema, propertyName, propertyValidator));
                    }
                    catch { }
                }
            }
        }

        static FluentValidationRule[] CreateDefaultRules() => new[]
        {
        new FluentValidationRule("Required")
        {
            Matches = propertyValidator => propertyValidator is INotNullValidator or INotEmptyValidator,
            Apply = context =>
            {
                var schema = context.Schema;
                if (!schema.RequiredProperties.Contains(context.PropertyKey))
                    schema.RequiredProperties.Add(context.PropertyKey);
            }
        },
        new FluentValidationRule("NotNull")
        {
            Matches = propertyValidator => propertyValidator is INotNullValidator,
            Apply = context =>
            {
                var schema = context.Schema;
                var properties = schema.ActualProperties;
                properties[context.PropertyKey].IsNullableRaw = false;
                if (properties[context.PropertyKey].Type.HasFlag(JsonObjectType.Null))
                    properties[context.PropertyKey].Type &= ~JsonObjectType.Null; // Remove nullable
                var oneOfsWithReference = properties[context.PropertyKey].OneOf
                    .Where(x => x.Reference != null)
                    .ToList();
                if (oneOfsWithReference.Count == 1)
                {
                    // Set the Reference directly instead and clear the OneOf collection
                    properties[context.PropertyKey].Reference = oneOfsWithReference.Single();
                    properties[context.PropertyKey].OneOf.Clear();
                }
            }
        },
        new FluentValidationRule("NotEmpty")
        {
            Matches = propertyValidator => propertyValidator is INotEmptyValidator,
            Apply = context =>
            {
                var schema = context.Schema;
                var properties = schema.ActualProperties;
                properties[context.PropertyKey].IsNullableRaw = false;
                if (properties[context.PropertyKey].Type.HasFlag(JsonObjectType.Null))
                    properties[context.PropertyKey].Type &= ~JsonObjectType.Null; // Remove nullable
                var oneOfsWithReference = properties[context.PropertyKey].OneOf
                    .Where(x => x.Reference != null)
                    .ToList();
                if (oneOfsWithReference.Count == 1)
                {
                    // Set the Reference directly instead and clear the OneOf collection
                    properties[context.PropertyKey].Reference = oneOfsWithReference.Single();
                    properties[context.PropertyKey].OneOf.Clear();
                }
                properties[context.PropertyKey].MinLength = 1;
            }
        },
        new FluentValidationRule("Length")
        {
            Matches = propertyValidator => propertyValidator is ILengthValidator,
            Apply = context =>
            {
                var schema = context.Schema;
                var properties = schema.ActualProperties;
                var lengthValidator = (ILengthValidator)context.PropertyValidator;
                if (lengthValidator.Max > 0)
                    properties[context.PropertyKey].MaxLength = lengthValidator.Max;
                if (lengthValidator.GetType() == typeof(MinimumLengthValidator<>) ||
                    lengthValidator.GetType() == typeof(ExactLengthValidator<>) ||
                    properties[context.PropertyKey].MinLength == null)
                {
                    properties[context.PropertyKey].MinLength = lengthValidator.Min;
                }
            }
        },
        new FluentValidationRule("Pattern")
        {
            Matches = propertyValidator => propertyValidator is IRegularExpressionValidator,
            Apply = context =>
            {
                var regularExpressionValidator = (IRegularExpressionValidator)context.PropertyValidator;
                var schema = context.Schema;
                var properties = schema.ActualProperties;
                properties[context.PropertyKey].Pattern = regularExpressionValidator.Expression;
            }
        },
        new FluentValidationRule("Comparison")
        {
            Matches = propertyValidator => propertyValidator is IComparisonValidator,
            Apply = context =>
            {
                var comparisonValidator = (IComparisonValidator)context.PropertyValidator;
                if (comparisonValidator.ValueToCompare.IsNumeric())
                {
                    var valueToCompare = Convert.ToDecimal(comparisonValidator.ValueToCompare);
                    var schema = context.Schema;
                    var properties = schema.ActualProperties;
                    var schemaProperty = properties[context.PropertyKey];
                    if (comparisonValidator.Comparison == Comparison.GreaterThanOrEqual)
                    {
                        schemaProperty.Minimum = valueToCompare;
                    }
                    else if (comparisonValidator.Comparison == Comparison.GreaterThan)
                    {
                        schemaProperty.Minimum = valueToCompare;
                        schemaProperty.IsExclusiveMinimum = true;
                    }
                    else if (comparisonValidator.Comparison == Comparison.LessThanOrEqual) { schemaProperty.Maximum = valueToCompare; } else if (comparisonValidator.Comparison == Comparison.LessThan)
                    {
                        schemaProperty.Maximum = valueToCompare;
                        schemaProperty.IsExclusiveMaximum = true;
                    }
                }
            }
        },
        new FluentValidationRule("Between")
        {
            Matches = propertyValidator => propertyValidator is IBetweenValidator,
            Apply = context =>
            {
                var betweenValidator = (IBetweenValidator)context.PropertyValidator;
                var schema = context.Schema;
                var properties = schema.ActualProperties;
                var schemaProperty = properties[context.PropertyKey];
                if (betweenValidator.From.IsNumeric())
                {
                    if (betweenValidator.GetType().IsSubClassOfGeneric(typeof(ExclusiveBetweenValidator<,>)))
                        schemaProperty.ExclusiveMinimum = Convert.ToDecimal(betweenValidator.From);
                    else
                        schemaProperty.Minimum = Convert.ToDecimal(betweenValidator.From);
                }
                if (betweenValidator.To.IsNumeric())
                {
                    if (betweenValidator.GetType().IsSubClassOfGeneric(typeof(ExclusiveBetweenValidator<,>)))
                        schemaProperty.ExclusiveMaximum = Convert.ToDecimal(betweenValidator.To);
                    else
                        schemaProperty.Maximum = Convert.ToDecimal(betweenValidator.To);
                }
            }
        },
        new FluentValidationRule("AspNetCoreCompatibleEmail")
        {
            Matches = propertyValidator => propertyValidator.GetType().IsSubClassOfGeneric(typeof(AspNetCoreCompatibleEmailValidator<>)),
            Apply = context =>
            {
                var schema = context.Schema;
                var properties = schema.ActualProperties;
                properties[context.PropertyKey].Pattern = "^[^@]+@[^@]+$"; // [^@] All chars except @
            }
        },
    };
    }
}