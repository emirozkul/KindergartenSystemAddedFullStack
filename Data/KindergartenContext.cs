using KindergartenSystem.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace KindergartenSystem.Data
{
    public class KindergartenContext : DbContext
    {
        public KindergartenContext() : base("KindergartenDB")
        {
        }

        public DbSet<Kindergarten> Kindergartens { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<GeneralSettings> GeneralSettings { get; set; }
        public DbSet<MissionVision> MissionVisions { get; set; }
        public DbSet<CoreEducationProgram> CoreEducationPrograms { get; set; }
        public DbSet<ParentTestimonial> ParentTestimonials { get; set; }
        public DbSet<AboutUsContent> AboutUsContents { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Staff> StaffMembers { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Gallery> GalleryImages { get; set; }
        public DbSet<ContactSubmission> ContactSubmissions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // Configure one-to-one relationship for GeneralSettings
            modelBuilder.Entity<Kindergarten>()
                .HasOptional(k => k.GeneralSettings)
                .WithRequired(s => s.Kindergarten);

            // Configure one-to-one relationship for MissionVision
            modelBuilder.Entity<Kindergarten>()
                .HasOptional(k => k.MissionVision)
                .WithRequired(m => m.Kindergarten);

            // Configure one-to-one relationship for AboutUsContent
            modelBuilder.Entity<Kindergarten>()
                .HasOptional(k => k.AboutUsContent)
                .WithRequired(a => a.Kindergarten);

            base.OnModelCreating(modelBuilder);
        }
    }
}