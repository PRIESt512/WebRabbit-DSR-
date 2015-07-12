using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace WebCommandDevice
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Конфигурация и службы веб-API

            // Маршруты веб-API
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                 name: "DeviceCommandApi",
                 routeTemplate: "api/{action}",
                 defaults: new { controller = "ControlDevice", id = RouteParameter.Optional }
             );

            config.Formatters.Remove(config.Formatters.XmlFormatter);
        }
    }
}
