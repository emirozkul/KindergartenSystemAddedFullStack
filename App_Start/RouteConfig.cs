using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace KindergartenSystem
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Admin area routes
            routes.MapRoute(
                name: "SuperAdminLogin",
                url: "admin/superadmin-login",
                defaults: new { controller = "Account", action = "SuperAdminLogin" }
            );

            routes.MapRoute(
                name: "AdminLogin",
                url: "admin/login",
                defaults: new { controller = "Account", action = "Login", admin = "1" }
            );

            routes.MapRoute(
                name: "AdminDashboard",
                url: "admin",
                defaults: new { controller = "Admin", action = "Index" }
            );

            routes.MapRoute(
                name: "AdminActions",
                url: "admin/{controller}/{action}/{id}",
                defaults: new { action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "KindergartenSystem.Controllers" }
            );

            // SuperAdmin routes
            routes.MapRoute(
                name: "SuperAdmin",
                url: "superadmin/{action}/{id}",
                defaults: new { controller = "SuperAdmin", action = "Index", id = UrlParameter.Optional }
            );

            // Public event detail route
            routes.MapRoute(
                name: "EventDetails",
                url: "etkinlik-detay/{id}",
                defaults: new { controller = "Home", action = "EventDetails", id = UrlParameter.Optional }
            );

            // Public announcement detail route
            routes.MapRoute(
                name: "AnnouncementDetails",
                url: "duyuru-detay/{id}",
                defaults: new { controller = "Home", action = "AnnouncementDetails", id = UrlParameter.Optional }
            );

            // Turkish URL mappings for public pages
            routes.MapRoute(
                name: "TurkishAbout",
                url: "hakkimizda",
                defaults: new { controller = "Home", action = "About" }
            );

            routes.MapRoute(
                name: "TurkishEvents",
                url: "etkinliklerimiz",
                defaults: new { controller = "Home", action = "Events" }
            );

            routes.MapRoute(
                name: "TurkishStaff",
                url: "kadromuz",
                defaults: new { controller = "Home", action = "Staff" }
            );

            routes.MapRoute(
                name: "TurkishAnnouncements",
                url: "duyuru",
                defaults: new { controller = "Home", action = "Announcements" }
            );

            routes.MapRoute(
                name: "TurkishGallery",
                url: "galeri",
                defaults: new { controller = "Home", action = "Gallery" }
            );

            routes.MapRoute(
                name: "TurkishContact",
                url: "iletisim",
                defaults: new { controller = "Home", action = "Contact" }
            );

            // Contact form submission
            routes.MapRoute(
                name: "ContactSubmit",
                url: "contact/submit",
                defaults: new { controller = "Contact", action = "Submit" }
            );

            // Default route
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
