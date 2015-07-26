using System.Web.Http;
using WebCommandDevice.ControlDevice.Pool;

namespace WebCommandDevice
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Конфигурация и службы веб-API
            PoolConnection.InitializationPool(1);

            // Маршруты веб-API
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute("DeviceCommandApi", "api/{action}", new { controller = "ControlDevice", id = RouteParameter.Optional }
             );

            config.Routes.MapHttpRoute("DeviceControlApi", "api/{action}", new { controller = "SendDevice", id = RouteParameter.Optional }
                        );

            config.Formatters.Remove(config.Formatters.XmlFormatter);
        }
    }
}
