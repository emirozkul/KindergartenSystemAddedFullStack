using System.Web;
using System.Web.Mvc;

namespace KindergartenSystem
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            // Add our custom global exception filter instead of default HandleErrorAttribute
            filters.Add(new KindergartenSystem.Filters.GlobalExceptionFilter());
            
            // Add custom CSRF protection for all POST/PUT/PATCH/DELETE requests
            filters.Add(new KindergartenSystem.Filters.CustomValidateAntiForgeryTokenAttribute());
        }
    }
}
