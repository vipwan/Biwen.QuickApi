using Namotion.Reflection;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace Biwen.QuickApi.Swagger
{

    /// <summary>
    /// QuickApiContractResolver 屏蔽FromServices
    /// </summary>
    public sealed class QuickApiContractResolver : DefaultContractResolver
    {
        /// <summary>
        /// QuickApiContractResolver 屏蔽FromServices & CamelCaseNamingStrategy
        /// </summary>
        public QuickApiContractResolver()
        {
            NamingStrategy = new CamelCaseNamingStrategy();
        }

        protected override JsonContract CreateContract(Type objectType)
        {
            var contract = base.CreateContract(objectType);

            if (objectType.IsClass && objectType != typeof(string))
            {
                if (objectType.GetProperties().Length == 0)
                {
                    contract.IsReference = false;
                }
            }
            return contract;
        }

        [Obsolete]
#pragma warning disable CS0809 // 过时成员重写未过时成员
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
#pragma warning restore CS0809 // 过时成员重写未过时成员
        {
            var attributes = member.GetCustomAttributes(true);

            var property = base.CreateProperty(member, memberSerialization);

            var propertyIgnored = false;


            //忽略不含属性的类型
            if (member is PropertyInfo propertyInfo && propertyInfo.PropertyType.IsClass && propertyInfo.PropertyType != typeof(string))
            {
                if (propertyInfo.PropertyType.GetProperties().Length == 0)
                {
                    propertyIgnored = true;
                }
            }

            //忽略FromServices
#if NET8_0_OR_GREATER

            var fromKeyedServiceAttribute = attributes.FirstAssignableToTypeNameOrDefault($"Biwen.QuickApi.{nameof(FromKeyedServicesAttribute)}", TypeNameStyle.FullName);
            if (fromKeyedServiceAttribute != null)
            {
                propertyIgnored = true;
            }
#endif
            //忽略FromServices
            var fromServiceAttribute = attributes.FirstAssignableToTypeNameOrDefault($"Microsoft.AspNetCore.Mvc.FromServicesAttribute", TypeNameStyle.FullName);
            if (fromServiceAttribute != null)
            {
                propertyIgnored = true;
            }

            var jsonIgnoreAttribute = attributes.FirstAssignableToTypeNameOrDefault("System.Text.Json.Serialization.JsonIgnoreAttribute", TypeNameStyle.FullName);
            if (jsonIgnoreAttribute != null)
            {
                var condition = jsonIgnoreAttribute.TryGetPropertyValue<object>("Condition");
                if (condition is null || condition.ToString() == "Always")
                {
                    propertyIgnored = true;
                }
            }

            property.Ignored = propertyIgnored || attributes.FirstAssignableToTypeNameOrDefault("System.Text.Json.Serialization.JsonExtensionDataAttribute", TypeNameStyle.FullName) != null;

            dynamic jsonPropertyNameAttribute = attributes.FirstAssignableToTypeNameOrDefault("System.Text.Json.Serialization.JsonPropertyNameAttribute", TypeNameStyle.FullName)!;

            if (jsonPropertyNameAttribute != null && !string.IsNullOrEmpty(jsonPropertyNameAttribute?.Name))
            {
                property.PropertyName = jsonPropertyNameAttribute?.Name;
            }

            return property;
        }
    }
}