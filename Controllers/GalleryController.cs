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
    public class GalleryController : AdminBaseController
    {
        public ActionResult Index()
        {
            var galleryImages = Context.GalleryImages
                .Where(g => g.KindergartenId == CurrentUser.KindergartenId)
                .OrderBy(g => g.DisplayOrder)
                .ToList();

            return View(galleryImages);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var galleryImage = Context.GalleryImages
                .FirstOrDefault(g => g.Id == id && g.KindergartenId == CurrentUser.KindergartenId);

            if (galleryImage == null)
            {
                return HttpNotFound();
            }

            return View(galleryImage);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Gallery gallery, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                gallery.KindergartenId = CurrentUser.KindergartenId;
                gallery.CreatedDate = DateTime.Now;

                // Set display order to be last
                var maxOrder = Context.GalleryImages
                    .Where(g => g.KindergartenId == CurrentUser.KindergartenId)
                    .Max(g => (int?)g.DisplayOrder) ?? 0;
                gallery.DisplayOrder = maxOrder + 1;

                // Handle image upload - required for gallery
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    gallery.ImagePath = SaveFile(imageFile, "gallery");
                    
                    Context.GalleryImages.Add(gallery);
                    Context.SaveChanges();
                    
                    TempData["Success"] = "Gallery image created successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Image file is required for gallery items.");
                }
            }

            return View(gallery);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var galleryImage = Context.GalleryImages
                .FirstOrDefault(g => g.Id == id && g.KindergartenId == CurrentUser.KindergartenId);

            if (galleryImage == null)
            {
                return HttpNotFound();
            }

            return View(galleryImage);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Gallery gallery, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                var existingGallery = Context.GalleryImages
                    .FirstOrDefault(g => g.Id == gallery.Id && g.KindergartenId == CurrentUser.KindergartenId);

                if (existingGallery != null)
                {
                    existingGallery.Title = gallery.Title;
                    existingGallery.DisplayOrder = gallery.DisplayOrder;
                    existingGallery.IsActive = gallery.IsActive;

                    // Handle image upload
                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        existingGallery.ImagePath = SaveFile(imageFile, "gallery");
                    }

                    Context.SaveChanges();
                    TempData["Success"] = "Gallery image updated successfully!";
                    return RedirectToAction("Index");
                }
            }

            return View(gallery);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var galleryImage = Context.GalleryImages
                .FirstOrDefault(g => g.Id == id && g.KindergartenId == CurrentUser.KindergartenId);

            if (galleryImage == null)
            {
                return HttpNotFound();
            }

            return View(galleryImage);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var galleryImage = Context.GalleryImages
                .FirstOrDefault(g => g.Id == id && g.KindergartenId == CurrentUser.KindergartenId);

            if (galleryImage != null)
            {
                Context.GalleryImages.Remove(galleryImage);
                Context.SaveChanges();
                TempData["Success"] = "Gallery image deleted successfully!";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase[] files)
        {
            var uploadedCount = 0;
            
            if (files != null && files.Length > 0)
            {
                var maxOrder = Context.GalleryImages
                    .Where(g => g.KindergartenId == CurrentUser.KindergartenId)
                    .Max(g => (int?)g.DisplayOrder) ?? 0;

                foreach (var file in files)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        var gallery = new Gallery
                        {
                            KindergartenId = CurrentUser.KindergartenId,
                            Title = Path.GetFileNameWithoutExtension(file.FileName),
                            ImagePath = SaveFile(file, "gallery"),
                            DisplayOrder = ++maxOrder,
                            IsActive = true,
                            CreatedDate = DateTime.Now
                        };

                        Context.GalleryImages.Add(gallery);
                        uploadedCount++;
                    }
                }

                Context.SaveChanges();
            }

            return Json(new { success = true, count = uploadedCount });
        }

        [HttpPost]
        public ActionResult UpdateOrder(int id, int newOrder)
        {
            var gallery = Context.GalleryImages
                .FirstOrDefault(g => g.Id == id && g.KindergartenId == CurrentUser.KindergartenId);

            if (gallery != null)
            {
                gallery.DisplayOrder = newOrder;
                Context.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
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