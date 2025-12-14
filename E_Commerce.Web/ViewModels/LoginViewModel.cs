using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Web.ViewModels
{
    /// <summary>
    /// ViewModel cho trang đăng nhập (có thể mở rộng thêm Remember Me, etc.)
    /// </summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Username { get; set; } // Property name giữ nguyên để tương thích với UserLoginDto, nhưng thực tế là Email

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }
    }
}


