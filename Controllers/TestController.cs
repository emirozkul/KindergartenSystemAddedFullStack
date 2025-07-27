using System;
using System.Linq;
using System.Web.Mvc;
using KindergartenSystem.Data;

namespace KindergartenSystem.Controllers
{
    public class TestController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.ServerTime = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            ViewBag.MachineName = Environment.MachineName;
            ViewBag.RequestUrl = Request.Url.ToString();
            
            // Show subdomain detection info
            var subdomain = GetSubdomain();
            ViewBag.DetectedSubdomain = string.IsNullOrEmpty(subdomain) ? "(boş)" : subdomain;
            
            // Test database connectivity
            try
            {
                using (var context = new KindergartenContext())
                {
                    var kindergartenCount = context.Kindergartens.Count();
                    ViewBag.DatabaseStatus = $"✅ Database connected successfully! Found {kindergartenCount} kindergarten(s).";
                    
                    if (kindergartenCount > 0)
                    {
                        var firstKindergarten = context.Kindergartens.First();
                        ViewBag.SampleKindergarten = $"Sample: {firstKindergarten.Name} (Subdomain: {firstKindergarten.Subdomain})";
                        
                        // Test relationships
                        var settingsCount = context.GeneralSettings.Count(s => s.KindergartenId == firstKindergarten.Id);
                        var programsCount = context.CoreEducationPrograms.Count(p => p.KindergartenId == firstKindergarten.Id);
                        var eventsCount = context.Events.Count(e => e.KindergartenId == firstKindergarten.Id);
                        
                        ViewBag.RelationshipTest = $"Relationships working: Settings({settingsCount}), Programs({programsCount}), Events({eventsCount})";
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.DatabaseStatus = $"❌ Database error: {ex.Message}";
                ViewBag.ErrorDetails = ex.ToString();
            }
            
            return View();
        }
        
        public ActionResult Html()
        {
            return File("~/test.html", "text/html");
        }
        
        private string GetSubdomain()
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
    }
}