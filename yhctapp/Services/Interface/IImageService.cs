namespace yhctapp.Services.Interface
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile file);

        // Hàm xóa ảnh: Dùng khi cập nhật ảnh mới hoặc xóa bản ghi
        void DeleteImage(string imagePath);
    }
}
