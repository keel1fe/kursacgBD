using System;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using System.Data.Entity; // Добавьте эту строку
using kursach.Models; // Добавьте эту строку

namespace kursach
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Инициализация базы данных (без авторизации)
            Database.SetInitializer<DanceContext>(new CreateDatabaseIfNotExists<DanceContext>());

            AreaRegistration.RegisterAllAreas();

            // Регистрация только фильтров ошибок (без авторизации)
            GlobalFilters.Filters.Add(new HandleErrorAttribute());

            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Настройка кастомных ошибок
            ConfigureCustomErrors();
        }

        private void ConfigureCustomErrors()
        {
            var config = WebConfigurationManager.OpenWebConfiguration("~");
            var customErrorsSection = (CustomErrorsSection)config.GetSection("system.web/customErrors");
            customErrorsSection.Mode = CustomErrorsMode.On;
            config.Save();
        }

        protected void Application_Error()
        {
            var exception = Server.GetLastError();
            var httpException = exception as HttpException ?? new HttpException(500, "Internal Server Error", exception);

            System.Diagnostics.Trace.TraceError($"Application Error: {httpException.Message}");

            Server.ClearError();

            // Перенаправление на соответствующий обработчик ошибок
            var routeData = new RouteData();
            routeData.Values["controller"] = "Error";
            routeData.Values["exception"] = exception;

            switch (httpException.GetHttpCode())
            {
                case 400:
                    routeData.Values["action"] = "BadRequest";
                    break;
                case 404:
                    routeData.Values["action"] = "NotFound";
                    break;
                case 500:
                    routeData.Values["action"] = "ServerError";
                    break;
                default:
                    routeData.Values["action"] = "Index";
                    break;
            }

            //IController errorController = new ErrorController();
            //errorController.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
        }
    }
}