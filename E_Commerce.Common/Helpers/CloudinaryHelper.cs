using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace E_Commerce.Common.Helpers
{
    /// <summary>
    /// Helper class để upload ảnh lên Cloudinary sử dụng CloudinaryDotNet library
    /// </summary>
    public static class CloudinaryHelper
    {
        /// <summary>
        /// Lấy Cloudinary instance (singleton pattern)
        /// </summary>
        private static Cloudinary GetCloudinary()
        {
            // Đọc từ Web.config để bảo mật
            var cloudName = ConfigurationManager.AppSettings["Cloudinary:CloudName"];
            var apiKey = ConfigurationManager.AppSettings["Cloudinary:ApiKey"];
            var apiSecret = ConfigurationManager.AppSettings["Cloudinary:ApiSecret"];

            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                throw new Exception("Cloudinary configuration is missing in Web.config. Please add Cloudinary:CloudName, Cloudinary:ApiKey, and Cloudinary:ApiSecret to appSettings.");
            }

            var account = new Account(cloudName, apiKey, apiSecret);
            return new Cloudinary(account);
        }

        /// <summary>
        /// Upload file lên Cloudinary và trả về secure_url
        /// </summary>
        /// <param name="file">File cần upload</param>
        /// <param name="folder">Folder trên Cloudinary (ví dụ: "products", "categories")</param>
        /// <param name="publicId">Public ID (alias) cho ảnh. Nếu null, Cloudinary sẽ tự generate UUID</param>
        public static async Task<string> UploadImageAsync(HttpPostedFileBase file, string folder = null, string publicId = null)
        {
            if (file == null || file.ContentLength == 0)
            {
                throw new ArgumentException("File không được để trống");
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName)?.ToLower();
            if (string.IsNullOrEmpty(fileExtension) || !Array.Exists(allowedExtensions, ext => ext == fileExtension))
            {
                throw new ArgumentException("File không hợp lệ. Chỉ chấp nhận: JPG, JPEG, PNG, GIF, WEBP");
            }

            // Validate file size (max 10MB)
            const int maxFileSize = 10 * 1024 * 1024; // 10MB
            if (file.ContentLength > maxFileSize)
            {
                throw new ArgumentException("File quá lớn. Kích thước tối đa: 10MB");
            }

            try
            {
                var cloudinary = GetCloudinary();

                // Đọc file vào memory stream (giữ stream mở cho CloudinaryDotNet)
                MemoryStream memoryStream = new MemoryStream();
                try
                {
                    if (file.InputStream.CanSeek)
                    {
                        file.InputStream.Position = 0;
                    }
                    file.InputStream.CopyTo(memoryStream);
                    memoryStream.Position = 0; // Reset position để CloudinaryDotNet đọc từ đầu

                    // Tạo FileDescription từ stream
                    var fileDescription = new FileDescription(file.FileName, memoryStream);

                    // Tạo upload parameters
                    var uploadParams = new ImageUploadParams()
                    {
                        File = fileDescription
                    };

                    // Thêm folder nếu có
                    if (!string.IsNullOrEmpty(folder))
                    {
                        uploadParams.Folder = folder;
                    }

                    // Thêm public_id (alias) nếu có - giúp quản lý và tránh trùng lặp
                    if (!string.IsNullOrEmpty(publicId))
                    {
                        uploadParams.PublicId = publicId;
                    }

                    System.Diagnostics.Debug.WriteLine($"[Cloudinary] Uploading file: {file.FileName}, Size: {memoryStream.Length} bytes");
                    if (!string.IsNullOrEmpty(folder))
                    {
                        System.Diagnostics.Debug.WriteLine($"[Cloudinary] Folder: {folder}");
                    }
                    if (!string.IsNullOrEmpty(publicId))
                    {
                        System.Diagnostics.Debug.WriteLine($"[Cloudinary] PublicId: {publicId}");
                    }

                    // Upload (CloudinaryDotNet tự động xử lý signature)
                    var uploadResult = await Task.Run(() => cloudinary.Upload(uploadParams));

                    if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Cloudinary] Error: StatusCode={uploadResult.StatusCode}, Error={uploadResult.Error?.Message}");
                        throw new Exception($"Lỗi upload Cloudinary: {uploadResult.Error?.Message ?? uploadResult.StatusCode.ToString()}");
                    }

                    var imageUrl = uploadResult.SecureUrl?.ToString() ?? uploadResult.Url?.ToString();
                    if (string.IsNullOrEmpty(imageUrl))
                    {
                        throw new Exception("Không lấy được URL ảnh từ Cloudinary response");
                    }

                    System.Diagnostics.Debug.WriteLine($"[Cloudinary] Upload successful: {imageUrl}");
                    return imageUrl;
                }
                finally
                {
                    // Đảm bảo stream được dispose sau khi upload xong
                    memoryStream?.Dispose();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Cloudinary] Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[Cloudinary] StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[Cloudinary] InnerException: {ex.InnerException.Message}");
                }
                throw; // Rethrow để tầng trên xử lý (hiển thị thông báo cho user)
            }
        }

        /// <summary>
        /// Upload file lên Cloudinary (synchronous version)
        /// </summary>
        public static string UploadImage(HttpPostedFileBase file, string folder = null, string publicId = null)
        {
            return UploadImageAsync(file, folder, publicId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Upload nhiều files lên Cloudinary
        /// </summary>
        public static async Task<List<string>> UploadImagesAsync(IEnumerable<HttpPostedFileBase> files, string folder = null)
        {
            var urls = new List<string>();
            foreach (var file in files)
            {
                if (file != null && file.ContentLength > 0)
                {
                    var url = await UploadImageAsync(file, folder);
                    urls.Add(url);
                }
            }
            return urls;
        }

        /// <summary>
        /// Upload nhiều files lên Cloudinary (synchronous version)
        /// </summary>
        public static List<string> UploadImages(IEnumerable<HttpPostedFileBase> files, string folder = null)
        {
            return UploadImagesAsync(files, folder).GetAwaiter().GetResult();
        }
    }
}