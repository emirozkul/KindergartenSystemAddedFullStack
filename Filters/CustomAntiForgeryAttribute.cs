using System;
using System.Linq;
using System.Web.Mvc;

namespace KindergartenSystem.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class CustomValidateAntiForgeryTokenAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            // Only apply to POST, PUT, PATCH, DELETE requests
            var request = filterContext.HttpContext.Request;
            var httpMethod = request.HttpMethod.ToUpper();
            
            if (httpMethod == "POST" || httpMethod == "PUT" || httpMethod == "PATCH" || httpMethod == "DELETE")
            {
                // Check if the action or controller has [IgnoreAntiForgeryToken] attribute
                var actionDescriptor = filterContext.ActionDescriptor;
                var controllerDescriptor = actionDescriptor.ControllerDescriptor;
                
                var ignoreTokenOnAction = actionDescriptor.GetCustomAttributes(typeof(IgnoreAntiForgeryTokenAttribute), false).Any();
                var ignoreTokenOnController = controllerDescriptor.GetCustomAttributes(typeof(IgnoreAntiForgeryTokenAttribute), false).Any();
                
                if (ignoreTokenOnAction || ignoreTokenOnController)
                {
                    return; // Skip validation
                }
                
                try
                {
                    // Validate the anti-forgery token
                    var validator = new ValidateAntiForgeryTokenAttribute();
                    validator.OnAuthorization(filterContext);
                }
                catch (Exception)
                {
                    filterContext.Result = new HttpStatusCodeResult(400, "CSRF token validation failed");
                }
            }
        }
    }
    
    /// <summary>
    /// Use this attribute to ignore CSRF validation on specific actions or controllers
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class IgnoreAntiForgeryTokenAttribute : Attribute
    {
    }
}