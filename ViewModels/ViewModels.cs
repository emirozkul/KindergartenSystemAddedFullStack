using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using KindergartenSystem.Models;

namespace KindergartenSystem.ViewModels
{
    public class HomeViewModel
    {
        public GeneralSettings Settings { get; set; }
        public MissionVision MissionVision { get; set; }
        public List<CoreEducationProgram> CorePrograms { get; set; }
        public List<ParentTestimonial> Testimonials { get; set; }
    }

    public class AboutViewModel
    {
        public AboutUsContent AboutUs { get; set; }
        public MissionVision MissionVision { get; set; }
    }

    public class ContactFormViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone")]
        public string Phone { get; set; }

        [Required]
        [Display(Name = "Subject")]
        public string Subject { get; set; }

        [Required]
        [Display(Name = "Message")]
        public string Message { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }

        public int KindergartenId { get; set; }
    }

    public class DashboardViewModel
    {
        public int TotalEvents { get; set; }
        public int TotalAnnouncements { get; set; }
        public int TotalStaff { get; set; }
        public int UnreadMessages { get; set; }
        public List<ContactSubmission> RecentSubmissions { get; set; }
    }
}