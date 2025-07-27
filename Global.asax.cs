using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Data.Entity;

namespace KindergartenSystem
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Set Turkish culture for proper character encoding
            var turkishCulture = new CultureInfo("tr-TR");
            Thread.CurrentThread.CurrentCulture = turkishCulture;
            Thread.CurrentThread.CurrentUICulture = turkishCulture;
            
            // Initialize database
            Database.SetInitializer(new KindergartenSystem.Data.DatabaseInitializer());
            
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            
            // Test database connection and ensure it's created
            try
            {
                using (var context = new KindergartenSystem.Data.KindergartenContext())
                {
                    // Force database creation/update
                    context.Database.Initialize(true);
                    
                    // Test database connection with existing data
                    var kindergartenCount = context.Kindergartens.Count();
                    var settingsCount = context.GeneralSettings.Count();
                    var programsCount = context.CoreEducationPrograms.Count();
                    var userCount = context.Users.Count();
                    
                    System.Diagnostics.Debug.WriteLine($"✅ Database connection verified: Kindergartens({kindergartenCount}), Settings({settingsCount}), Programs({programsCount}), Users({userCount})");
                    
                    // Log admin credentials
                    var adminUser = context.Users.FirstOrDefault(u => u.Role == "KindergartenAdmin");
                    if (adminUser != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"👤 Admin Login: {adminUser.Email} / admin123");
                    }
                    
                    var superAdmin = context.Users.FirstOrDefault(u => u.Role == "SuperAdmin");
                    if (superAdmin != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"🔧 SuperAdmin Login: {superAdmin.Email} / SuperAdmin123!");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Database connection failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            }
        }

        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            // Set Turkish culture for each request to ensure proper character encoding
            var turkishCulture = new CultureInfo("tr-TR");
            Thread.CurrentThread.CurrentCulture = turkishCulture;
            Thread.CurrentThread.CurrentUICulture = turkishCulture;
            
            // Set response encoding to UTF-8
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.HeaderEncoding = System.Text.Encoding.UTF8;
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





