using NSwag.Annotations;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

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

        [JsonIgnore]
        [Obsolete("请使用Validate(),以同时兼容DataAnnotation和FluentValidation,请注意该属性未来会被移除!", false)]
        public IValidator<T> RealValidator => Validator;

        public ValidationResult Validate()
        {
            var req = (T)MemberwiseClone();
            return Validator.Validate(req);

            #region 重写PreValidate实现

            ////ms内建的DataAnnotations验证器
            //var context = new MSDA.ValidationContext(req);
            //var validationResults = new List<MSDA.ValidationResult>();
            //var defaultFlag = MSDA.Validator.TryValidateObject(req, context, validationResults, true);

            ////FluentValidation验证器
            //var fluentValidationResult = Validator.Validate(req);

            //if (!defaultFlag)
            //{
            //    fluentValidationResult.Errors.AddRange(validationResults.Select(x => new ValidationFailure(x.MemberNames.FirstOrDefault(), x.ErrorMessage)));
            //}
            ////var method = typeof(InnerValidator).GetMethods().First(x => x.Name == nameof(IValidator.Validate));
            ////return (method!.Invoke(Validator, new object[] { this }) as ValidationResult)!;
            //return fluentValidationResult;

            #endregion
        }
        #endregion

        private class InnerValidator : AbstractValidator<T>
        {

            private static bool? _hasAnnotationAttr = null;
            private static bool HasAnnotationAttr
            {
                get
                {
                    if (_hasAnnotationAttr == null)
                    {
                        _hasAnnotationAttr = typeof(T).GetProperties().Any(
                            prop => prop.GetCustomAttributes(true).Any(x => x is MSDA.ValidationAttribute));
                    }
                    return _hasAnnotationAttr.Value;
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