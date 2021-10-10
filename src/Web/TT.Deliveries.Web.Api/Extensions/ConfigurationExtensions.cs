using Microsoft.Extensions.Configuration;

namespace TT.Deliveries.Web.Api
{
    public static class ConfigurationExtensions
    {
        public static T Bind<T>(this IConfiguration configuration)
            where T : new()
        {
            var instance = new T();
            configuration.Bind(instance);

            return instance;
        }
    }
}