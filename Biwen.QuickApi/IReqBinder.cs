using Microsoft.AspNetCore.Http;
using System.Dynamic;

namespace Biwen.QuickApi
{

    /// <summary>
    /// Req绑定器接口
    /// 请注意ReqBinder不支持构造器注入
    /// 如果需要DI,使用HttpContext.RequestServices获取Service
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReqBinder<T> where T : class, new()
    {
        Task<T> BindAsync(HttpContext context);
    }

    /// <summary>
    /// 默认的内部绑定器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DefaultReqBinder<T> : IReqBinder<T> where T : class, new()
    {
        /// <summary>
        /// 可以重写此方法，实现自定义绑定,
        /// 如果需要表单绑定,以及IFormFile绑定，请务必重写此方法
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual async Task<T> BindAsync(HttpContext context)
        {
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
                        var prop = GetProperty(item.Key);
                        if (prop != null)
                        {
                            //转换
                            var value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(item.Value.ToString());
                            prop.SetValue(@default, value);
                        }
                    }
                }
            }
            if (requestMethod == HttpMethods.Head)
            {
                //header
                {
                    var qs = context.Request.Headers;
                    foreach (var item in qs)
                    {
                        var prop = GetProperty(item.Key);
                        if (prop != null)
                        {
                            //转换
                            var value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(item.Value.ToString());
                            prop.SetValue(@default, item.Value);
                        }
                    }
                }
            }

            if (requestMethod == HttpMethods.Post ||
                requestMethod == HttpMethods.Put ||
                requestMethod == HttpMethods.Options ||
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
                    var type = typeof(T);
                    if (type == typeof(EmptyRequest))
                    {
                        return @default;
                    }
                    if (type.GetProperties().Length == 0)
                    {
                        return @default;
                    }

                    if (type.GetProperties().Any(x => x.GetCustomAttribute<AliasAsAttribute>() != null))
                    {
                        var jsonObject = await context.Request.ReadFromJsonAsync<ExpandoObject>();
                        if (jsonObject == null)
                            return @default;

                        var dic = jsonObject as IDictionary<string, object>;
                        foreach (var item in dic)
                        {
                            var prop = GetProperty(item.Key);
                            if (prop != null)
                            {
                                //转换
                                var value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(item.Value.ToString()!);
                                prop.SetValue(@default, value);
                            }
                        }
                        return @default;
                    }
                    else
                    {
                        @default = await context.Request.ReadFromJsonAsync<T>();
                    }
                }
            }
            //route
            {
                var qs = context.Request.RouteValues;
                foreach (var item in qs)
                {
                    var prop = GetProperty(item.Key);
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
        /// <summary>
        /// 定位属性
        /// </summary>
        private static Func<string?, PropertyInfo?> GetProperty = (string? name) =>
        {
            if (string.IsNullOrEmpty(name))
                return null;

            var prop =
                typeof(T).GetProperties().FirstOrDefault(x => x.GetCustomAttribute<AliasAsAttribute>()?.Name.Equals(name, StringComparison.OrdinalIgnoreCase) ?? false) ??
                typeof(T).GetProperties().FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return prop;
        };

    }


    /// <summary>
    /// 空绑定器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class EmptyReqBinder<T> : IReqBinder<T> where T : class, new()
    {
        public Task<T> BindAsync(HttpContext context)
        {
            return Task.FromResult(new T());
        }
    }
}