using System;
using System.Data.Entity;
using System.Linq;
using KindergartenSystem.Models;
using KindergartenSystem.Auth;

namespace KindergartenSystem.Data
{
    public class DatabaseInitializer : DropCreateDatabaseIfModelChanges<KindergartenContext>
    {
        protected override void Seed(KindergartenContext context)
        {
            System.Diagnostics.Debug.WriteLine("=== DatabaseInitializer.Seed() started - checking existing data ===");
            
            // Check if data already exists
            if (context.Users.Any() || context.Kindergartens.Any())
            {
                System.Diagnostics.Debug.WriteLine("Database already has data, skipping seed");
                return;
            }
            
            System.Diagnostics.Debug.WriteLine("Database is empty, seeding initial data...");
            
            // First, create SuperAdmin user without any kindergarten dependency
            var authService = new AuthenticationService(context);
            
            System.Diagnostics.Debug.WriteLine("Step 1: Creating SuperAdmin with KindergartenId = null");
            var superAdmin = new User
            {
                KindergartenId = null, // SuperAdmin is not tied to any specific kindergarten
                Username = "superadmin",
                Email = "superadmin@system.com",
                PasswordHash = authService.HashPassword("SuperAdmin123!"),
                Role = "SuperAdmin",
                IsActive = true,
                CreatedDate = DateTime.Now
            };
            
            System.Diagnostics.Debug.WriteLine($"SuperAdmin object created - KindergartenId: {superAdmin.KindergartenId}");
            context.Users.Add(superAdmin);
            System.Diagnostics.Debug.WriteLine("SuperAdmin added to context");
            
            try
            {
                System.Diagnostics.Debug.WriteLine("Step 2: Attempting to save SuperAdmin to database...");
                context.SaveChanges();
                System.Diagnostics.Debug.WriteLine("✅ SuperAdmin saved successfully!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error saving SuperAdmin: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Inner exception: {ex.InnerException.Message}");
                    if (ex.InnerException.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Inner-inner exception: {ex.InnerException.InnerException.Message}");
                    }
                }
                throw;
            }
            
            // Now create sample kindergartens
            System.Diagnostics.Debug.WriteLine("Step 3: Creating sample kindergartens");
            var kindergarten1 = new Kindergarten
            {
                Name = "Mutlu Çocuklar Kreşi",
                Subdomain = "mutlucocuklar", 
                IsActive = true,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };
            context.Kindergartens.Add(kindergarten1);

            var kindergarten2 = new Kindergarten
            {
                Name = "Neşeli Dünya Kreşi",
                Subdomain = "neselidunya", 
                IsActive = true,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };
            context.Kindergartens.Add(kindergarten2);
            context.SaveChanges();
            
            System.Diagnostics.Debug.WriteLine($"✅ Created kindergartens: {kindergarten1.Name}, {kindergarten2.Name}");

            // Create one test KreşAdmin for first kindergarten
            System.Diagnostics.Debug.WriteLine("Step 4: Creating test KreşAdmin");
            var kresAdmin1 = new User
            {
                KindergartenId = kindergarten1.Id,
                Username = "kresadmin1",
                Email = "admin@mutlucocuklar.com",
                PasswordHash = authService.HashPassword("KresAdmin123!"),
                Role = "KreşAdmin",
                IsActive = true,
                CreatedDate = DateTime.Now
            };
            context.Users.Add(kresAdmin1);
            context.SaveChanges();
            System.Diagnostics.Debug.WriteLine("✅ KreşAdmin created successfully");

            // Create basic settings for first kindergarten
            System.Diagnostics.Debug.WriteLine("Step 5: Creating basic settings");
            var settings1 = new GeneralSettings
            {
                KindergartenId = kindergarten1.Id,
                LogoPath = "/Content/images/logo.png",
                Slogan = "Mutlu çocuklar, güvenli gelecek",
                SubSlogan = "Sevgi dolu ortamımızda çocuklarınız güvenle büyüyor",
                Address = "Mutlu Mahallesi, Çocuk Sokağı No:5 İstanbul",
                Phone = "0212 555 11 11",
                Email = "info@mutlucocuklar.com",
                UpdatedDate = DateTime.Now
            };
            context.GeneralSettings.Add(settings1);
            
            // Create basic mission vision for first kindergarten
            var missionVision1 = new MissionVision
            {
                KindergartenId = kindergarten1.Id,
                MissionTitle = "Misyonumuz",
                MissionText = "Çocuklarımızın fiziksel, zihinsel, sosyal ve duygusal gelişimlerini destekleyen, onların yaratıcılıklarını ortaya çıkaran, mutlu ve güvenli bir ortam sağlamak.",
                VisionTitle = "Vizyonumuz", 
                VisionText = "Çocukları hayata hazırlayan, onların bireysel özelliklerini keşfetmelerine yardımcı olan, modern eğitim anlayışıyla hizmet veren öncü bir kurum olmak.",
                UpdatedDate = DateTime.Now
            };
            context.MissionVisions.Add(missionVision1);
            context.SaveChanges();

            System.Diagnostics.Debug.WriteLine("=== Database initialization completed successfully! ===");
            base.Seed(context);
        }
    }
}