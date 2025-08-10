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
    public class ContactController : Controller
    {
        // Public contact form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Submit(ContactSubmission contact, HttpPostedFileBase attachmentFile)
        {
            if (ModelState.IsValid)
            {
                // Get kindergarten from subdomain or session
                var kindergartenId = GetKindergartenIdFromContext();
                
                if (kindergartenId > 0)
                {
                    contact.KindergartenId = kindergartenId;
                    contact.SubmittedDate = DateTime.Now;

                    // Handle file attachment
                    if (attachmentFile != null && attachmentFile.ContentLength > 0)
                    {
                        contact.FilePath = SaveFile(attachmentFile, kindergartenId);
                    }

                    using (var context = new Data.KindergartenContext())
                    {
                        context.ContactSubmissions.Add(contact);
                        context.SaveChanges();
                    }

                    TempData["Success"] = "Your message has been sent successfully! We will get back to you soon.";
                }
                else
                {
                    TempData["Error"] = "Unable to submit form. Please try again.";
                }
            }
            else
            {
                TempData["Error"] = "Please fill all required fields correctly.";
            }

            return RedirectToAction("Contact", "Home");
        }

        private int GetKindergartenIdFromContext()
        {
            // Try to get from subdomain
            var host = Request.Url.Host;
            var parts = host.Split('.');

            string subdomain = null;
            if (parts.Length < 3 || host.Contains("localhost") || System.Net.IPAddress.TryParse(host, out _))
            {
                subdomain = Request.QueryString["subdomain"];
            }
            else
            {
                subdomain = parts[0];
            }

            if (!string.IsNullOrEmpty(subdomain))
            {
                using (var context = new Data.KindergartenContext())
                {
                    var kindergarten = context.Kindergartens
                        .FirstOrDefault(k => k.Subdomain == subdomain && k.IsActive);
                    
                    return kindergarten?.Id ?? 0;
                }
            }

            return 0;
        }

        private string SaveFile(HttpPostedFileBase file, int kindergartenId)
        {
            if (file == null || file.ContentLength == 0)
                return null;

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var kindergartenFolder = Path.Combine(Server.MapPath("~/Content/uploads"), kindergartenId.ToString(), "contact");
            
            if (!Directory.Exists(kindergartenFolder))
            {
                Directory.CreateDirectory(kindergartenFolder);
            }

            var filePath = Path.Combine(kindergartenFolder, fileName);
            file.SaveAs(filePath);

            return $"/Content/uploads/{kindergartenId}/contact/{fileName}";
        }
    }

    // Admin contact management
    [KindergartenAuthorize("SuperAdmin", "KreşAdmin")]
    public class MessagesController : AdminBaseController
    {
        public ActionResult Index()
        {
            var messages = Context.ContactSubmissions
                .Where(c => c.KindergartenId == CurrentUser.KindergartenId)
                .OrderByDescending(c => c.SubmittedDate)
                .ToList();

            return View(messages);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var message = Context.ContactSubmissions
                .FirstOrDefault(c => c.Id == id && c.KindergartenId == CurrentUser.KindergartenId);

            if (message == null)
            {
                return HttpNotFound();
            }

            // Mark as read
            if (!message.IsRead)
            {
                message.IsRead = true;
                Context.SaveChanges();
            }

            return View(message);
        }

        [HttpPost]
        public ActionResult MarkAsRead(int id)
        {
            var message = Context.ContactSubmissions
                .FirstOrDefault(c => c.Id == id && c.KindergartenId == CurrentUser.KindergartenId);

            if (message != null)
            {
                message.IsRead = true;
                Context.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult MarkAsUnread(int id)
        {
            var message = Context.ContactSubmissions
                .FirstOrDefault(c => c.Id == id && c.KindergartenId == CurrentUser.KindergartenId);

            if (message != null)
            {
                message.IsRead = false;
                Context.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var message = Context.ContactSubmissions
                .FirstOrDefault(c => c.Id == id && c.KindergartenId == CurrentUser.KindergartenId);

            if (message == null)
            {
                return HttpNotFound();
            }

            return View(message);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var message = Context.ContactSubmissions
                .FirstOrDefault(c => c.Id == id && c.KindergartenId == CurrentUser.KindergartenId);

            if (message != null)
            {
                Context.ContactSubmissions.Remove(message);
                Context.SaveChanges();
                TempData["Success"] = "Message deleted successfully!";
            }

            return RedirectToAction("Index");
        }

        public ActionResult GetUnreadCount()
        {
            var count = Context.ContactSubmissions
                .Count(c => c.KindergartenId == CurrentUser.KindergartenId && !c.IsRead);

            return Json(new { count = count }, JsonRequestBehavior.AllowGet);
        }
    }
}