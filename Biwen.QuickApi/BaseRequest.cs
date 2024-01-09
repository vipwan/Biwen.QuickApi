using NSwag.Annotations;
using System.Linq.Expressions;

namespace Biwen.QuickApi
{
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
            return Validator.RuleFor(expression);
        }

        #region 内部属性
        /// <summary>
        /// 全局仅有一个T的内部验证器
        /// </summary>
        private readonly InnerValidator Validator = new();

        public ValidationResult Validate()
        {
            var req = (T)MemberwiseClone();
            return Validator.Validate(req);
        }
        #endregion

        private class InnerValidator : AbstractValidator<T>
        {
            //private static bool? _hasAnnotationAttr = null;

            private static bool HasAnnotationAttr
            {
                get
                {
                    if (Caching.TAnnotationAttrs.TryGetValue(typeof(T), out var attr))
                    {
                        return attr;
                    }
                    var has = typeof(T).GetProperties().Any(
                        prop => prop.GetCustomAttributes(true).Any(x => x is MSDA.ValidationAttribute));

                    Caching.TAnnotationAttrs.TryAdd(typeof(T), has);
                    return has;
                }
            }

            protected override bool PreValidate(ValidationContext<T> context, ValidationResult result)
            {
                //用于提升性能,如果没有DataAnnotation,则不再执行DataAnnotation的验证
                if (!HasAnnotationAttr)
                {
                    return base.PreValidate(context, result);
                }

                var req = context.InstanceToValidate;
                //ms内建的DataAnnotations验证器
                var mc = new MSDA.ValidationContext(req);
                var validationResults = new List<MSDA.ValidationResult>();
                var flag = MSDA.Validator.TryValidateObject(req, mc, validationResults, true);
                if (!flag)
                {
                    result.Errors.AddRange(validationResults.Select(x => new ValidationFailure(x.MemberNames.FirstOrDefault(), x.ErrorMessage)));
                }
                return base.PreValidate(context, result);
            }
        }

    }

    /// <summary>
    /// BaseRequest FromBody:Json
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [FromBody]
    public abstract class BaseRequestFromBody<T> : BaseRequest<T> where T : class, new()
    {
    }

    /// <summary>
    /// 空请求
    /// </summary>
    [OpenApiIgnore]
    public sealed class EmptyRequest : BaseRequest<EmptyRequest>
    {
    }
}