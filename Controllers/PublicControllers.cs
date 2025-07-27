using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using KindergartenSystem.Auth;
using KindergartenSystem.Data;
using KindergartenSystem.Models;
using KindergartenSystem.Services;

namespace KindergartenSystem.Controllers
{
    public class HomeController : PublicBaseController
    {
        private readonly IWhatsAppService _whatsAppService;

        public HomeController()
        {
            _whatsAppService = new WhatsAppService();
        }

        public ActionResult Index()
        {
            if (CurrentKindergarten == null)
                return HttpNotFound();

            var viewModel = new HomeViewModel
            {
                Settings = Context.GeneralSettings.FirstOrDefault(s => s.KindergartenId == CurrentKindergarten.Id),
                MissionVision = Context.MissionVisions.FirstOrDefault(m => m.KindergartenId == CurrentKindergarten.Id),
                CorePrograms = Context.CoreEducationPrograms
                    .Where(p => p.KindergartenId == CurrentKindergarten.Id && p.IsActive)
                    .OrderBy(p => p.DisplayOrder)
                    .ToList(),
                Testimonials = Context.ParentTestimonials
                    .Where(t => t.KindergartenId == CurrentKindergarten.Id && t.IsActive)
                    .OrderByDescending(t => t.CreatedDate)
                    .Take(6)
                    .ToList()
            };

            return View(viewModel);
        }

        public ActionResult About()
        {
            if (CurrentKindergarten == null)
                return HttpNotFound();

            var aboutUs = Context.AboutUsContents
                .FirstOrDefault(a => a.KindergartenId == CurrentKindergarten.Id);

            var viewModel = new AboutViewModel
            {
                AboutUs = aboutUs,
                MissionVision = Context.MissionVisions.FirstOrDefault(m => m.KindergartenId == CurrentKindergarten.Id)
            };

            return View(viewModel);
        }

        public ActionResult Events()
        {
            if (CurrentKindergarten == null)
                return HttpNotFound();

            var events = Context.Events
                .Where(e => e.KindergartenId == CurrentKindergarten.Id && e.IsActive)
                .OrderByDescending(e => e.EventDate)
                .ToList();

            return View(events);
        }

        public ActionResult EventDetail(int id)
        {
            if (CurrentKindergarten == null)
                return HttpNotFound();

            var eventItem = Context.Events
                .FirstOrDefault(e => e.Id == id && e.KindergartenId == CurrentKindergarten.Id && e.IsActive);

            if (eventItem == null)
                return HttpNotFound();

            return View(eventItem);
        }

