namespace QLGiaSuLopHoc.Services
{
    /// <summary>Lưu ảnh tải lên một cách an toàn (kiểm tra đuôi file, dung lượng và chữ ký file thực).</summary>
    public class FileUploadService
    {
        private static readonly string[] AllowedExt = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxBytes = 3 * 1024 * 1024; // 3 MB

        private readonly IWebHostEnvironment _env;
        public FileUploadService(IWebHostEnvironment env) => _env = env;

        /// <returns>(đường dẫn web, thông báo lỗi). Nếu lỗi thì đường dẫn = null.</returns>
        public async Task<(string? Path, string? Error)> SaveImageAsync(IFormFile file, string subFolder, string prefix)
        {
            if (file.Length == 0) return (null, "Tệp tải lên rỗng.");
            if (file.Length > MaxBytes) return (null, "Ảnh vượt quá dung lượng tối đa 3 MB.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExt.Contains(ext)) return (null, "Chỉ chấp nhận ảnh .jpg, .jpeg, .png hoặc .webp.");

            // Kiểm tra chữ ký (magic bytes) để chặn file giả đuôi ảnh
            var header = new byte[12];
            await using (var probe = file.OpenReadStream())
            {
                var read = await probe.ReadAsync(header.AsMemory(0, header.Length));
                if (read < 4 || !HasImageSignature(header)) return (null, "Tệp không phải là ảnh hợp lệ.");
            }

            var folder = Path.Combine(_env.WebRootPath, "uploads", subFolder);
            Directory.CreateDirectory(folder);
            var fileName = $"{prefix}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N")[..6]}{ext}";
            await using (var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return ($"/uploads/{subFolder}/{fileName}", null);
        }

        private static bool HasImageSignature(byte[] h)
        {
            bool jpg = h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF;
            bool png = h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47;
            bool webp = h[0] == 0x52 && h[1] == 0x49 && h[2] == 0x46 && h[3] == 0x46 && h[8] == 0x57 && h[9] == 0x45 && h[10] == 0x42 && h[11] == 0x50;
            return jpg || png || webp;
        }
    }
}
