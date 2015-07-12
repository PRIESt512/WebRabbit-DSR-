using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace WebControlDevice
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Конфигурация и службы веб-API

            // Маршруты веб-API
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                            name: "DeviceControlApi",
                            routeTemplate: "api/{action}",
                            defaults: new { controller = "SendDevice", id = RouteParameter.Optional }
                        );

            config.Formatters.Remove(config.Formatters.XmlFormatter);
        }
    }
}
