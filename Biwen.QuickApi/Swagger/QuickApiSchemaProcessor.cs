namespace Biwen.QuickApi.Swagger
{
    using Microsoft.AspNetCore.Mvc;
    using NJsonSchema.Generation;

    internal class QuickApiSchemaProcessor : ISchemaProcessor
    {
        public void Process(SchemaProcessorContext context)
        {
            //var type = context?.ContextualType.TypeInfo.ReflectedType;
            //if (type is not null)
            //{
            //    //遍历属性
            //    var props = type.GetProperties();
            //    foreach (var prop in props)
            //    {
            //        var isFromService = prop.GetCustomAttributes().Any(x =>
            //        x.GetType() == typeof(FromServicesAttribute) ||
            //        x.GetType() == typeof(FromKeyedServicesAttribute));

            //        if (isFromService)
            //        {
            //            //移除属性
            //            context?.Schema.Definitions.Remove(prop.Name);
            //            return;
            //        }
            //    }
            //}

        }
    }
}