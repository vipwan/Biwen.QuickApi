
namespace Biwen.QuickApi.Swagger
{
    using NJsonSchema.Generation;

    public class QuickApiSchemaProcessor : ISchemaProcessor
    {
        public void Process(SchemaProcessorContext context)
        {
            //var tRequest = context.ContextualType;
            //if (tRequest == null)
            //{
            //    return;
            //}

            //context.Settings.ExcludedTypeNames=new string[] { "BaseResponse" };

            //if (tRequest.Type == typeof(BaseResponse))
            //{
            //    context.Schema.Definitions.Remove(tRequest.Type.Name);
            //    return;
            //}

        }
    }
}