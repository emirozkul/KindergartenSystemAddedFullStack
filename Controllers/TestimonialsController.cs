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
    [KindergartenAuthorize("SuperAdmin", "KreşAdmin")]
    public class TestimonialsController : AdminBaseController
    {
        public ActionResult Index()
        {
            var testimonials = Context.ParentTestimonials
                .Where(t => t.KindergartenId == CurrentUser.KindergartenId)
                .OrderByDescending(t => t.CreatedDate)
                .ToList();

            return View(testimonials);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var testimonial = Context.ParentTestimonials
                .FirstOrDefault(t => t.Id == id && t.KindergartenId == CurrentUser.KindergartenId);

            if (testimonial == null)
            {
                return HttpNotFound();
            }

            return View(testimonial);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ParentTestimonial testimonial, HttpPostedFileBase photoFile)
        {
            if (ModelState.IsValid)
            {
                testimonial.KindergartenId = CurrentUser.KindergartenId;
                testimonial.CreatedDate = DateTime.Now;

                // Handle photo upload
                if (photoFile != null && photoFile.ContentLength > 0)
                {
                    testimonial.ParentPhoto = SaveFile(photoFile, "testimonials");
                }

                Context.ParentTestimonials.Add(testimonial);
                Context.SaveChanges();
                
                TempData["Success"] = "Testimonial created successfully!";
                return RedirectToAction("Index");
            }

            return View(testimonial);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var testimonial = Context.ParentTestimonials
                .FirstOrDefault(t => t.Id == id && t.KindergartenId == CurrentUser.KindergartenId);

            if (testimonial == null)
            {
                return HttpNotFound();
            }

            return View(testimonial);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ParentTestimonial testimonial, HttpPostedFileBase photoFile)
        {
            if (ModelState.IsValid)
            {
                var existingTestimonial = Context.ParentTestimonials
                    .FirstOrDefault(t => t.Id == testimonial.Id && t.KindergartenId == CurrentUser.KindergartenId);

                if (existingTestimonial != null)
                {
                    existingTestimonial.FirstName = testimonial.FirstName;
                    existingTestimonial.LastName = testimonial.LastName;
                    existingTestimonial.Rating = testimonial.Rating;
                    existingTestimonial.Comment = testimonial.Comment;
                    existingTestimonial.IsActive = testimonial.IsActive;

                    // Handle photo upload
                    if (photoFile != null && photoFile.ContentLength > 0)
                    {
                        existingTestimonial.ParentPhoto = SaveFile(photoFile, "testimonials");
                    }

                    Context.SaveChanges();
                    TempData["Success"] = "Testimonial updated successfully!";
                    return RedirectToAction("Index");
                }
            }

            return View(testimonial);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var testimonial = Context.ParentTestimonials
                .FirstOrDefault(t => t.Id == id && t.KindergartenId == CurrentUser.KindergartenId);

            if (testimonial == null)
            {
                return HttpNotFound();
            }

            return View(testimonial);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var testimonial = Context.ParentTestimonials
                .FirstOrDefault(t => t.Id == id && t.KindergartenId == CurrentUser.KindergartenId);

            if (testimonial != null)
            {
                Context.ParentTestimonials.Remove(testimonial);
                Context.SaveChanges();
                TempData["Success"] = "Testimonial deleted successfully!";
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