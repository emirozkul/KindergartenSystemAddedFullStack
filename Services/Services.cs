using KindergartenSystem.Models;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KindergartenSystem.Services
{
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