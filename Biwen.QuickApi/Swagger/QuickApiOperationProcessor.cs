using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Biwen.QuickApi.Swagger
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Metadata;
    using Microsoft.AspNetCore.Mvc;
    using Namotion.Reflection;
    using NJsonSchema;
    using NSwag;
    using NSwag.Generation.AspNetCore;
    using NSwag.Generation.Processors;
    using NSwag.Generation.Processors.Contexts;
    using System.Collections;
    using System.Text.Json.Serialization;
    using System.Text.RegularExpressions;

    /// <summary>
    /// QuickApi Swagger OperationProcessor
    /// </summary>
    public class QuickApiOperationProcessor : IOperationProcessor
    {
        /// <summary>
        /// 路由Regex
        /// </summary>
        static readonly Regex routeParamsRegex = new("(?<={)(?:.*?)*(?=})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        static readonly Regex routeConstraintsRegex = new("(?<={)([^?:}]+)[^}]*(?=})", RegexOptions.Compiled);

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
            op.Summary = metaData.OfType<IEndpointSummaryMetadata>()?.FirstOrDefault()?.Summary;
            op.Description = metaData.OfType<IEndpointDescriptionMetadata>()?.FirstOrDefault()?.Description;

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

            var reqParams = new List<OpenApiParameter>();

            //移除 [JsonIgnore] 和 没有 public setter 的属性
            if (reqDtoProps != null)
            {
                foreach (var p in reqDtoProps.Where(p => p.IsDefined(typeof(JsonIgnoreAttribute)) || p.GetSetMethod()?.IsPublic is not true).ToArray()) //prop has no public setter or has ignore attribute
                {
                    reqDtoProps.Remove(p);
                }

                //添加参数
                foreach (var prop in reqDtoProps)
                {
                    if (prop.IsDefined(typeof(FromQueryAttribute)))
                    {
                        reqParams.Add(CreateParam(
                            ctx: context,
                            prop: prop,
                            paramName: prop.GetCustomAttribute<FromQueryAttribute>()?.Name ?? prop.Name,
                            isRequired: !IsNullable(prop),
                            kind: OpenApiParameterKind.Query));
                        continue;
                    }
                    if (prop.IsDefined(typeof(FromRouteAttribute)))
                    {
                        reqParams.Add(CreateParam(
                            ctx: context,
                            prop: prop,
                            paramName: prop.Name,
                            isRequired: IsNullable(prop),
                            kind: OpenApiParameterKind.Path));
                        continue;
                    }
                    if (prop.IsDefined(typeof(FromHeaderAttribute)))
                    {
                        reqParams.Add(CreateParam(
                            ctx: context,
                            prop: prop,
                            paramName: prop.GetCustomAttribute<FromHeaderAttribute>()?.Name ?? prop.Name,
                            isRequired: IsNullable(prop),
                            kind: OpenApiParameterKind.Header));
                        continue;
                    }
                    if (prop.IsDefined(typeof(FromBodyAttribute)))
                    {
                        reqParams.Add(CreateParam(ctx: context, prop:
                            prop, paramName: prop.Name,
                            isRequired: IsNullable(prop),
                            kind: OpenApiParameterKind.Body));
                        continue;
                    }
                    if (prop.IsDefined(typeof(FromFormAttribute)))
                    {
                        reqParams.Add(CreateParam(context,
                            prop: prop,
                            paramName: prop.GetCustomAttribute<FromFormAttribute>()?.Name ?? prop.Name,
                            isRequired: IsNullable(prop),
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
                            reqParams.Add(CreateParam(
                                ctx: context,
                                prop: prop,
                                paramName: prop.Name,
                                isRequired: null,
                                kind: OpenApiParameterKind.FormData));
                            continue;
                        }

                        if (prop.PropertyType == typeof(IFormFileCollection))
                        {
                            reqParams.Add(CreateParam(
                                ctx: context,
                                prop: prop,
                                paramName: null,
                                isRequired: null,
                                kind: OpenApiParameterKind.FormData));
                            continue;
                        }
                    }

                    //别名
                    if (prop.IsDefined(typeof(AliasAsAttribute)))
                    {
                        reqParams.Add(CreateParam(context,
                            prop: prop,
                            paramName: prop.GetCustomAttribute<AliasAsAttribute>()?.Name ?? prop.Name,
                            isRequired: IsNullable(prop),
                            kind: OpenApiParameterKind.Query));
                        continue;
                    }
                    //默认,如果不是来自路由则从Query中绑定
                    var isFromRoute =
                        opPath.Contains($"{{{prop.Name}}}", StringComparison.OrdinalIgnoreCase) ||
                        opPath.Contains($"{{{prop.Name}?}}", StringComparison.OrdinalIgnoreCase) ||
                        opPath.Contains($"{{{prop.Name}?:}}", StringComparison.OrdinalIgnoreCase);

                    //opPath == null ? false : routeParamsRegex.IsMatch(opPath);
                    reqParams.Add(CreateParam(context,
                        prop: prop,
                        paramName: prop.Name,
                        isRequired: IsNullable(prop),
                        kind: isFromRoute ? OpenApiParameterKind.Path : OpenApiParameterKind.Query));
                    continue;

                    //如果重写了自定义绑定.上述配置都无意义


                }
            }

            //移除空的schema
            //foreach (var s in context.Document.Components.Schemas)
            //{
            //    if (s.Value.ActualProperties.Count == 0 && s.Value.IsObject)
            //        context.Document.Components.Schemas.Remove(s.Key);
            //}


            foreach (var p in reqParams)
                op.Parameters.Add(p);

            return true;
        }


        #region Helper

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