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
            // Use existing database file from App_Data - disable auto initialization
            Database.SetInitializer<KindergartenContext>(null);
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

            // Configure one-to-many relationship for GeneralSettings (not one-to-one)
            modelBuilder.Entity<GeneralSettings>()
                .HasRequired(s => s.Kindergarten)
                .WithMany()
                .HasForeignKey(s => s.KindergartenId)
                .WillCascadeOnDelete(false);

            // Configure one-to-many relationship for MissionVision (not one-to-one)
            modelBuilder.Entity<MissionVision>()
                .HasRequired(m => m.Kindergarten)
                .WithMany()
                .HasForeignKey(m => m.KindergartenId)
                .WillCascadeOnDelete(false);

            // Configure one-to-many relationship for AboutUsContent (not one-to-one)
            modelBuilder.Entity<AboutUsContent>()
                .HasRequired(a => a.Kindergarten)
                .WithMany()
                .HasForeignKey(a => a.KindergartenId)
                .WillCascadeOnDelete(false);

            // Configure other relationships
            modelBuilder.Entity<User>()
                .HasRequired(u => u.Kindergarten)
                .WithMany(k => k.Users)
                .HasForeignKey(u => u.KindergartenId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CoreEducationProgram>()
                .HasRequired(c => c.Kindergarten)
                .WithMany(k => k.CoreEducationPrograms)
                .HasForeignKey(c => c.KindergartenId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ParentTestimonial>()
                .HasRequired(p => p.Kindergarten)
                .WithMany(k => k.ParentTestimonials)
                .HasForeignKey(p => p.KindergartenId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Event>()
                .HasRequired(e => e.Kindergarten)
                .WithMany(k => k.Events)
                .HasForeignKey(e => e.KindergartenId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Staff>()
                .HasRequired(s => s.Kindergarten)
                .WithMany(k => k.StaffMembers)
                .HasForeignKey(s => s.KindergartenId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Announcement>()
                .HasRequired(a => a.Kindergarten)
                .WithMany(k => k.Announcements)
                .HasForeignKey(a => a.KindergartenId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Gallery>()
                .HasRequired(g => g.Kindergarten)
                .WithMany(k => k.GalleryImages)  
                .HasForeignKey(g => g.KindergartenId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ContactSubmission>()
                .HasRequired(c => c.Kindergarten)
                .WithMany(k => k.ContactSubmissions)
                .HasForeignKey(c => c.KindergartenId)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}