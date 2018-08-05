using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WorkFlowManager.Common.ViewModels;
using WorkFlowManager.Web.Infra;

namespace WorkFlowManager.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            ModelBinders.Binders.Add(typeof(WorkFlowFormViewModel), new WorkFlowFormModelBinder());

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            DependencyRegistrar.RegisterDependencies();
        }
        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
        }

    }
}
