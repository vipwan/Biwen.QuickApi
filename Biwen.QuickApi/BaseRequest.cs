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
        public IValidator<T> RealValidator => Validator;

        public ValidationResult Validate()
        {
            var req = (T)MemberwiseClone();

            //ms内建的DataAnnotations验证器
            var context = new MSDA.ValidationContext(req);
            var validationResults = new List<MSDA.ValidationResult>();
            var defaultFlag = MSDA.Validator.TryValidateObject(req, context, validationResults, true);

            //FluentValidation验证器
            var fluentValidationResult = Validator.Validate(req);

            if (!defaultFlag)
            {
                fluentValidationResult.Errors.AddRange(validationResults.Select(x => new FluentValidation.Results.ValidationFailure(x.MemberNames.FirstOrDefault(), x.ErrorMessage)));
            }
            //var method = typeof(InnerValidator).GetMethods().First(x => x.Name == nameof(IValidator.Validate));
            //return (method!.Invoke(Validator, new object[] { this }) as ValidationResult)!;
            return fluentValidationResult;
        }
        #endregion


        private class InnerValidator : AbstractValidator<T>
        {
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