namespace Biwen.QuickApi.Mapping.Mapster
{
    [CoreModular]
    internal class MapsterModular : ModularBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddMapsterMapper();
        }
    }
}