using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace Biwen.QuickApi
{

    public abstract class BaseRequest<T> : IRequestValidator<T> where T : class, new()
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

        #endregion

        private class InnerValidator : AbstractValidator<T>
        {
        }
    }

    /// <summary>
    /// 空请求
    /// </summary>
    public class EmptyRequest : BaseRequest<EmptyRequest>
    {
    }

    /// <summary>
    /// 验证器接口
    /// </summary>
    interface IRequestValidator<T> where T : class, new()
    {
        IValidator<T> RealValidator { get; }
    }

}