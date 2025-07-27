using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using KindergartenSystem.Auth;
using KindergartenSystem.Data;
using KindergartenSystem.Models;
using KindergartenSystem.ViewModels;

namespace KindergartenSystem.Controllers
{
    [KindergartenAuthorize("SuperAdmin")]
    public class SuperAdminController : AdminBaseController
    {
        public ActionResult Index()
        {
            var dashboardData = new SuperAdminDashboardViewModel
            {
                TotalKindergartens = Context.Kindergartens.Count(),
                ActiveKindergartens = Context.Kindergartens.Count(k => k.IsActive),
                TotalUsers = Context.Users.Count(),
                ActiveUsers = Context.Users.Count(u => u.IsActive),
                RecentKindergartens = Context.Kindergartens
                    .OrderByDescending(k => k.CreatedDate)
                    .Take(5)
                    .ToList(),
                RecentUsers = Context.Users
                    .Include("Kindergarten")
                    .OrderByDescending(u => u.CreatedDate)
                    .Take(5)
                    .ToList()
            };

            return View(dashboardData);
        }

        #region Kindergarten Management

        public ActionResult Kindergartens()
        {
            var kindergartens = Context.Kindergartens
                .OrderByDescending(k => k.CreatedDate)
                .ToList();

            return View(kindergartens);
        }

        public ActionResult CreateKindergarten()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateKindergarten(Kindergarten kindergarten)
        {
            if (ModelState.IsValid)
            {
                // Check if subdomain already exists
                var existingSubdomain = Context.Kindergartens
                    .Any(k => k.Subdomain.ToLower() == kindergarten.Subdomain.ToLower());

                if (existingSubdomain)
                {
                    ModelState.AddModelError("Subdomain", "This subdomain is already taken.");
                    return View(kindergarten);
                }

                kindergarten.CreatedDate = DateTime.Now;
                kindergarten.UpdatedDate = DateTime.Now;
                
                Context.Kindergartens.Add(kindergarten);
                Context.SaveChanges();

                // Create default settings
                CreateDefaultSettings(kindergarten.Id);

                TempData["Success"] = "Kindergarten created successfully!";
                return RedirectToAction("Kindergartens");
            }

            return View(kindergarten);
        }

