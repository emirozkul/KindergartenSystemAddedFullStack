using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using KindergartenSystem.Auth;
using KindergartenSystem.Models;

namespace KindergartenSystem.Controllers
{
    [KindergartenAuthorize("SuperAdmin", "KreşAdmin")]
    public class ProgramsController : AdminBaseController
    {
        public ActionResult Index()
        {
            var programs = Context.CoreEducationPrograms
                .Where(p => p.KindergartenId == CurrentUser.KindergartenId)
                .OrderBy(p => p.DisplayOrder)
                .ToList();

            return View(programs);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var program = Context.CoreEducationPrograms
                .FirstOrDefault(p => p.Id == id && p.KindergartenId == CurrentUser.KindergartenId);

            if (program == null)
            {
                return HttpNotFound();
            }

            return View(program);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CoreEducationProgram program, HttpPostedFileBase iconFile)
        {
            if (ModelState.IsValid)
            {
                program.KindergartenId = CurrentUser.KindergartenId;
                program.CreatedDate = DateTime.Now;
                
                // Icon upload functionality removed - IconPath column not in database
                
                // Set display order to be last
                var maxOrder = Context.CoreEducationPrograms
                    .Where(p => p.KindergartenId == CurrentUser.KindergartenId)
                    .Max(p => (int?)p.DisplayOrder) ?? 0;
                program.DisplayOrder = maxOrder + 1;

                Context.CoreEducationPrograms.Add(program);
                Context.SaveChanges();
                
                TempData["Success"] = "Program created successfully!";
                return RedirectToAction("Index");
            }

            return View(program);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var program = Context.CoreEducationPrograms
                .FirstOrDefault(p => p.Id == id && p.KindergartenId == CurrentUser.KindergartenId);

            if (program == null)
            {
                return HttpNotFound();
            }

            return View(program);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CoreEducationProgram program, HttpPostedFileBase iconFile)
        {
            if (ModelState.IsValid)
            {
                var existingProgram = Context.CoreEducationPrograms
                    .FirstOrDefault(p => p.Id == program.Id && p.KindergartenId == CurrentUser.KindergartenId);

                if (existingProgram != null)
                {
                    // Handle icon upload
                    // Icon upload functionality removed - IconPath column not in database
                    
                    existingProgram.Title = program.Title;
                    existingProgram.Description = program.Description;
                    existingProgram.DisplayOrder = program.DisplayOrder;
                    existingProgram.IsActive = program.IsActive;

                    Context.SaveChanges();
                    TempData["Success"] = "Program updated successfully!";
                    return RedirectToAction("Index");
                }
            }

            return View(program);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var program = Context.CoreEducationPrograms
                .FirstOrDefault(p => p.Id == id && p.KindergartenId == CurrentUser.KindergartenId);

            if (program == null)
            {
                return HttpNotFound();
            }

            return View(program);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var program = Context.CoreEducationPrograms
                .FirstOrDefault(p => p.Id == id && p.KindergartenId == CurrentUser.KindergartenId);

            if (program != null)
            {
                Context.CoreEducationPrograms.Remove(program);
                Context.SaveChanges();
                TempData["Success"] = "Program deleted successfully!";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult UpdateOrder(int id, int newOrder)
        {
            var program = Context.CoreEducationPrograms
                .FirstOrDefault(p => p.Id == id && p.KindergartenId == CurrentUser.KindergartenId);

            if (program != null)
            {
                program.DisplayOrder = newOrder;
                Context.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }
    }
}