using KindergartenSystem.Data;
using KindergartenSystem.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace KindergartenSystem.Auth
{
    // Custom Authorization Attribute
    public class KindergartenAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly string[] _allowedRoles;

        public KindergartenAuthorizeAttribute(params string[] roles)
        {
            _allowedRoles = roles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!base.AuthorizeCore(httpContext))
                return false;

            var user = httpContext.User as KindergartenPrincipal;
            if (user == null)
                return false;

            if (_allowedRoles.Length == 0)
                return true;

            return _allowedRoles.Any(role => user.IsInRole(role));
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
            else
            {
                filterContext.Result = new ViewResult
                {
                    ViewName = "~/Views/Shared/Unauthorized.cshtml"
                };
            }
        }
    }

    // Custom Principal
    public class KindergartenPrincipal : IPrincipal
    {
        public KindergartenPrincipal(string username)
        {
            Identity = new GenericIdentity(username);
        }

        public IIdentity Identity { get; private set; }
        public int UserId { get; set; }
        public int KindergartenId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string KindergartenName { get; set; }
        public string Subdomain { get; set; }

        public bool IsInRole(string role)
        {
            return Role.Equals(role, StringComparison.OrdinalIgnoreCase);
        }
    }

    // Authentication Service
    public interface IAuthenticationService
    {
        User ValidateUser(string email, string password, int? kindergartenId);
        void SignIn(User user, bool rememberMe);
        void SignOut();
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly KindergartenContext _context;

        public AuthenticationService(KindergartenContext context)
        {
            _context = context;
        }

        public User ValidateUser(string email, string password, int? kindergartenId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"ValidateUser called: email={email}, kindergartenId={kindergartenId}");
                
                var query = _context.Users
                    .Include("Kindergarten")
                    .Where(u => u.Email == email && u.IsActive);

                // For SuperAdmin login, kindergartenId can be null
                if (kindergartenId.HasValue)
                {
                    query = query.Where(u => u.KindergartenId == kindergartenId.Value);
                }
                else
                {
                    // For SuperAdmin, accept users with Role = "SuperAdmin" and null KindergartenId
                    query = query.Where(u => u.Role == "SuperAdmin" && u.KindergartenId == null);
                }

                var user = query.FirstOrDefault();
                
                System.Diagnostics.Debug.WriteLine($"User found: {user != null}");
                if (user != null)
                {
                    System.Diagnostics.Debug.WriteLine($"User details: Email={user.Email}, Role={user.Role}, KindergartenId={user.KindergartenId}, Active={user.IsActive}");
                    
                    var passwordValid = VerifyPassword(password, user.PasswordHash);
                    System.Diagnostics.Debug.WriteLine($"Password valid: {passwordValid}");
                    
                    if (passwordValid)
                    {
                        return user;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No user found matching criteria");
                    
                    // Debug: Show all SuperAdmin users in database
                    var allSuperAdmins = _context.Users.Where(u => u.Role == "SuperAdmin").ToList();
                    System.Diagnostics.Debug.WriteLine($"All SuperAdmin users in database: {allSuperAdmins.Count}");
                    foreach (var sa in allSuperAdmins)
                    {
                        System.Diagnostics.Debug.WriteLine($"  - Email: {sa.Email}, KindergartenId: {sa.KindergartenId}, Active: {sa.IsActive}");
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ValidateUser error: {ex.Message}");
                return null;
            }
        }

        public void SignIn(User user, bool rememberMe)
        {
            var userData = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}",
                user.Id,
                user.KindergartenId.HasValue ? user.KindergartenId.Value : 0,
                user.Username,
                user.Email,
                user.Role,
                user.Kindergarten?.Name ?? "System",
                user.Kindergarten?.Subdomain ?? "system");

            var ticket = new FormsAuthenticationTicket(
                1,
                user.Username,
                DateTime.Now,
                DateTime.Now.AddMinutes(rememberMe ? 43200 : 60), // 30 days or 1 hour
                rememberMe,
                userData,
                FormsAuthentication.FormsCookiePath);

            var encryptedTicket = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
            {
                HttpOnly = true,
                Secure = FormsAuthentication.RequireSSL,
                Path = FormsAuthentication.FormsCookiePath
            };

            if (rememberMe)
            {
                cookie.Expires = DateTime.Now.AddDays(30);
            }

            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public void SignOut()
        {
            FormsAuthentication.SignOut();
        }

        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "K1nd3rg@rt3n"));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hashedPassword;
        }
    }
}