using System;
using System.Web;
using System.Web.Mvc;

namespace KindergartenSystem.Filters
{
    public class GlobalExceptionFilter : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext == null || filterContext.ExceptionHandled)
            {
                return;
            }

            var exception = filterContext.Exception;
            var request = filterContext.HttpContext.Request;
            
            // Log the error (in production, use proper logging framework like NLog, Serilog)
            LogError(exception, request);

            // Handle different exception types
            if (exception is HttpException httpException)
            {
                HandleHttpException(filterContext, httpException);
            }
            else if (exception is ArgumentException || exception is InvalidOperationException)
            {
                HandleBusinessException(filterContext, exception);
            }
            else
            {
                HandleGeneralException(filterContext, exception);
            }

            filterContext.ExceptionHandled = true;
        }

        private void HandleHttpException(ExceptionContext filterContext, HttpException httpException)
        {
            var statusCode = httpException.GetHttpCode();
            
            switch (statusCode)
            {
                case 404:
                    filterContext.Result = new ViewResult
                    {
                        ViewName = "NotFound",
                        ViewData = new ViewDataDictionary<string>("The requested resource was not found.")
                    };
                    break;
                case 403:
                    filterContext.Result = new ViewResult
                    {
                        ViewName = "Forbidden",
                        ViewData = new ViewDataDictionary<string>("Access denied.")
                    };
                    break;
                default:
                    filterContext.Result = new ViewResult
                    {
                        ViewName = "Error",
                        ViewData = new ViewDataDictionary<string>("An error occurred while processing your request.")
                    };
                    break;
            }
            
            filterContext.HttpContext.Response.StatusCode = statusCode;
        }

        private void HandleBusinessException(ExceptionContext filterContext, Exception exception)
        {
            // Business logic exceptions - show user-friendly message
            filterContext.Result = new ViewResult
            {
                ViewName = "Error",
                ViewData = new ViewDataDictionary<string>(exception.Message)
            };
            filterContext.HttpContext.Response.StatusCode = 400;
        }

        private void HandleGeneralException(ExceptionContext filterContext, Exception exception)
        {
            // General exceptions - show generic message, don't expose details
            filterContext.Result = new ViewResult
            {
                ViewName = "Error",
                ViewData = new ViewDataDictionary<string>("An unexpected error occurred. Please try again later.")
            };
            filterContext.HttpContext.Response.StatusCode = 500;
        }

        private void LogError(Exception exception, HttpRequestBase request)
        {
            try
            {
                // In production, replace with proper logging framework
                var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] " +
                               $"ERROR: {exception.Message} | " +
                               $"URL: {request.Url} | " +
                               $"User: {request.RequestContext.HttpContext.User?.Identity?.Name ?? "Anonymous"} | " +
                               $"IP: {GetClientIP(request)} | " +
                               $"UserAgent: {request.UserAgent}";
                               
                // Write to Event Log or file system
                System.Diagnostics.EventLog.WriteEntry("KindergartenSystem", 
                    logMessage + Environment.NewLine + exception.StackTrace, 
                    System.Diagnostics.EventLogEntryType.Error);
            }
            catch
            {
                // Fail silently if logging fails - don't break the application
            }
        }

        private string GetClientIP(HttpRequestBase request)
        {
            var ip = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(ip))
            {
                ip = request.ServerVariables["REMOTE_ADDR"];
            }
            return ip ?? "Unknown";
        }
    }
}