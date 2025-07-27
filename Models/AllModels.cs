using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KindergartenSystem.Models
{
    public class Kindergarten
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string Subdomain { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<GeneralSettings> GeneralSettings { get; set; }
        public virtual ICollection<MissionVision> MissionVisions { get; set; }
        public virtual ICollection<AboutUsContent> AboutUsContents { get; set; }
        public virtual ICollection<CoreEducationProgram> CoreEducationPrograms { get; set; }
        public virtual ICollection<ParentTestimonial> ParentTestimonials { get; set; }
        public virtual ICollection<Event> Events { get; set; }
        public virtual ICollection<Staff> StaffMembers { get; set; }
        public virtual ICollection<Announcement> Announcements { get; set; }
        public virtual ICollection<Gallery> GalleryImages { get; set; }
        public virtual ICollection<ContactSubmission> ContactSubmissions { get; set; }
    }

    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int KindergartenId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; }

        [Required]
        [MaxLength(200)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } // SuperAdmin, KindergartenAdmin, Teacher

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ForeignKey("KindergartenId")]
        public virtual Kindergarten Kindergarten { get; set; }
    }

    public class GeneralSettings
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int KindergartenId { get; set; }

        [MaxLength(500)]
        public string LogoPath { get; set; }

        [MaxLength(500)]
        public string FooterLogoPath { get; set; }

        [MaxLength(500)]
        public string Slogan { get; set; }

        [MaxLength(500)]
        public string SubSlogan { get; set; }

        [MaxLength(500)]
        public string HeroBackgroundPath { get; set; }

        [MaxLength(500)]
        public string FooterSlogan { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }

        [MaxLength(50)]
        public string Phone { get; set; }

        [MaxLength(200)]
        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(500)]
        public string FacebookUrl { get; set; }

        [MaxLength(500)]
        public string TwitterUrl { get; set; }

        [MaxLength(500)]
        public string InstagramUrl { get; set; }

        public string GoogleMapEmbed { get; set; }

        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        [ForeignKey("KindergartenId")]
        public virtual Kindergarten Kindergarten { get; set; }
    }

    public class MissionVision
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int KindergartenId { get; set; }

        [MaxLength(200)]
        public string MissionTitle { get; set; }

        public string MissionText { get; set; }

        [MaxLength(200)]
        public string VisionTitle { get; set; }

        public string VisionText { get; set; }

        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        [ForeignKey("KindergartenId")]
        public virtual Kindergarten Kindergarten { get; set; }
    }

    public class CoreEducationProgram
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int KindergartenId { get; set; }

        [MaxLength(100)]
        public string Icon { get; set; } // Font Awesome class

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ForeignKey("KindergartenId")]
        public virtual Kindergarten Kindergarten { get; set; }
    }

    public class ParentTestimonial
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int KindergartenId { get; set; }

        [Required]
        [MaxLength(200)]
        public string ParentName { get; set; }

        [MaxLength(500)]
        public string ParentPhoto { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public string Comment { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ForeignKey("KindergartenId")]
        public virtual Kindergarten Kindergarten { get; set; }
    }

    public class AboutUsContent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int KindergartenId { get; set; }

        [MaxLength(500)]
        public string Title { get; set; }

        public string Description { get; set; }

        [MaxLength(500)]
        public string ImagePath { get; set; }

        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        [ForeignKey("KindergartenId")]
        public virtual Kindergarten Kindergarten { get; set; }
    }

    public class Event
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int KindergartenId { get; set; }

        [Required]
        [MaxLength(300)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string ShortDescription { get; set; }

        public string DetailedDescription { get; set; }

        [MaxLength(500)]
        public string ImagePath { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [MaxLength(50)]
        public string EventTime { get; set; }

        [MaxLength(300)]
        public string Location { get; set; }

        public string EventSchedule { get; set; } // Can store as JSON

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ForeignKey("KindergartenId")]
        public virtual Kindergarten Kindergarten { get; set; }
    }

    public class Staff
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int KindergartenId { get; set; }

        [Required]
        [MaxLength(200)]
        public string FullName { get; set; }

        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(200)]
        public string Branch { get; set; }

        public string Biography { get; set; }

        [MaxLength(500)]
        public string PhotoPath { get; set; }

        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ForeignKey("KindergartenId")]
        public virtual Kindergarten Kindergarten { get; set; }
    }

    public class Announcement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int KindergartenId { get; set; }

        [Required]
        [MaxLength(300)]
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime AnnouncementDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ForeignKey("KindergartenId")]
        public virtual Kindergarten Kindergarten { get; set; }
    }

    public class Gallery
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int KindergartenId { get; set; }

        [Required]
        [MaxLength(500)]
        public string ImagePath { get; set; }

        [MaxLength(200)]
        public string Title { get; set; }

        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ForeignKey("KindergartenId")]
        public virtual Kindergarten Kindergarten { get; set; }
    }

    public class ContactSubmission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int KindergartenId { get; set; }

        [MaxLength(100)]
        public string FirstName { get; set; }

        [MaxLength(100)]
        public string LastName { get; set; }

        [MaxLength(200)]
        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(50)]
        public string Phone { get; set; }

        [MaxLength(200)]
        public string Subject { get; set; }

        public string Message { get; set; }

        [MaxLength(500)]
        public string FilePath { get; set; }

        public bool IsRead { get; set; } = false;
        public DateTime SubmittedDate { get; set; } = DateTime.Now;

        [ForeignKey("KindergartenId")]
        public virtual Kindergarten Kindergarten { get; set; }
    }
}