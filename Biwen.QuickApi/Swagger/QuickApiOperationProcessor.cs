namespace Biwen.QuickApi.Swagger
{
    using Biwen.QuickApi.Metadata;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Namotion.Reflection;
    using NJsonSchema;
    using NSwag;
    using NSwag.Annotations;
    using NSwag.Generation.AspNetCore;
    using NSwag.Generation.Processors;
    using NSwag.Generation.Processors.Contexts;
    using System.Collections;
    using System.Dynamic;
    using System.Text.Json.Serialization;
    using System.Text.RegularExpressions;

    /// <summary>
    /// QuickApi Swagger OperationProcessor
    /// </summary>
    internal partial class QuickApiOperationProcessor : IOperationProcessor
    {

        public bool Process(OperationProcessorContext context)
        {

            var metaData = ((AspNetCoreOperationProcessorContext)context).ApiDescription.ActionDescriptor.EndpointMetadata;
            var kuickApiDef = metaData.OfType<QuickApiMetadata>().SingleOrDefault();

            if (kuickApiDef is null)
                return true; //this is not a QuickApi

            var apiDescription = ((AspNetCoreOperationProcessorContext)context).ApiDescription;
            var op = context.OperationDescription.Operation;
            var reqContent = op.RequestBody?.Content;
            var opPath = context.OperationDescription.Path = $"/{StripRouteConstraints(apiDescription.RelativePath!)}";//fix missing path parameters

            //set endpoint summary & description
            //op.Summary = metaData.OfType<IEndpointSummaryMetadata>()?.FirstOrDefault()?.Summary;
            //op.Description = metaData.OfType<IEndpointDescriptionMetadata>()?.FirstOrDefault()?.Description;

            var apiOperationAttribute = kuickApiDef.QuickApiType?.GetMethod(nameof(IHandlerBuilder.HandlerBuilder))?.GetCustomAttribute<OpenApiOperationAttribute>();
            if (apiOperationAttribute != null)
            {
                op.Summary = apiOperationAttribute.Summary;
                op.Description = apiOperationAttribute.Description;
            }

            //fix request content-types not displaying correctly
            if (reqContent?.Count > 0)
            {
                var contentVal = reqContent.FirstOrDefault().Value;
                var list = new List<KeyValuePair<string, OpenApiMediaType>>(op.Consumes.Count);
                for (var i = 0; i < op.Consumes.Count; i++)
                    list.Add(new(op.Consumes[i], contentVal));
                reqContent.Clear();
                foreach (var c in list)
                    reqContent.Add(c);
            }

            var reqDtoType = apiDescription.ParameterDescriptions.FirstOrDefault()?.Type;
            var reqDtoIsList = reqDtoType?.GetInterfaces().Contains(typeof(IEnumerable));
            var reqDtoProps = reqDtoIsList is true ? null : reqDtoType?.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy).ToList();
            var isGETRequest = apiDescription.HttpMethod == HttpMethods.Get;

            //移除Get请求的Body以及Content没有属性的请求
            if (isGETRequest || (reqContent != null && HasNoProperties(reqContent)))
            {
                if (reqDtoIsList is false)
                {
                    op.RequestBody = null;
                    foreach (var body in op.Parameters.Where(x => x.Kind == OpenApiParameterKind.Body).ToArray())
                        op.Parameters.Remove(body);
                }
            }

            //请求的Req要求为FromBody的情况:
            var reqType = kuickApiDef.QuickApiType?.GetInterface($"{nameof(IQuickApi<EmptyRequest, EmptyResponse>)}`2")?.GenericTypeArguments[0];
            if (reqType?.GetCustomAttribute<QuickApi.FromBodyAttribute>() != null)
            {
                op.RequestBody = new OpenApiRequestBody
                {
                    Content =
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = context.SchemaGenerator.Generate(reqType.ToContextualType(),context.SchemaResolver)
                        }
                    }
                };
                return true;
            }

            var reqParams = new List<OpenApiParameter>();
            //移除 [JsonIgnore] 和 没有 public setter 的属性
            if (reqDtoProps != null)
            {
                //prop has no public setter or has ignore attribute
                foreach (var p in reqDtoProps.Where(p => p.IsDefined(typeof(JsonIgnoreAttribute)) || p.GetSetMethod()?.IsPublic is not true).ToArray())
                {
                    reqDtoProps.Remove(p);
                }
                //FromServicesAttribute
                foreach (var p in reqDtoProps.Where(p => p.IsDefined(typeof(FromServicesAttribute)) || p.GetSetMethod()?.IsPublic is not true).ToArray())
                {
                    reqDtoProps.Remove(p);
                }
#if NET8_0_OR_GREATER
                //FromKeyedServicesAttribute
                foreach (var p in reqDtoProps.Where(p => p.IsDefined(typeof(FromKeyedServicesAttribute)) || p.GetSetMethod()?.IsPublic is not true).ToArray())
                {
                    reqDtoProps.Remove(p);
                }
#endif

                //添加参数
                foreach (var prop in reqDtoProps)
                {
                    if (prop.IsDefined(typeof(FromQueryAttribute)))
                    {
                        reqParams.Add(CreateParam(
                            context,
                            prop: prop,
                            paramName: prop.GetCustomAttribute<FromQueryAttribute>()?.Name ?? prop.Name,
                            isRequired: !IsNullable(prop),
                            kind: OpenApiParameterKind.Query));
                        continue;
                    }
                    if (prop.IsDefined(typeof(FromRouteAttribute)))
                    {
                        reqParams.Add(CreateParam(
                            context,
                            prop: prop,
                            paramName: prop.Name,
                            isRequired: !IsNullable(prop),
                            kind: OpenApiParameterKind.Path));
                        continue;
                    }
                    if (prop.IsDefined(typeof(FromHeaderAttribute)))
                    {
                        reqParams.Add(CreateParam(
                            context,
                            prop: prop,
                            paramName: prop.GetCustomAttribute<FromHeaderAttribute>()?.Name ?? prop.Name,
                            isRequired: !IsNullable(prop),
                            kind: OpenApiParameterKind.Header));
                        continue;
                    }
                    if (prop.IsDefined(typeof(FromBodyAttribute)))
                    {
                        reqParams.Add(CreateParam(
                            context, 
                            prop:prop, 
                            paramName: prop.Name,
                            isRequired: !IsNullable(prop),
                            kind: OpenApiParameterKind.Body));
                        continue;
                    }
                    if (prop.IsDefined(typeof(FromFormAttribute)))
                    {
                        reqParams.Add(CreateParam(
                            context,
                            prop: prop,
                            paramName: prop.GetCustomAttribute<FromFormAttribute>()?.Name ?? prop.Name,
                            isRequired: !IsNullable(prop),
                            kind: OpenApiParameterKind.FormData));
                        continue;
                    }
                    if (prop.IsDefined(typeof(FromServicesAttribute)))
                    {
                        continue;
                    }
#if NET8_0_OR_GREATER

                    if (prop.IsDefined(typeof(FromKeyedServicesAttribute)))
                    {
                        continue;
                    }
#endif
                    //上传文件
                    {
                        if (prop.PropertyType == typeof(IFormFile))
                        {
                            //reqParams.Add(CreateParam(
                            //    ctx: context,
                            //    prop: prop,
                            //    paramName: prop.Name,
                            //    isRequired: null,
                            //    kind: OpenApiParameterKind.FormData));
                            continue;
                        }

                        if (prop.PropertyType == typeof(IFormFileCollection))
                        {
                            //reqParams.Add(CreateParam(
                            //    ctx: context,
                            //    prop: prop,
                            //    paramName: null,
                            //    isRequired: null,
                            //    kind: OpenApiParameterKind.FormData));
                            continue;
                        }
                    }

                    //别名
                    if (prop.IsDefined(typeof(AliasAsAttribute)))
                    {
                        reqParams.Add(CreateParam(
                            context,
                            prop: prop,
                            paramName: prop.GetCustomAttribute<AliasAsAttribute>()?.Name ?? prop.Name,
                            isRequired: !IsNullable(prop),
                            kind: OpenApiParameterKind.Query));
                        continue;
                    }
                    //默认,如果不是来自路由则从Query中绑定
                    var isFromRoute =
                        opPath.Contains($"{{{prop.Name}}}", StringComparison.OrdinalIgnoreCase) ||
                        opPath.Contains($"{{{prop.Name}?}}", StringComparison.OrdinalIgnoreCase) ||
                        opPath.Contains($"{{{prop.Name}?:}}", StringComparison.OrdinalIgnoreCase);

                    if (isFromRoute)
                    {
                        reqParams.Add(CreateParam(
                            context,
                            prop: prop,
                            paramName: prop.Name,
                            isRequired: !IsNullable(prop),
                            kind: OpenApiParameterKind.Path));
                        continue;
                    }

                    if (isGETRequest && !isFromRoute)
                    {
                        reqParams.Add(CreateParam(
                            context,
                            prop: prop,
                            paramName: prop.Name,
                            isRequired: !IsNullable(prop),
                            kind: OpenApiParameterKind.Query));
                        continue;
                    }

                    //如果是POST请求,则默认从Body中绑定

                }
            }


            //移除空的schema
            //var slist = context.Document.Components.Schemas.AsEnumerable().
            //    Where(x => x.Value.ActualProperties.Count == 0 && x.Value.IsObject);
            //foreach (var s in slist)
            //{
            //    context.OperationDescription.Operation.Parameters.Where(x => x.Name == s.Key).ToList().ForEach(
            //        x => context.OperationDescription.Operation.Parameters.Remove(x));
                
            //    context.Document.Components.Schemas.Remove(s.Key);
            //}

            foreach (var p in reqParams)
                op.Parameters.Add(p);

            //包含Example的情况:
            var example = metaData.OfType<QuickApiExampleMetadata>().SingleOrDefault();
            if (example?.Example != null)
            {
                foreach (var requestBody in op.Parameters.Where(x => x.Kind == OpenApiParameterKind.Body))
                {
                    var clone = new ExpandoObject();
                    foreach (var prop in example.Example.GetType().GetProperties())
                    {
                        if (prop.IsDefined(typeof(JsonIgnoreAttribute)))
                        {
                            continue;
                        }
                        if (prop.IsDefined(typeof(FromServicesAttribute)))
                        {
                            continue;
                        }
#if NET8_0_OR_GREATER
                        if (prop.IsDefined(typeof(FromKeyedServicesAttribute)))
                        {
                            continue;
                        }
#endif
                        if(prop.IsDefined(typeof(FromQueryAttribute)))
                        {
                            continue;
                        }
                        if(prop.IsDefined(typeof(FromRouteAttribute)))
                        {
                            continue;
                        }
                        if(prop.IsDefined(typeof(FromHeaderAttribute)))
                        {
                            continue;
                        }
                        if(!prop.CanRead || !prop.CanWrite)
                        {
                            continue;
                        }

                        clone.TryAdd(prop.Name, prop.GetValue(example.Example));
                    }
                    requestBody.ActualSchema.Example = clone;
                    break;
                }
            }
            return true;
        }

        #region Helper

        [GeneratedRegex("(?<={)(?:.*?)*(?=})", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
        private static partial Regex RouteParamsRegex();

        [GeneratedRegex("(?<={)([^?:}]+)[^}]*(?=})", RegexOptions.Compiled)]
        private static partial Regex RouteConstraintsRegex();

        /// <summary>
        /// 路由Regex
        /// </summary>
        static readonly Regex routeParamsRegex = RouteParamsRegex();
        static readonly Regex routeConstraintsRegex = RouteConstraintsRegex();

        static bool HasNoProperties(IDictionary<string, OpenApiMediaType> content)
             => !content.Any(c => GetAllProperties(c).Any());

        static IEnumerable<KeyValuePair<string, JsonSchemaProperty>> GetAllProperties(KeyValuePair<string, OpenApiMediaType> mediaType)
        {
            return mediaType
                  .Value.Schema.ActualSchema.ActualProperties
                  .Union(mediaType
                        .Value.Schema.ActualSchema.AllInheritedSchemas
                        .Select(s => s.ActualProperties)
                        .SelectMany(s => s.Select(s => s)));
        }

        static readonly NullabilityInfoContext nullCtx = new();
        static bool IsNullable(PropertyInfo p) => nullCtx.Create(p).WriteState == NullabilityState.Nullable;

        static string StripRouteConstraints(string relativePath)
        {
            var parts = relativePath.Split('/');

            for (var i = 0; i < parts.Length; i++)
                parts[i] = routeConstraintsRegex.Replace(parts[i], "$1");

            return string.Join("/", parts);
        }


        /// <summary>
        /// 创建参数
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="prop"></param>
        /// <param name="paramName"></param>
        /// <param name="isRequired"></param>
        /// <param name="kind"></param>
        /// <param name="descriptions"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        static OpenApiParameter CreateParam(OperationProcessorContext ctx,
                                            PropertyInfo? prop,
                                            string? paramName,
                                            bool? isRequired,
                                            OpenApiParameterKind kind)
        {
            var prm = ctx.DocumentGenerator.CreatePrimitiveParameter(
                paramName,
                paramName,
                (prop?.PropertyType ?? typeof(string)).ToContextualType());
            prm.Kind = kind;
            prm.IsRequired = isRequired ?? !IsNullable(prop!);
            prm.IsNullableRaw = null; //if this is not null, nswag generates an incorrect swagger spec for some unknown reason.
            return prm;
        }

        #endregion
    }
}