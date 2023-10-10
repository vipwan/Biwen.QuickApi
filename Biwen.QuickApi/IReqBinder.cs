using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Dynamic;
using System.Xml.Linq;

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
            var type = typeof(T);
            var props = type.GetProperties();
            if (props?.Length == 0) return @default;

            foreach (var prop in props!)
            {
                if (prop.Name == nameof(BaseRequest<T>.RealValidator)) continue;

                var fromQuery = prop.GetCustomAttribute<FromQueryAttribute>();
                if (fromQuery != null)
                {
                    var name = fromQuery.Name ?? prop.Name;
                    var qs = context.Request.Query;
                    if (qs.ContainsKey(name))
                    {
                        //转换
                        var value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(qs[name].ToString());
                        prop.SetValue(@default, value);
                        continue;
                    }
                }
                var fromHeader = prop.GetCustomAttribute<FromHeaderAttribute>();
                if ((fromHeader != null))
                {
                    var name = fromHeader.Name ?? prop.Name;
                    var qs = context.Request.Headers;
                    if (qs.ContainsKey(name))
                    {
                        //转换
                        var value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(qs[name].ToString());
                        prop.SetValue(@default, value);
                        continue;
                    }
                }
                var fromRoute = prop.GetCustomAttribute<FromRouteAttribute>();
                if (fromRoute != null)
                {
                    var name = fromRoute.Name ?? prop.Name;
                    var qs = context.Request.RouteValues;
                    if (qs.ContainsKey(name))
                    {
                        var value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(qs[name]?.ToString()!);
                        prop.SetValue(@default, value);
                        continue;
                    }
                }
                var fromService = prop.GetCustomAttribute<FromServicesAttribute>();
                if (fromService != null)
                {
                    var service = context.RequestServices.GetService(prop.PropertyType);
                    if (service != null)
                    {
                        prop.SetValue(@default, service);
                        continue;
                    }
                }
                var fromForm = prop.GetCustomAttribute<FromFormAttribute>();
                if (fromForm != null)
                {
                    var name = fromForm.Name ?? prop.Name;
                    var qs = context.Request.Form;
                    if (qs.ContainsKey(name))
                    {
                        var value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(qs[name].ToString());
                        prop.SetValue(@default, value);
                        continue;
                    }
                }

                if (fromQuery != null ||
                    fromHeader != null ||
                    fromRoute != null ||
                    fromService != null ||
                    fromForm != null)
                {
                    continue;
                }
                //如果仍然未找到
                {
                    bool isBodySet = false;

                    var requestMethod = context.Request.Method!;
                    if (requestMethod == HttpMethods.Get)
                    {
                        //querystring
                        {
                            var qs = context.Request.Query;
                            foreach (var item in qs)
                            {
                                if (prop != null)
                                {
                                    //转换
                                    var value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(item.Value.ToString());
                                    prop.SetValue(@default, value);
                                    break;
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
                                if (prop != null)
                                {
                                    //转换
                                    var value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(item.Value.ToString());
                                    prop.SetValue(@default, item.Value);
                                    break;
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
                            var jsonObject = await context.Request.ReadFromJsonAsync<ExpandoObject>();
                            var alias = prop.GetCustomAttribute<AliasAsAttribute>();
                            if (alias != null)
                            {
                                var dic = (jsonObject as IDictionary<string, object>)!;
                                if (dic.TryGetValue(alias.Name, out object? value))
                                {
                                    //转换
                                    var value2 = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(value.ToString()!);
                                    prop.SetValue(@default, value2);
                                    isBodySet = true;
                                }
                            }
                            else
                            {
                                var dic = (jsonObject as IDictionary<string, object>)!;
                                if (dic.TryGetValue(prop.Name, out object? value))
                                {
                                    //转换
                                    var value2 = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(value.ToString()!);
                                    prop.SetValue(@default, value2);
                                    isBodySet = true;
                                }
                            }
                        }
                    }
                    if (isBodySet) continue;
                    //route
                    {
                        var qs = context.Request.RouteValues;
                        if (qs.ContainsKey(prop.Name))
                        {
                            var value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(qs[prop.Name]?.ToString()!);
                            prop.SetValue(@default, value);
                            continue;
                        }
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