using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;

namespace Biwen.QuickApi
{
    /// <summary>
    /// Req绑定器接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReqBinder<T> where T : class, new()
    {
        Task<T> Bind(HttpContext context);
    }

    public abstract class BaseRequest<T> : IRequestValidator,IReqBinder<T> where T : class, new()
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

        public virtual async Task<T> Bind(HttpContext context)
        {
            //默认实现，如果需要自定义，请重写

            //route > header > body(Post) = querystring(Get)

            var @default = new T();

            var requestMethod = context.Request.Method!;
            if (requestMethod == HttpMethods.Get)
            {
                //querystring
                {
                    var qs = context.Request.Query;
                    foreach (var item in qs)
                    {
                        var prop = typeof(T).GetProperties().FirstOrDefault(x => x.Name.Equals(item.Key, StringComparison.OrdinalIgnoreCase));
                        if (prop != null)
                        {
                            //转换
                            var value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(item.Value.ToString());
                            prop.SetValue(@default, value);
                        }
                    }
                }
            }

            if (requestMethod == HttpMethods.Post ||
                requestMethod == HttpMethods.Put ||
                requestMethod == HttpMethods.Patch ||
                requestMethod == HttpMethods.Delete)
            {
                //form
                //{
                //    var qs = context.Request.Form;
                //    foreach (var item in qs)
                //    {
                //        var prop = typeof(T).GetProperties().FirstOrDefault(x => x.Name.Equals(item.Key, StringComparison.OrdinalIgnoreCase));
                //        if (prop != null)
                //        {
                //            //转换
                //            var value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(item.Value.ToString());
                //            prop.SetValue(@default, item.Value);
                //        }
                //    }
                //}
                //body
                {
                    @default = await context.Request.ReadFromJsonAsync<T>();
                }
            }

            if (requestMethod == HttpMethods.Head ||
                requestMethod == HttpMethods.Options)
            {
                //header
                {
                    var qs = context.Request.Headers;
                    foreach (var item in qs)
                    {
                        var prop = typeof(T).GetProperties().FirstOrDefault(x => x.Name.Equals(item.Key, StringComparison.OrdinalIgnoreCase));
                        if (prop != null)
                        {
                            //转换
                            var value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(item.Value.ToString());
                            prop.SetValue(@default, item.Value);
                        }
                    }
                }
            }

            //route
            {
                var qs = context.Request.RouteValues;
                foreach (var item in qs)
                {
                    var prop = typeof(T).GetProperties().FirstOrDefault(x => x.Name.Equals(item.Key, StringComparison.OrdinalIgnoreCase));
                    if (prop != null && item.Value != null)
                    {
                        //转换
                        var value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(item.Value!.ToString()!);
                        prop.SetValue(@default, item.Value);
                    }
                }
            }

            //返回
            return @default ?? new();
        }

        #region 内部属性
        /// <summary>
        /// 全局仅有一个T的内部验证器
        /// </summary>
        private readonly InnerValidator Validator = new();

        public object RealValidator => Validator;

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
    interface IRequestValidator
    {
        object RealValidator { get; }
    }

}