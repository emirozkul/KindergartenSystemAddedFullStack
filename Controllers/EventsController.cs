using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using KindergartenSystem.Auth;
using KindergartenSystem.Models;

namespace KindergartenSystem.Controllers
{
    [KindergartenAuthorize("SuperAdmin", "KindergartenAdmin")]
    public class EventsController : AdminBaseController
    {
        public ActionResult Index()
        {
            var events = Context.Events
                .Where(e => e.KindergartenId == CurrentUser.KindergartenId)
                .OrderByDescending(e => e.EventDate)
                .ToList();

            return View(events);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var eventItem = Context.Events
                .FirstOrDefault(e => e.Id == id && e.KindergartenId == CurrentUser.KindergartenId);

            if (eventItem == null)
            {
                return HttpNotFound();
            }

            return View(eventItem);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Event eventItem, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                eventItem.KindergartenId = CurrentUser.KindergartenId;
                eventItem.CreatedDate = DateTime.Now;

                // Handle image upload
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    eventItem.ImagePath = SaveFile(imageFile, "events");
                }

                Context.Events.Add(eventItem);
                Context.SaveChanges();
                
                TempData["Success"] = "Event created successfully!";
                return RedirectToAction("Index");
            }

            return View(eventItem);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var eventItem = Context.Events
                .FirstOrDefault(e => e.Id == id && e.KindergartenId == CurrentUser.KindergartenId);

            if (eventItem == null)
            {
                return HttpNotFound();
            }

            return View(eventItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Event eventItem, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                var existingEvent = Context.Events
                    .FirstOrDefault(e => e.Id == eventItem.Id && e.KindergartenId == CurrentUser.KindergartenId);

                if (existingEvent != null)
                {
                    existingEvent.Title = eventItem.Title;
                    existingEvent.ShortDescription = eventItem.ShortDescription;
                    existingEvent.DetailedDescription = eventItem.DetailedDescription;
                    existingEvent.EventDate = eventItem.EventDate;
                    existingEvent.EventTime = eventItem.EventTime;
                    existingEvent.Location = eventItem.Location;
                    existingEvent.EventSchedule = eventItem.EventSchedule;
                    existingEvent.IsActive = eventItem.IsActive;

                    // Handle image upload
                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        existingEvent.ImagePath = SaveFile(imageFile, "events");
                    }

                    Context.SaveChanges();
                    TempData["Success"] = "Event updated successfully!";
                    return RedirectToAction("Index");
                }
            }

            return View(eventItem);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var eventItem = Context.Events
                .FirstOrDefault(e => e.Id == id && e.KindergartenId == CurrentUser.KindergartenId);

            if (eventItem == null)
            {
                return HttpNotFound();
            }

            return View(eventItem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var eventItem = Context.Events
                .FirstOrDefault(e => e.Id == id && e.KindergartenId == CurrentUser.KindergartenId);

            if (eventItem != null)
            {
                Context.Events.Remove(eventItem);
                Context.SaveChanges();
                TempData["Success"] = "Event deleted successfully!";
            }

            return RedirectToAction("Index");
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