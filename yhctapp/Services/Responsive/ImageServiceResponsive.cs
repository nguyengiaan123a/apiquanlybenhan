using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using yhctapp.Services.Interface;

public class ImageServiceResponsive : IImageService
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    // Tiêm IWebHostEnvironment vào để lấy đường dẫn wwwroot
    public ImageServiceResponsive(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<string> UploadImageAsync(IFormFile file)
    {
        // Kiểm tra file có hợp lệ không
        if (file == null || file.Length == 0)
        {
            return null;
        }

        // 1. Lấy đường dẫn vật lý tới thư mục wwwroot/Uploads
        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads");

        // 2. Kiểm tra và tạo thư mục nếu nó chưa tồn tại
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        // 3. Tạo tên file độc nhất để không bị ghi đè file cũ (Dùng GUID)
        string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        // 4. Lưu file xuống ổ cứng
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }
        // 5. Trả về đường dẫn ảo để lưu vào Database (ví dụ: /Uploads/abc.jpg)
        return uniqueFileName;
    }

    public void DeleteImage(string imagePath)
    {
        // Kiểm tra xem đường dẫn có rỗng hoặc là ký tự mặc định không
        if (string.IsNullOrEmpty(imagePath) || imagePath == "#")
        {
            return;
        }

        // 1. Bỏ dấu '/' ở đầu chuỗi (nếu có) để khi dùng Path.Combine không bị lỗi
        string relativePath = imagePath.TrimStart('/');

        // 2. Ghép với thư mục wwwroot để ra đường dẫn vật lý trên ổ cứng
        // Ví dụ: C:\...\wwwroot\Uploads\abc.jpg
        string physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads", relativePath);
        // 3. Nếu file tồn tại thì tiến hành xoá
        if (File.Exists(physicalPath))
        {
            try
            {
                File.Delete(physicalPath);
            }
            catch (Exception ex)
            {
                // Có thể log lỗi ở đây nếu cần (ví dụ: file đang bị mở bởi tiến trình khác)
                Console.WriteLine($"Lỗi khi xoá ảnh: {ex.Message}");
            }
        }
    }
}