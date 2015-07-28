using System.Web.Http;
using System.Web.Http.Routing.Constraints;
using WebCommandDevice.ControlDevice.Pool;

namespace WebCommandDevice
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Конфигурация и службы веб-API
            PoolConnection.InitializationPool(10);

            // Маршруты веб-API
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                "HistoryCommandApi", "{action}/{deviceId}",
                new { controller = "HistoryDevice", deviceId = RouteParameter.Optional },
                constraints: new { action = new MaxLengthRouteConstraint(7) });

            config.Routes.MapHttpRoute(
                "DeviceCommandApi", "{action}/{deviceId}/{timeout}",
                new { controller = "ControlDevice", deviceId = RouteParameter.Optional, timeout = RouteParameter.Optional },
                constraints: new { action = new MaxLengthRouteConstraint(8) });


            config.Formatters.Remove(config.Formatters.XmlFormatter);
        }
    }
}
