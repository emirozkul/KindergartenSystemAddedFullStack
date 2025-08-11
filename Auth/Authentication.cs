using KindergartenSystem.Data;
using KindergartenSystem.Models;
using System;
using System.Data.Entity;
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
                
                var query = _context.Users.AsQueryable();

                // If kindergartenId is null, it might be a SuperAdmin login attempt
                if (!kindergartenId.HasValue)
                {
                    // First, check for a SuperAdmin with the given email
                    var superAdmin = query.FirstOrDefault(u => u.Email == email && u.Role == "SuperAdmin" && u.IsActive);
                    if (superAdmin != null)
                    {
                        if (VerifyPassword(password, superAdmin.PasswordHash))
                        {
                            return superAdmin;
                        }
                    }
                }
                
                // If not a SuperAdmin or if SuperAdmin password was wrong, check for a regular user
                var user = _context.Users
                    .Include("Kindergarten")
                    .FirstOrDefault(u => u.Email == email && u.KindergartenId == kindergartenId && u.IsActive);

                if (user != null)
                {
                    
                    var passwordValid = VerifyPassword(password, user.PasswordHash);
                    
                    if (passwordValid)
                    {
                        return user;
                    }
                }
                else
                {
                    
                    // Debug: Show all SuperAdmin users in database
                    var allSuperAdmins = _context.Users.Where(u => u.Role == "SuperAdmin").ToList();
                    foreach (var sa in allSuperAdmins)
                    {
                    }
                }

                return null;
            }
            catch (Exception)
            {
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
            // Generate a random salt for each password
            byte[] salt = new byte[16];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Use PBKDF2 (RFC 2898) for secure password hashing
            using (var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(password, salt, 100000))
            {
                byte[] hash = pbkdf2.GetBytes(32);
                
                // Combine salt and hash
                byte[] hashBytes = new byte[48];
                Array.Copy(salt, 0, hashBytes, 0, 16);
                Array.Copy(hash, 0, hashBytes, 16, 32);
                
                return Convert.ToBase64String(hashBytes);
            }
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                // Extract the bytes
                byte[] hashBytes = Convert.FromBase64String(hashedPassword);
                
                // Get the salt
                byte[] salt = new byte[16];
                Array.Copy(hashBytes, 0, salt, 0, 16);
                
                // Compute the hash on the password the user entered
                using (var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(password, salt, 100000))
                {
                    byte[] hash = pbkdf2.GetBytes(32);
                    
                    // Compare the results
                    for (int i = 0; i < 32; i++)
                    {
                        if (hashBytes[i + 16] != hash[i])
                            return false;
                    }
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}