        public ActionResult Staff()
        {
            if (CurrentKindergarten == null)
                return HttpNotFound();

            var staff = Context.StaffMembers
                .Where(s => s.KindergartenId == CurrentKindergarten.Id && s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .ToList();

            return View(staff);
        }

        public ActionResult Announcements()
        {
            if (CurrentKindergarten == null)
                return HttpNotFound();

            var announcements = Context.Announcements
                .Where(a => a.KindergartenId == CurrentKindergarten.Id && a.IsActive)
                .OrderByDescending(a => a.AnnouncementDate)
                .ToList();

            return View(announcements);
        }

        public ActionResult AnnouncementDetail(int id)
        {
            if (CurrentKindergarten == null)
                return HttpNotFound();

            var announcement = Context.Announcements
                .FirstOrDefault(a => a.Id == id && a.KindergartenId == CurrentKindergarten.Id && a.IsActive);

            if (announcement == null)
                return HttpNotFound();

            return View(announcement);
        }

        public ActionResult Gallery()
        {
            if (CurrentKindergarten == null)
                return HttpNotFound();

            var images = Context.GalleryImages
                .Where(g => g.KindergartenId == CurrentKindergarten.Id && g.IsActive)
                .OrderBy(g => g.DisplayOrder)
                .ToList();

            return View(images);
        }

        public ActionResult Contact()
        {
            if (CurrentKindergarten == null)
                return HttpNotFound();

            var settings = Context.GeneralSettings
                .FirstOrDefault(s => s.KindergartenId == CurrentKindergarten.Id);

            ViewBag.Settings = settings;
            return View(new ContactFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Contact(ContactFormViewModel model, HttpPostedFileBase file)
        {
            if (CurrentKindergarten == null)
                return HttpNotFound();

            if (ModelState.IsValid)
            {
                var submission = new ContactSubmission
                {
                    KindergartenId = CurrentKindergarten.Id,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Phone = model.Phone,
                    Subject = model.Subject,
                    Message = model.Message
                };

                // Handle file upload
                if (file != null && file.ContentLength > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx" };
                    var extension = Path.GetExtension(file.FileName).ToLower();

                    if (allowedExtensions.Contains(extension))
                    {
                        var fileName = Guid.NewGuid() + extension;
                        var uploadPath = Path.Combine(Server.MapPath($"~/Content/uploads/{CurrentKindergarten.Id}/contacts"), fileName);

                        Directory.CreateDirectory(Path.GetDirectoryName(uploadPath));
                        file.SaveAs(uploadPath);

                        submission.FilePath = $"/Content/uploads/{CurrentKindergarten.Id}/contacts/{fileName}";
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid file type. Allowed: JPG, PNG, PDF, DOC, DOCX");
                        return View(model);
                    }
                }

                Context.ContactSubmissions.Add(submission);
                Context.SaveChanges();

                // Send notifications
                await SendNotificationsAsync(submission);

                TempData["Success"] = "Mesajınız başarıyla gönderildi! En kısa sürede sizinle iletişime geçeceğiz.";
                return RedirectToAction("Contact");
            }

            var settings = Context.GeneralSettings
                .FirstOrDefault(s => s.KindergartenId == CurrentKindergarten.Id);
            ViewBag.Settings = settings;

            return View(model);
        }

        private async Task SendNotificationsAsync(ContactSubmission submission)
        {
            try
            {
                var settings = Context.GeneralSettings
                    .FirstOrDefault(s => s.KindergartenId == submission.KindergartenId);

                if (settings != null)
                {
                    // Send email notification to the new address
                    await SendEmailNotificationAsync(submission, "heroner2000@gmail.com");

                    // Send email notification to the admin's configured email
                    if (!string.IsNullOrEmpty(settings.Email))
                    {
                        await SendEmailNotificationAsync(submission, settings.Email);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the submission
                System.Diagnostics.Debug.WriteLine($"Failed to send notifications: {ex.Message}");
            }
        }

        private async Task SendEmailNotificationAsync(ContactSubmission submission, string toEmail)
        {
            try
            {
                var subject = $"Yeni İletişim Formu Mesajı: {submission.Subject}";
                var body = $@"
                    <h3>Yeni İletişim Formu Mesajı</h3>
                    <p><strong>Gönderen:</strong> {submission.FirstName} {submission.LastName}</p>
                    <p><strong>E-posta:</strong> {submission.Email}</p>
                    <p><strong>Telefon:</strong> {submission.Phone}</p>
                    <p><strong>Konu:</strong> {submission.Subject}</p>
                    <p><strong>Mesaj:</strong></p>
                    <div style='background: #f5f5f5; padding: 15px; border-radius: 5px;'>
                        {submission.Message.Replace(Environment.NewLine, "<br/>")}
                    </div>
                    <p><strong>Gönderim Tarihi:</strong> {submission.SubmittedDate:dd.MM.yyyy HH:mm}</p>
                    {(string.IsNullOrEmpty(submission.FilePath) ? "" : $"<p><strong>Ek Dosya:</strong> <a href='{Request.Url.Scheme}://{Request.Url.Authority}{submission.FilePath}'>Dosyayı İndir</a></p>")}
                ";

                using (var client = new SmtpClient())
                {
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("noreply@kindergarten.com", CurrentKindergarten.Name),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(toEmail);
                    mailMessage.ReplyToList.Add(new MailAddress(submission.Email, $"{submission.FirstName} {submission.LastName}"));

                    await client.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Email send failed: {ex.Message}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // _whatsAppService is not disposable from its interface
            }
            base.Dispose(disposing);
        }
    }

    // Account Controller for Login
    public class AccountController : Controller
    {
        private readonly IAuthenticationService _authService;
        private readonly KindergartenContext _context;

        public AccountController()
        {
            _context = new KindergartenContext();
            _authService = new AuthenticationService(_context);
        }

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            // Determine kindergarten from subdomain
            var subdomain = GetSubdomain();
            Kindergarten kindergarten = null;

            if (!string.IsNullOrEmpty(subdomain))
            {
                kindergarten = _context.Kindergartens
                    .FirstOrDefault(k => k.Subdomain == subdomain && k.IsActive);
            }

            if (kindergarten == null && subdomain != "admin")
            {
                // If no kindergarten found and not admin subdomain, show selection
                ViewBag.Kindergartens = new SelectList(_context.Kindergartens.Where(k => k.IsActive).ToList(), "Id", "Name");
                return View("SelectKindergarten");
            }

            ViewBag.KindergartenId = kindergarten?.Id ?? 1; // 1 is system admin
            ViewBag.KindergartenName = kindergarten?.Name ?? "System Admin";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = _authService.ValidateUser(model.Email, model.Password, model.KindergartenId);

                if (user != null)
                {
                    _authService.SignIn(user, model.RememberMe);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }

                ModelState.AddModelError("", "Invalid email or password.");
            }

            ViewBag.KindergartenId = model.KindergartenId;
            var kindergarten = _context.Kindergartens.Find(model.KindergartenId);
            ViewBag.KindergartenName = kindergarten?.Name ?? "System Admin";

            return View(model);
        }

        public ActionResult Logout()
        {
            _authService.SignOut();
            return RedirectToAction("Login");
        }

        private string GetSubdomain()
        {
            var host = Request.Url.Host;
            var parts = host.Split('.');

            if (parts.Length < 3 || host.Contains("localhost") || System.Net.IPAddress.TryParse(host, out _))
            {
                return Request.QueryString["subdomain"];
            }

            return parts[0];
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}