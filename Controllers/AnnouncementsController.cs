using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using KindergartenSystem.Auth;
using KindergartenSystem.Models;

namespace KindergartenSystem.Controllers
{
    [KindergartenAuthorize("SuperAdmin", "KindergartenAdmin")]
    public class AnnouncementsController : AdminBaseController
    {
        public ActionResult Index()
        {
            var announcements = Context.Announcements
                .Where(a => a.KindergartenId == CurrentUser.KindergartenId)
                .OrderByDescending(a => a.AnnouncementDate)
                .ToList();

            return View(announcements);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var announcement = Context.Announcements
                .FirstOrDefault(a => a.Id == id && a.KindergartenId == CurrentUser.KindergartenId);

            if (announcement == null)
            {
                return HttpNotFound();
            }

            return View(announcement);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Announcement announcement)
        {
            if (ModelState.IsValid)
            {
                announcement.KindergartenId = CurrentUser.KindergartenId;
                announcement.CreatedDate = DateTime.Now;

                Context.Announcements.Add(announcement);
                Context.SaveChanges();
                
                TempData["Success"] = "Announcement created successfully!";
                return RedirectToAction("Index");
            }

            return View(announcement);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var announcement = Context.Announcements
                .FirstOrDefault(a => a.Id == id && a.KindergartenId == CurrentUser.KindergartenId);

            if (announcement == null)
            {
                return HttpNotFound();
            }

            return View(announcement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Announcement announcement)
        {
            if (ModelState.IsValid)
            {
                var existingAnnouncement = Context.Announcements
                    .FirstOrDefault(a => a.Id == announcement.Id && a.KindergartenId == CurrentUser.KindergartenId);

                if (existingAnnouncement != null)
                {
                    existingAnnouncement.Title = announcement.Title;
                    existingAnnouncement.Description = announcement.Description;
                    existingAnnouncement.AnnouncementDate = announcement.AnnouncementDate;
                    existingAnnouncement.IsActive = announcement.IsActive;

                    Context.SaveChanges();
                    TempData["Success"] = "Announcement updated successfully!";
                    return RedirectToAction("Index");
                }
            }

            return View(announcement);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var announcement = Context.Announcements
                .FirstOrDefault(a => a.Id == id && a.KindergartenId == CurrentUser.KindergartenId);

            if (announcement == null)
            {
                return HttpNotFound();
            }

            return View(announcement);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var announcement = Context.Announcements
                .FirstOrDefault(a => a.Id == id && a.KindergartenId == CurrentUser.KindergartenId);

            if (announcement != null)
            {
                Context.Announcements.Remove(announcement);
                Context.SaveChanges();
                TempData["Success"] = "Announcement deleted successfully!";
            }

            return RedirectToAction("Index");
        }
    }
}