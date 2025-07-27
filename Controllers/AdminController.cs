using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KindergartenSystem.Auth;
using KindergartenSystem.Models;
using KindergartenSystem.ViewModels;

namespace KindergartenSystem.Controllers
{
    [KindergartenAuthorize("SuperAdmin", "KindergartenAdmin")]
    public class AdminController : AdminBaseController
    {
        public ActionResult Index()
        {
            var dashboardData = new AdminDashboardViewModel
            {
                KindergartenName = CurrentKindergarten?.Name ?? "Admin Panel",
                TotalEvents = Context.Events.Count(e => e.KindergartenId == CurrentUser.KindergartenId),
                TotalStaff = Context.StaffMembers.Count(s => s.KindergartenId == CurrentUser.KindergartenId),
                TotalPrograms = Context.CoreEducationPrograms.Count(p => p.KindergartenId == CurrentUser.KindergartenId),
                TotalTestimonials = Context.ParentTestimonials.Count(t => t.KindergartenId == CurrentUser.KindergartenId),
                RecentContactSubmissions = Context.ContactSubmissions
                    .Where(c => c.KindergartenId == CurrentUser.KindergartenId)
                    .OrderByDescending(c => c.SubmittedDate)
                    .Take(5)
                    .ToList(),
                RecentAnnouncements = Context.Announcements
                    .Where(a => a.KindergartenId == CurrentUser.KindergartenId)
                    .OrderByDescending(a => a.CreatedDate)
                    .Take(5)
                    .ToList()
            };

            return View(dashboardData);
        }

        public ActionResult Settings()
        {
            var settings = Context.GeneralSettings
                .FirstOrDefault(s => s.KindergartenId == CurrentUser.KindergartenId);

            if (settings == null)
            {
                settings = new GeneralSettings { KindergartenId = CurrentUser.KindergartenId };
                Context.GeneralSettings.Add(settings);
                Context.SaveChanges();
            }

            var missionVision = Context.MissionVisions
                .FirstOrDefault(m => m.KindergartenId == CurrentUser.KindergartenId);

            if (missionVision == null)
            {
                missionVision = new MissionVision { KindergartenId = CurrentUser.KindergartenId };
                Context.MissionVisions.Add(missionVision);
                Context.SaveChanges();
            }

            var viewModel = new SettingsViewModel
            {
                GeneralSettings = settings,
                MissionVision = missionVision
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Settings(SettingsViewModel model, HttpPostedFileBase logoFile, HttpPostedFileBase footerLogoFile, HttpPostedFileBase heroBackgroundFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var settings = Context.GeneralSettings
                        .FirstOrDefault(s => s.KindergartenId == CurrentUser.KindergartenId);

                    if (settings != null)
                    {
                        // Update settings
                        settings.Slogan = model.GeneralSettings.Slogan;
                        settings.SubSlogan = model.GeneralSettings.SubSlogan;
                        settings.FooterSlogan = model.GeneralSettings.FooterSlogan;
                        settings.Address = model.GeneralSettings.Address;
                        settings.Phone = model.GeneralSettings.Phone;
                        settings.Email = model.GeneralSettings.Email;
                        settings.FacebookUrl = model.GeneralSettings.FacebookUrl;
                        settings.TwitterUrl = model.GeneralSettings.TwitterUrl;
                        settings.InstagramUrl = model.GeneralSettings.InstagramUrl;
                        settings.GoogleMapEmbed = model.GeneralSettings.GoogleMapEmbed;
                        settings.UpdatedDate = DateTime.Now;

                        // Handle file uploads
                        if (logoFile != null && logoFile.ContentLength > 0)
                        {
                            settings.LogoPath = SaveFile(logoFile, "logos");
                        }
                        if (footerLogoFile != null && footerLogoFile.ContentLength > 0)
                        {
                            settings.FooterLogoPath = SaveFile(footerLogoFile, "logos");
                        }
                        if (heroBackgroundFile != null && heroBackgroundFile.ContentLength > 0)
                        {
                            settings.HeroBackgroundPath = SaveFile(heroBackgroundFile, "backgrounds");
                        }
                    }

                    var missionVision = Context.MissionVisions
                        .FirstOrDefault(m => m.KindergartenId == CurrentUser.KindergartenId);

                    if (missionVision != null)
                    {
                        missionVision.MissionTitle = model.MissionVision.MissionTitle;
                        missionVision.MissionText = model.MissionVision.MissionText;
                        missionVision.VisionTitle = model.MissionVision.VisionTitle;
                        missionVision.VisionText = model.MissionVision.VisionText;
                        missionVision.UpdatedDate = DateTime.Now;
                    }

                    Context.SaveChanges();
                    TempData["Success"] = "Settings updated successfully!";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error updating settings: " + ex.Message;
                }
            }

            return View(model);
        }

        private string SaveFile(HttpPostedFileBase file, string folder)
        {
            if (file == null || file.ContentLength == 0)
                return null;

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var kindergartenFolder = Path.Combine(Server.MapPath("~/Content/uploads"), CurrentUser.KindergartenId.ToString(), folder);
            
            if (!Directory.Exists(kindergartenFolder))
            {
                Directory.CreateDirectory(kindergartenFolder);
            }

            var filePath = Path.Combine(kindergartenFolder, fileName);
            file.SaveAs(filePath);

            return $"/Content/uploads/{CurrentUser.KindergartenId}/{folder}/{fileName}";
        }
    }
}