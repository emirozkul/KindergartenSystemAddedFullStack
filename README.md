# Kindergarten System

Bu proje, kreş/anaokulu websiteleri için geliştirilmiş bir yönetim sistemidir. ASP.NET MVC 5 (.NET Framework 4.7.2) kullanılarak geliştirilmiştir.

## Özellikler

- Multi-tenant yapı (Alt domain desteği)
- Responsive tasarım
- Admin paneli
- İletişim formu
- Etkinlik yönetimi
- Duyuru sistemi
- Personel tanıtımı
- Galeri yönetimi
- E-posta bildirimleri

## Gereksinimler

- .NET Framework 4.7.2
- SQL Server LocalDB
- Visual Studio 2019 veya üstü
- IIS Express

## Kurulum

### Visual Studio ile:
1. Visual Studio 2019/2022 ile KindergartenSystem.sln dosyasını açın
2. NuGet paketlerini geri yükleyin (Restore NuGet Packages)
3. Projeyi Build edin (Build Solution)
4. F5 ile çalıştırın

### Komut satırı ile:
1. `run.bat` dosyasını çift tıklayın veya
2. Command Prompt'ta proje klasöründe `run.bat` komutunu çalıştırın

## Veritabanı

Proje mevcut bir SQL Server veritabanı dosyasını (`App_Data\KindergartenDB.mdf`) kullanır. Veritabanı dosyası projeye dahil edilmiştir ve herhangi bir makinede çalışabilir. İlk çalıştırmada LocalDB otomatik olarak veritabanı dosyasını attach eder.

## Varsayılan Giriş Bilgileri

- Email: admin@ornek.com
- Şifre: admin123

## Test

### Kolay Başlatma (Önerilen):
```cmd
start.bat
```

### Test URL'leri:
- **Bağlantı Testi**: `http://localhost:3000/Test`
- **Debug Bilgileri**: `http://localhost:3000/Debug`  
- **Ana Uygulama**: `http://localhost:3000/?subdomain=ornek`

**ÖNEMLİ NOT**: Localhost'ta ana sayfaya erişmek için mutlaka `?subdomain=ornek` parametresini kullanın. Parametre olmadan erişim denerseniz otomatik olarak doğru URL'ye yönlendirileceksiniz.

### Manuel Çalıştırma:
```cmd
run.bat
```

## Sorun Giderme

### "localhost bağlanmayı reddetti" Hatası:

1. **start.bat kullanın** - Otomatik port tespiti yapar
2. **Yönetici olarak çalıştırın** - Command Prompt'u "Run as Administrator" ile açın
3. **Windows Firewall kontrolü**:
   - Windows Security > Firewall & network protection
   - "Allow an app through firewall" > IIS Express'i ekleyin
4. **Antivirus kontrolü** - Geçici olarak devre dışı bırakın
5. **Port kontrolü**:
   ```cmd
   netstat -an | findstr :3000
   ```

### Alternatif Çözümler:
- Visual Studio 2022'de projeyi açıp F5'e basın
- IIS Express'i manuel port ile çalıştırın:
  ```cmd
  "C:\Program Files\IIS Express\iisexpress.exe" /path:"C:\path\to\project" /port:3000
  ```

## Yapılandırma

Web.config dosyasında email ve diğer ayarları yapılandırabilirsiniz.