        public ActionResult EditKindergarten(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var kindergarten = Context.Kindergartens.Find(id);
            if (kindergarten == null)
            {
                return HttpNotFound();
            }

            return View(kindergarten);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditKindergarten(Kindergarten kindergarten)
        {
            if (ModelState.IsValid)
            {
                var existingKindergarten = Context.Kindergartens.Find(kindergarten.Id);
                if (existingKindergarten != null)
                {
                    // Check if subdomain already exists (excluding current kindergarten)
                    var existingSubdomain = Context.Kindergartens
                        .Any(k => k.Subdomain.ToLower() == kindergarten.Subdomain.ToLower() && k.Id != kindergarten.Id);

                    if (existingSubdomain)
                    {
                        ModelState.AddModelError("Subdomain", "This subdomain is already taken.");
                        return View(kindergarten);
                    }

                    existingKindergarten.Name = kindergarten.Name;
                    existingKindergarten.Subdomain = kindergarten.Subdomain;
                    existingKindergarten.IsActive = kindergarten.IsActive;
                    existingKindergarten.UpdatedDate = DateTime.Now;

                    Context.SaveChanges();
                    TempData["Success"] = "Kindergarten updated successfully!";
                    return RedirectToAction("Kindergartens");
                }
            }

            return View(kindergarten);
        }

        public ActionResult DeleteKindergarten(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var kindergarten = Context.Kindergartens.Find(id);
            if (kindergarten == null)
            {
                return HttpNotFound();
            }

            return View(kindergarten);
        }

        [HttpPost, ActionName("DeleteKindergarten")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteKindergartenConfirmed(int id)
        {
            var kindergarten = Context.Kindergartens.Find(id);
            if (kindergarten != null)
            {
                // Note: Consider soft delete or data archival in production
                Context.Kindergartens.Remove(kindergarten);
                Context.SaveChanges();
                TempData["Success"] = "Kindergarten deleted successfully!";
            }

            return RedirectToAction("Kindergartens");
        }

        #endregion

        #region User Management

        public ActionResult Users()
        {
            var users = Context.Users
                .Include("Kindergarten")
                .OrderByDescending(u => u.CreatedDate)
                .ToList();

            return View(users);
        }

        public ActionResult CreateUser()
        {
            ViewBag.Kindergartens = new SelectList(Context.Kindergartens.Where(k => k.IsActive), "Id", "Name");
            ViewBag.Roles = new SelectList(new[] { "SuperAdmin", "KindergartenAdmin", "Teacher" });
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateUser(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                var existingUser = Context.Users.Any(u => u.Email.ToLower() == model.Email.ToLower());
                if (existingUser)
                {
                    ModelState.AddModelError("Email", "This email is already registered.");
                    ViewBag.Kindergartens = new SelectList(Context.Kindergartens.Where(k => k.IsActive), "Id", "Name", model.KindergartenId);
                    ViewBag.Roles = new SelectList(new[] { "SuperAdmin", "KindergartenAdmin", "Teacher" }, model.Role);
                    return View(model);
                }

                var authService = new AuthenticationService(Context);
                var user = new User
                {
                    KindergartenId = model.Role == "SuperAdmin" ? (int?)null : model.KindergartenId,
                    Username = model.Username,
                    Email = model.Email,
                    PasswordHash = authService.HashPassword(model.Password),
                    Role = model.Role,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                Context.Users.Add(user);
                Context.SaveChanges();

                TempData["Success"] = "User created successfully!";
                return RedirectToAction("Users");
            }

            ViewBag.Kindergartens = new SelectList(Context.Kindergartens.Where(k => k.IsActive), "Id", "Name", model.KindergartenId);
            ViewBag.Roles = new SelectList(new[] { "SuperAdmin", "KindergartenAdmin", "Teacher" }, model.Role);
            return View(model);
        }

        public ActionResult EditUser(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = Context.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var model = new EditUserViewModel
            {
                Id = user.Id,
                KindergartenId = user.KindergartenId ?? 0,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive
            };

            ViewBag.Kindergartens = new SelectList(Context.Kindergartens.Where(k => k.IsActive), "Id", "Name", user.KindergartenId);
            ViewBag.Roles = new SelectList(new[] { "SuperAdmin", "KindergartenAdmin", "Teacher" }, user.Role);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = Context.Users.Find(model.Id);
                if (user != null)
                {
                    // Check if email already exists (excluding current user)
                    var existingUser = Context.Users.Any(u => u.Email.ToLower() == model.Email.ToLower() && u.Id != model.Id);
                    if (existingUser)
                    {
                        ModelState.AddModelError("Email", "This email is already registered.");
                        ViewBag.Kindergartens = new SelectList(Context.Kindergartens.Where(k => k.IsActive), "Id", "Name", model.KindergartenId);
                        ViewBag.Roles = new SelectList(new[] { "SuperAdmin", "KindergartenAdmin", "Teacher" }, model.Role);
                        return View(model);
                    }

                    user.KindergartenId = model.Role == "SuperAdmin" ? (int?)null : model.KindergartenId;
                    user.Username = model.Username;
                    user.Email = model.Email;
                    user.Role = model.Role;
                    user.IsActive = model.IsActive;

                    // Update password if provided
                    if (!string.IsNullOrEmpty(model.NewPassword))
                    {
                        var authService = new AuthenticationService(Context);
                        user.PasswordHash = authService.HashPassword(model.NewPassword);
                    }

                    Context.SaveChanges();
                    TempData["Success"] = "User updated successfully!";
                    return RedirectToAction("Users");
                }
            }

            ViewBag.Kindergartens = new SelectList(Context.Kindergartens.Where(k => k.IsActive), "Id", "Name", model.KindergartenId);
            ViewBag.Roles = new SelectList(new[] { "SuperAdmin", "KindergartenAdmin", "Teacher" }, model.Role);
            return View(model);
        }

        public ActionResult DeleteUser(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = Context.Users.Include("Kindergarten").FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteUserConfirmed(int id)
        {
            var user = Context.Users.Find(id);
            if (user != null)
            {
                Context.Users.Remove(user);
                Context.SaveChanges();
                TempData["Success"] = "User deleted successfully!";
            }

            return RedirectToAction("Users");
        }

        #endregion

        private void CreateDefaultSettings(int kindergartenId)
        {
            // Create default general settings
            var settings = new GeneralSettings
            {
                KindergartenId = kindergartenId,
                Slogan = "Welcome to Our Kindergarten",
                SubSlogan = "A safe and nurturing environment for your children",
                UpdatedDate = DateTime.Now
            };
            Context.GeneralSettings.Add(settings);

            // Create default mission/vision
            var missionVision = new MissionVision
            {
                KindergartenId = kindergartenId,
                MissionTitle = "Our Mission",
                MissionText = "To provide quality early childhood education in a nurturing environment.",
                VisionTitle = "Our Vision",
                VisionText = "To be a leading kindergarten that prepares children for lifelong learning.",
                UpdatedDate = DateTime.Now
            };
            Context.MissionVisions.Add(missionVision);

            // Create default about us content
            var aboutUs = new AboutUsContent
            {
                KindergartenId = kindergartenId,
                Title = "About Our Kindergarten",
                Description = "We are dedicated to providing the best early childhood education experience.",
                UpdatedDate = DateTime.Now
            };
            Context.AboutUsContents.Add(aboutUs);

            Context.SaveChanges();
        }
    }
}