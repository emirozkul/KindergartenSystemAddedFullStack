using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KindergartenSystem.Auth;
using KindergartenSystem.Models;

namespace KindergartenSystem.Controllers
{
    [KindergartenAuthorize("SuperAdmin", "KreşAdmin")]
    public class AboutController : AdminBaseController
    {
        public ActionResult Index()
        {
            var aboutUs = Context.AboutUsContents
                .FirstOrDefault(a => a.KindergartenId == CurrentUser.KindergartenId);

            if (aboutUs == null)
            {
                aboutUs = new AboutUsContent { KindergartenId = CurrentUser.KindergartenId };
                Context.AboutUsContents.Add(aboutUs);
                Context.SaveChanges();
            }

            return View(aboutUs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(AboutUsContent aboutUs, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                var existingAboutUs = Context.AboutUsContents
                    .FirstOrDefault(a => a.KindergartenId == CurrentUser.KindergartenId);

                if (existingAboutUs != null)
                {
                    existingAboutUs.Title = aboutUs.Title;
                    existingAboutUs.Description = aboutUs.Description;
                    existingAboutUs.UpdatedDate = DateTime.Now;

                    // Handle image upload
                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        existingAboutUs.ImagePath = SaveFile(imageFile, "about");
                    }

                    Context.SaveChanges();
                    TempData["Success"] = "About Us content updated successfully!";
                }
                else
                {
                    aboutUs.KindergartenId = CurrentUser.KindergartenId;
                    aboutUs.UpdatedDate = DateTime.Now;

                    // Handle image upload
                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        aboutUs.ImagePath = SaveFile(imageFile, "about");
                    }

                    Context.AboutUsContents.Add(aboutUs);
                    Context.SaveChanges();
                    TempData["Success"] = "About Us content created successfully!";
                }
            }

            return View(aboutUs);
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