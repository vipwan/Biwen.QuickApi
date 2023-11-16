
namespace Biwen.QuickApi.Swagger
{
    using NJsonSchema.Generation;

    internal class QuickApiSchemaProcessor : ISchemaProcessor
    {
        public void Process(SchemaProcessorContext context)
        {
            //var tRequest = context.ContextualType;
            //if (tRequest == null)
            //{
            //    return;
            //}

            //// 移除BaseRequestOf
            //var BaseRequestOfT = "BaseRequest`1";
            //if (tRequest.Type.Name.StartsWith(BaseRequestOfT))
            //{
            //    context.Schema.Definitions.Remove(tRequest.Type.Name);
            //    return;
            //}
        }
    }
}