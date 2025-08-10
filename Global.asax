<%@ Application Language="C#" %>

<script runat="server">
    void Application_Start(object sender, EventArgs e)
    {
        try
        {
            System.Web.Mvc.AreaRegistration.RegisterAllAreas();
            System.Web.Routing.RouteTable.Routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            System.Web.Routing.RouteTable.Routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = System.Web.Mvc.UrlParameter.Optional }
            );
            
            // Initialize database
            System.Data.Entity.Database.SetInitializer(new KindergartenSystem.Data.DatabaseInitializer());
            
            // Force database initialization
            using (var context = new KindergartenSystem.Data.KindergartenContext())
            {
                context.Database.Initialize(force: false);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Error during Application_Start: " + ex.Message);
        }
    }
</script>