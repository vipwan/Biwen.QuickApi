// Licensed to the Biwen.QuickApi (net8.0) under one or more agreements.
// The Biwen.QuickApi (net8.0) licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.QuickApi (net8.0) Author: vipwa Github: https://github.com/vipwan
// Modify Date: 2024-09-06 15:10:17 BaseRequest.cs

using System.Linq.Expressions;

namespace Biwen.QuickApi;

/// <summary>
/// BaseRequest<T>,如需自动验证,请ctor中使用RuleFor()添加验证规则
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class BaseRequest<T> : IReqValidator<T> where T : class, new()
{
    /// <summary>
    /// 添加验证规则
    /// </summary>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="expression"></param>
    /// <returns></returns>
    public IRuleBuilderInitial<T, TProperty> RuleFor<TProperty>(Expression<Func<T, TProperty>> expression)
    {
        var validator = _innerValidators.GetOrAdd(typeof(T), type => new InnerValidator());
        return validator.RuleFor(expression);
    }

    #region 内部属性

    /// <summary>
    /// 验证器集合
    /// </summary>
    private static readonly ConcurrentDictionary<Type, InnerValidator> _innerValidators = new();

    /// <summary>
    /// 验证请求对象
    /// </summary>
    /// <returns></returns>
    public ValidationResult Validate()
    {
        var validator = _innerValidators.GetOrAdd(typeof(T), type => new InnerValidator());

        // 对于没有验证规则且没有数据注解的情况，可以直接返回有效结果
        if (!validator.HasRules() && !InnerValidator.HasAnnotation)
        {
            return new ValidationResult();
        }

        var req = (T)MemberwiseClone();
        return validator.Validate(req);
    }

    /// <summary>
    /// 异步验证请求对象
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<ValidationResult> ValidateAsync(CancellationToken cancellationToken = default)
    {
        var validator = _innerValidators.GetOrAdd(typeof(T), type => new InnerValidator());

        // 对于没有验证规则且没有数据注解的情况，可以直接返回有效结果
        if (!validator.HasRules() && !InnerValidator.HasAnnotation)
        {
            return new ValidationResult();
        }

        var req = (T)MemberwiseClone();
        return await validator.ValidateAsync(req, cancellationToken);
    }
    #endregion

    private class InnerValidator : AbstractValidator<T>
    {
        /// <summary>
        /// 缓存T是否有DataAnnotation
        /// </summary>
        static readonly ConcurrentDictionary<Type, bool> _tHasAnnotations = new();

        /// <summary>
        /// 判断类型是否有DataAnnotation
        /// </summary>
        public static bool HasAnnotation => _hasAnnotation.Value;

        /// <summary>
        /// 使用惰性初始化模式减少反射开销
        /// </summary>
        private static readonly Lazy<bool> _hasAnnotation = new(() =>
        {
            return _tHasAnnotations.GetOrAdd(typeof(T), type =>
            {
                return type.GetProperties().Any(
                    prop => prop.GetCustomAttributes(true).Any(x => x is MSDA.ValidationAttribute));
            });
        });

        /// <summary>
        /// 判断是否存在验证规则
        /// </summary>
        public bool HasRules()
        {
            return this.GetType()
                .GetProperty("Rules", BindingFlags.NonPublic | BindingFlags.Instance)?
                .GetValue(this) is IEnumerable<object> rules && rules.Any();
        }

        protected override bool PreValidate(ValidationContext<T> context, ValidationResult result)
        {
            // 用于提升性能,如果没有DataAnnotation,则不再执行DataAnnotation的验证
            if (!HasAnnotation)
            {
                return base.PreValidate(context, result);
            }

            var req = context.InstanceToValidate;
            // ms内建的DataAnnotations验证器
            var mc = new MSDA.ValidationContext(req);

            // 添加验证策略以提高性能
            mc.Items.Add("ValidationStrategy", "StopOnFirstFailure");

            var validationResults = new List<MSDA.ValidationResult>();
            var flag = MSDA.Validator.TryValidateObject(req, mc, validationResults, true);

            if (!flag && validationResults.Count > 0)
            {
                result.Errors.AddRange(validationResults.Select(x =>
                    new ValidationFailure(x.MemberNames.FirstOrDefault(), x.ErrorMessage)));
            }

            return base.PreValidate(context, result);
        }
    }
}

/// <summary>
/// 请求来自表单:JSON序列化
/// </summary>
/// <typeparam name="T"></typeparam>
[FromBody]
public abstract class BaseRequestFromBody<T> : BaseRequest<T> where T : class, new()
{
}

/// <summary>
/// 空请求
/// </summary>
public sealed class EmptyRequest : BaseRequest<EmptyRequest>
{
}
