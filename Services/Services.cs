using KindergartenSystem.Models;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace KindergartenSystem.Services
{
    public interface IImageService
    {
        string SaveImage(HttpPostedFileBase file, int kindergartenId, string folder, int maxWidth = 1200, int maxHeight = 800);
        string SaveIcon(HttpPostedFileBase file, int kindergartenId, string folder, int size = 64);
        bool DeleteImage(string path);
    }

    public class ImageService : IImageService
    {
        public string SaveImage(HttpPostedFileBase file, int kindergartenId, string folder, int maxWidth = 1200, int maxHeight = 800)
        {
            if (file == null || file.ContentLength == 0)
                return null;

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var uploadDir = HttpContext.Current.Server.MapPath($"~/Content/uploads/{kindergartenId}/{folder}/");
            
            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            var filePath = Path.Combine(uploadDir, fileName);

            // Resize and save image
            using (var image = Image.FromStream(file.InputStream))
            {
                var resized = ResizeImage(image, maxWidth, maxHeight);
                resized.Save(filePath, GetImageFormat(file.FileName));
                resized.Dispose();
            }

            return $"/Content/uploads/{kindergartenId}/{folder}/{fileName}";
        }

        public string SaveIcon(HttpPostedFileBase file, int kindergartenId, string folder, int size = 64)
        {
            if (file == null || file.ContentLength == 0)
                return null;

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var uploadDir = HttpContext.Current.Server.MapPath($"~/Content/uploads/{kindergartenId}/{folder}/");
            
            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            var filePath = Path.Combine(uploadDir, fileName);

            // Resize to square icon
            using (var image = Image.FromStream(file.InputStream))
            {
                var icon = ResizeToSquare(image, size);
                icon.Save(filePath, GetImageFormat(file.FileName));
                icon.Dispose();
            }

            return $"/Content/uploads/{kindergartenId}/{folder}/{fileName}";
        }

        public bool DeleteImage(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path))
                {
                    var fullPath = HttpContext.Current.Server.MapPath(path);
                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private Image ResizeImage(Image image, int maxWidth, int maxHeight)
        {
            var ratio = Math.Min((double)maxWidth / image.Width, (double)maxHeight / image.Height);
            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);
            using (var graphics = Graphics.FromImage(newImage))
            {
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            return newImage;
        }

        private Image ResizeToSquare(Image image, int size)
        {
            var newImage = new Bitmap(size, size);
            using (var graphics = Graphics.FromImage(newImage))
            {
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                
                // Center crop to square
                var sourceSize = Math.Min(image.Width, image.Height);
                var sourceX = (image.Width - sourceSize) / 2;
                var sourceY = (image.Height - sourceSize) / 2;
                
                graphics.DrawImage(image, new Rectangle(0, 0, size, size), 
                    new Rectangle(sourceX, sourceY, sourceSize, sourceSize), GraphicsUnit.Pixel);
            }

            return newImage;
        }

        private ImageFormat GetImageFormat(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();
            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    return ImageFormat.Jpeg;
                case ".png":
                    return ImageFormat.Png;
                case ".gif":
                    return ImageFormat.Gif;
                case ".bmp":
                    return ImageFormat.Bmp;
                default:
                    return ImageFormat.Jpeg;
            }
        }
    }

    public interface IWhatsAppService
    {
        Task<bool> SendContactFormNotificationAsync(ContactSubmission submission, string kindergartenPhone);
        Task<bool> SendAnnouncementNotificationAsync(string title, string content, string kindergartenPhone);
    }

    public class WhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;
        private readonly string _apiToken;

        public WhatsAppService()
        {
            _httpClient = new HttpClient();
            _apiUrl = ConfigurationManager.AppSettings["WhatsAppApiUrl"]; // e.g., WhatsApp Business API
            _apiToken = ConfigurationManager.AppSettings["WhatsAppApiToken"];
        }

        public async Task<bool> SendContactFormNotificationAsync(ContactSubmission submission, string kindergartenPhone)
        {
            try
            {
                var message = $"🏫 *Yeni İletişim Formu Mesajı*\n\n" +
                             $"👤 *Ad Soyad:* {submission.FirstName} {submission.LastName}\n" +
                             $"📧 *E-posta:* {submission.Email}\n" +
                             $"📞 *Telefon:* {submission.Phone}\n" +
                             $"📋 *Konu:* {submission.Subject}\n" +
                             $"💬 *Mesaj:* {submission.Message}\n" +
                             $"🕐 *Tarih:* {submission.SubmittedDate:dd.MM.yyyy HH:mm}";

                return await SendWhatsAppMessage(kindergartenPhone, message);
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"WhatsApp send error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendAnnouncementNotificationAsync(string title, string content, string kindergartenPhone)
        {
            try
            {
                var message = $"📢 *Yeni Duyuru Yayınlandı*\n\n" +
                             $"📝 *Başlık:* {title}\n" +
                             $"📄 *İçerik:* {content}\n" +
                             $"🕐 *Tarih:* {DateTime.Now:dd.MM.yyyy HH:mm}";

                return await SendWhatsAppMessage(kindergartenPhone, message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WhatsApp send error: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> SendWhatsAppMessage(string phoneNumber, string message)
        {
            if (string.IsNullOrEmpty(_apiUrl) || string.IsNullOrEmpty(_apiToken))
            {
                // Fallback: Generate WhatsApp web link
                var encodedMessage = Uri.EscapeDataString(message);
                var whatsappUrl = $"https://wa.me/{phoneNumber.Replace("+", "").Replace(" ", "")}?text={encodedMessage}";

                // Log the URL for manual sending or auto-open
                System.Diagnostics.Debug.WriteLine($"WhatsApp URL: {whatsappUrl}");
                return true;
            }

            try
            {
                var payload = new
                {
                    phone = phoneNumber.Replace("+", "").Replace(" ", ""),
                    message = message
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiToken);

                var response = await _httpClient.PostAsync(_apiUrl, content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}