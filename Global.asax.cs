
namespace ComplyNowWebMVC
{
    using System.Data.Entity;
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Optimization;
    using System.Web.Routing;
    using Factories;
    using Models;

    /// <inheritdoc />
    /// <summary>
    /// The mvc application.
    /// </summary>
    public class MvcApplication : System.Web.HttpApplication
    {
        /// <summary>
        /// The application_ start.
        /// </summary>
        protected void Application_Start()
        {
            Database.SetInitializer<ApplicationDbContext>(null);
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AutoMapperConfiguration.Configure();

            // create a custome controller factory so we can inject a common repository
            RegisterCustomControllerFactory();

        }

        private void RegisterCustomControllerFactory()
        {
            IControllerFactory factory = new ComplyNowControllerFactory();
            ControllerBuilder.Current.SetControllerFactory(factory);
        }
    }

}


