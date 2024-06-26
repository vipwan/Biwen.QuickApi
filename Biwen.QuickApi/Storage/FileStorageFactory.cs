namespace Biwen.QuickApi.Storage
{
    /// <summary>
    /// FileStorage Factory
    /// </summary>
    public class FileStorageFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public FileStorageFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IFileStorage Create(string key)
        {
            return _serviceProvider.GetRequiredKeyedService<IFileStorage>(key);
        }
    }
}