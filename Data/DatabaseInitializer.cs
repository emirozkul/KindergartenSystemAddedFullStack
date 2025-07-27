using System;
using System.Data.Entity;
using System.Linq;
using KindergartenSystem.Models;
using KindergartenSystem.Auth;

namespace KindergartenSystem.Data
{
    public class DatabaseInitializer : DropCreateDatabaseAlways<KindergartenContext>
    {
        protected override void Seed(KindergartenContext context)
        {
            System.Diagnostics.Debug.WriteLine("DatabaseInitializer.Seed() started - recreating database");
            
            // Create sample kindergarten
            var kindergarten = new Kindergarten
            {
                Name = "Örnek Kreş",
                Subdomain = "ornek", 
                IsActive = true,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };
            context.Kindergartens.Add(kindergarten);
            context.SaveChanges();
            
            System.Diagnostics.Debug.WriteLine($"Created kindergarten: {kindergarten.Name} with subdomain: {kindergarten.Subdomain}");

            // Create SuperAdmin user (should not be tied to any specific kindergarten)
            var authService = new AuthenticationService(context);
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
            context.Users.Add(superAdmin);
            
            System.Diagnostics.Debug.WriteLine($"Created SuperAdmin user: {superAdmin.Email} with password: SuperAdmin123!");

            // Create kindergarten admin user
            var adminUser = new User
            {
                KindergartenId = kindergarten.Id,
                Username = "admin",
                Email = "admin@ornek.com",
                PasswordHash = authService.HashPassword("admin123"),
                Role = "KindergartenAdmin",
                IsActive = true,
                CreatedDate = DateTime.Now
            };
            context.Users.Add(adminUser);
            
            // Create teacher user
            var teacherUser = new User
            {
                KindergartenId = kindergarten.Id,
                Username = "teacher",
                Email = "teacher@ornek.com",
                PasswordHash = authService.HashPassword("teacher123"),
                Role = "Teacher",
                IsActive = true,
                CreatedDate = DateTime.Now
            };
            context.Users.Add(teacherUser);

            // Create general settings
            var settings = new GeneralSettings
            {
                KindergartenId = kindergarten.Id,
                LogoPath = "/Content/images/logo.png",
                Slogan = "Çocuklarınızın mutlu geleceği için...",
                SubSlogan = "Güvenli, eğlenceli ve öğretici ortamda büyüyorlar",
                Address = "Örnek Mahallesi, Kreş Sokak No:1 İstanbul",
                Phone = "0212 555 01 01",
                Email = "info@ornekkreş.com",
                FacebookUrl = "https://facebook.com/ornekkreş",
                InstagramUrl = "https://instagram.com/ornekkreş",
                UpdatedDate = DateTime.Now
            };
            context.GeneralSettings.Add(settings);

            // Create mission vision
            var missionVision = new MissionVision
            {
                KindergartenId = kindergarten.Id,
                MissionTitle = "Misyonumuz",
                MissionText = "Çocuklarımızın fiziksel, zihinsel, sosyal ve duygusal gelişimlerini destekleyen, onların yaratıcılıklarını ortaya çıkaran, mutlu ve güvenli bir ortam sağlamak.",
                VisionTitle = "Vizyonumuz", 
                VisionText = "Çocukları hayata hazırlayan, onların bireysel özelliklerini keşfetmelerine yardımcı olan, modern eğitim anlayışıyla hizmet veren öncü bir kurum olmak.",
                UpdatedDate = DateTime.Now
            };
            context.MissionVisions.Add(missionVision);

            // Create sample core programs
            var programs = new[]
            {
                new CoreEducationProgram
                {
                    KindergartenId = kindergarten.Id,
                    Icon = "fas fa-palette",
                    Title = "Sanat ve Yaratıcılık",
                    Description = "Çocukların yaratıcı düşünme becerilerini geliştiren sanat etkinlikleri",
                    DisplayOrder = 1,
                    IsActive = true
                },
                new CoreEducationProgram
                {
                    KindergartenId = kindergarten.Id,
                    Icon = "fas fa-music",
                    Title = "Müzik ve Ritim",
                    Description = "Müzik eğitimi ile ritim duygusunu geliştiren aktiviteler",
                    DisplayOrder = 2,
                    IsActive = true
                },
                new CoreEducationProgram
                {
                    KindergartenId = kindergarten.Id,
                    Icon = "fas fa-running",
                    Title = "Fiziksel Gelişim",
                    Description = "Motor becerilerini geliştiren spor ve hareket etkinlikleri",
                    DisplayOrder = 3,
                    IsActive = true
                }
            };
            context.CoreEducationPrograms.AddRange(programs);

            // Create sample events
            var events = new[]
            {
                new Event
                {
                    KindergartenId = kindergarten.Id,
                    Title = "Yılsonu Gösterisi",
                    ShortDescription = "Çocuklarımızın yıl boyunca öğrendiklerini sergileyeceği muhteşem gösteri",
                    DetailedDescription = "Bu özel etkinlikte çocuklarımız dans, şarkı ve tiyatro performansları sergileyecekler. Velilerimizi de aramızda görmekten mutluluk duyacağız.",
                    EventDate = DateTime.Now.AddDays(30),
                    EventTime = "14:00-16:00",
                    Location = "Kreş Bahçesi",
                    IsActive = true
                },
                new Event
                {
                    KindergartenId = kindergarten.Id,
                    Title = "Doğa Yürüyüşü",
                    ShortDescription = "Çocuklarımızla birlikte doğayı keşfedeceğimiz eğlenceli yürüyüş",
                    DetailedDescription = "Yakındaki parka düzenleyeceğimiz yürüyüşte çocuklarımız doğayı gözlemleyecek ve çevre bilinci kazanacaklar.",
                    EventDate = DateTime.Now.AddDays(15),
                    EventTime = "10:00-12:00", 
                    Location = "Merkez Park",
                    IsActive = true
                }
            };
            context.Events.AddRange(events);

            // Create sample staff
            var staff = new[]
            {
                new Staff
                {
                    KindergartenId = kindergarten.Id,
                    FullName = "Ayşe Öğretmen",
                    Title = "Okul Öncesi Öğretmeni",
                    Branch = "4-5 Yaş Grubu",
                    Biography = "10 yıllık deneyime sahip, çocuk gelişimi uzmanı öğretmenimiz",
                    DisplayOrder = 1,
                    IsActive = true
                },
                new Staff
                {
                    KindergartenId = kindergarten.Id,
                    FullName = "Mehmet Öğretmen",
                    Title = "Beden Eğitimi Öğretmeni", 
                    Branch = "Spor Aktiviteleri",
                    Biography = "Çocuk sporları konusunda uzman, enerjik öğretmenimiz",
                    DisplayOrder = 2,
                    IsActive = true
                }
            };
            context.StaffMembers.AddRange(staff);

            // Create sample announcements
            var announcements = new[]
            {
                new Announcement
                {
                    KindergartenId = kindergarten.Id,
                    Title = "Yeni Dönem Kayıtları Başladı",
                    Description = "2024-2025 eğitim-öğretim yılı için kayıtlarımız başlamıştır. Detaylı bilgi için iletişime geçebilirsiniz.",
                    AnnouncementDate = DateTime.Now.AddDays(-5),
                    IsActive = true
                },
                new Announcement
                {
                    KindergartenId = kindergarten.Id,
                    Title = "Veli Toplantısı",
                    Description = "Ayın ilk haftasında gerçekleştirilecek veli toplantısına tüm velilerimizi bekliyoruz.",
                    AnnouncementDate = DateTime.Now.AddDays(-2),
                    IsActive = true
                }
            };
            context.Announcements.AddRange(announcements);

            context.SaveChanges();
            base.Seed(context);
        }
    }
}