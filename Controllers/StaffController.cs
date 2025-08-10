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
    public class StaffController : AdminBaseController
    {
        public ActionResult Index()
        {
            var staff = Context.StaffMembers
                .Where(s => s.KindergartenId == CurrentUser.KindergartenId)
                .OrderBy(s => s.DisplayOrder)
                .ToList();

            return View(staff);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var staffMember = Context.StaffMembers
                .FirstOrDefault(s => s.Id == id && s.KindergartenId == CurrentUser.KindergartenId);

            if (staffMember == null)
            {
                return HttpNotFound();
            }

            return View(staffMember);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Staff staff, HttpPostedFileBase photoFile)
        {
            if (ModelState.IsValid)
            {
                staff.KindergartenId = CurrentUser.KindergartenId;
                staff.CreatedDate = DateTime.Now;

                // Set display order to be last
                var maxOrder = Context.StaffMembers
                    .Where(s => s.KindergartenId == CurrentUser.KindergartenId)
                    .Max(s => (int?)s.DisplayOrder) ?? 0;
                staff.DisplayOrder = maxOrder + 1;

                // Handle photo upload
                if (photoFile != null && photoFile.ContentLength > 0)
                {
                    staff.PhotoPath = SaveFile(photoFile, "staff");
                }

                Context.StaffMembers.Add(staff);
                Context.SaveChanges();
                
                TempData["Success"] = "Staff member created successfully!";
                return RedirectToAction("Index");
            }

            return View(staff);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var staffMember = Context.StaffMembers
                .FirstOrDefault(s => s.Id == id && s.KindergartenId == CurrentUser.KindergartenId);

            if (staffMember == null)
            {
                return HttpNotFound();
            }

            return View(staffMember);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Staff staff, HttpPostedFileBase photoFile)
        {
            if (ModelState.IsValid)
            {
                var existingStaff = Context.StaffMembers
                    .FirstOrDefault(s => s.Id == staff.Id && s.KindergartenId == CurrentUser.KindergartenId);

                if (existingStaff != null)
                {
                    existingStaff.FullName = staff.FullName;
                    existingStaff.Title = staff.Title;
                    existingStaff.Branch = staff.Branch;
                    existingStaff.Biography = staff.Biography;
                    existingStaff.DisplayOrder = staff.DisplayOrder;
                    existingStaff.IsActive = staff.IsActive;

                    // Handle photo upload
                    if (photoFile != null && photoFile.ContentLength > 0)
                    {
                        existingStaff.PhotoPath = SaveFile(photoFile, "staff");
                    }

                    Context.SaveChanges();
                    TempData["Success"] = "Staff member updated successfully!";
                    return RedirectToAction("Index");
                }
            }

            return View(staff);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var staffMember = Context.StaffMembers
                .FirstOrDefault(s => s.Id == id && s.KindergartenId == CurrentUser.KindergartenId);

            if (staffMember == null)
            {
                return HttpNotFound();
            }

            return View(staffMember);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var staffMember = Context.StaffMembers
                .FirstOrDefault(s => s.Id == id && s.KindergartenId == CurrentUser.KindergartenId);

            if (staffMember != null)
            {
                Context.StaffMembers.Remove(staffMember);
                Context.SaveChanges();
                TempData["Success"] = "Staff member deleted successfully!";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult UpdateOrder(int id, int newOrder)
        {
            var staff = Context.StaffMembers
                .FirstOrDefault(s => s.Id == id && s.KindergartenId == CurrentUser.KindergartenId);

            if (staff != null)
            {
                staff.DisplayOrder = newOrder;
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