using System;
using System.Linq;
using System.Web.Mvc;
using KindergartenSystem.Auth;
using KindergartenSystem.Data;
using KindergartenSystem.Models;

namespace KindergartenSystem.Controllers
{
    public abstract class BaseController : Controller
    {
        private KindergartenContext _context;
        protected KindergartenContext Context 
        { 
            get 
            { 
                if (_context == null)
                    _context = new KindergartenContext();
                return _context;
            } 
        }
        protected KindergartenPrincipal CurrentUser { get; private set; }
        protected Kindergarten CurrentKindergarten { get; private set; }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Context is now lazy-initialized through property

            if (User.Identity.IsAuthenticated)
            {
                CurrentUser = User as KindergartenPrincipal;
                if (CurrentUser != null)
                {
                    // For SuperAdmin, CurrentKindergarten can be null
                    if (CurrentUser.KindergartenId > 0)
                    {
                        CurrentKindergarten = Context.Kindergartens
                            .FirstOrDefault(k => k.Id == CurrentUser.KindergartenId);
                    }
                    else if (CurrentUser.Role == "SuperAdmin")
                    {
                        // SuperAdmin doesn't have a specific kindergarten - set to first one for display purposes
                        CurrentKindergarten = Context.Kindergartens.FirstOrDefault(k => k.IsActive);
                    }

                    ViewBag.CurrentUser = CurrentUser;
                    ViewBag.CurrentKindergarten = CurrentKindergarten;
                }
            }
            else
            {
                // For public pages, determine kindergarten from subdomain
                var subdomain = GetSubdomain();
                
                if (!string.IsNullOrEmpty(subdomain))
                {
                    CurrentKindergarten = Context.Kindergartens
                        .FirstOrDefault(k => k.Subdomain == subdomain && k.IsActive);
                    
                    if (CurrentKindergarten != null)
                    {
                    }
                    
                    ViewBag.CurrentKindergarten = CurrentKindergarten;
                }
                else
                {
                }
            }

            base.OnActionExecuting(filterContext);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _context != null)
            {
                _context.Dispose();
                _context = null;
            }
            base.Dispose(disposing);
        }

        protected string GetSubdomain()
        {
            var host = Request.Url.Host;
            var parts = host.Split('.');

            // Check if it's localhost or IP
            if (parts.Length < 3 || host.Contains("localhost") || System.Net.IPAddress.TryParse(host, out _))
            {
                // For development, check query string
                return Request.QueryString["subdomain"];
            }

            return parts[0];
        }

        protected ActionResult RedirectToLogin()
        {
            return RedirectToAction("Login", "Admin");
        }

        protected bool IsAuthorized(params string[] roles)
        {
            if (CurrentUser == null) return false;
            if (roles.Length == 0) return true;
            return roles.Any(role => CurrentUser.IsInRole(role));
        }
    }

    // Base controller for admin pages
    [KindergartenAuthorize]
    public abstract class AdminBaseController : BaseController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            // Ensure user can only access their own kindergarten data
            if (CurrentUser != null && CurrentUser.Role != "SuperAdmin")
            {
                // Additional security checks can be added here
            }
        }
    }

    // Base controller for public pages
    public abstract class PublicBaseController : BaseController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            // Load kindergarten-specific settings for public pages
            if (CurrentKindergarten != null)
            {
                var settings = Context.GeneralSettings
                    .FirstOrDefault(s => s.KindergartenId == CurrentKindergarten.Id);
                ViewBag.Settings = settings;
            }
            else
            {
                // For development/debugging, show detailed error
                var subdomain = GetSubdomain();
                var errorMessage = $"Kindergarten not found for subdomain: '{subdomain}'. ";
                
                // Check if any kindergartens exist in database
                var kindergartenCount = Context.Kindergartens.Count();
                errorMessage += $"Total kindergartens in database: {kindergartenCount}. ";
                
                if (kindergartenCount > 0)
                {
                    var allSubdomains = string.Join(", ", Context.Kindergartens.Select(k => k.Subdomain));
                    errorMessage += $"Available subdomains: {allSubdomains}";
                    
                    // If no subdomain provided and we're on localhost, redirect to first available kindergarten
                    if (string.IsNullOrEmpty(subdomain) && Request.Url.Host.Contains("localhost"))
                    {
                        var firstKindergarten = Context.Kindergartens.FirstOrDefault(k => k.IsActive);
                        if (firstKindergarten != null)
                        {
                            var redirectUrl = $"{Request.Url.Scheme}://{Request.Url.Authority}{Request.Url.AbsolutePath}?subdomain={firstKindergarten.Subdomain}";
                            filterContext.Result = new RedirectResult(redirectUrl);
                            return;
                        }
                    }
                }
                
                
                // Show custom error page instead of generic 404
                filterContext.Result = new HttpNotFoundResult(errorMessage);
            }
        }
    }
}