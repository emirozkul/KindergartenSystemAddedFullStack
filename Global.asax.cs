using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace KindergartenSystem
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie == null) return;

            try
            {
                var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                if (authTicket == null) return;

                var userData = authTicket.UserData.Split('|');
                if (userData.Length != 7) return;

                var principal = new KindergartenSystem.Auth.KindergartenPrincipal(authTicket.Name)
                {
                    UserId = int.Parse(userData[0]),
                    KindergartenId = int.Parse(userData[1]),
                    Username = userData[2],
                    Email = userData[3],
                    Role = userData[4],
                    KindergartenName = userData[5],
                    Subdomain = userData[6]
                };

                HttpContext.Current.User = principal;
            }
            catch
            {
                FormsAuthentication.SignOut();
            }
        }

    }
}



