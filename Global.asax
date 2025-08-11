<%@ Application Language="C#" %>
<%@ Import Namespace="KindergartenSystem" %>

<script runat="server">
    void Application_Start(object sender, EventArgs e)
    {
        try
        {
            // Register areas, routes, and filters in proper order
            System.Web.Mvc.AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(System.Web.Mvc.GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(System.Web.Routing.RouteTable.Routes);
            
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
            // Log to event log or proper logging framework in production
            throw;
        }
    }
</script>