namespace E_Commerce.Dto
{
    /// <summary>
    /// DTO để đăng nhập
    /// Lưu ý: Property Username thực tế chứa email (giữ tên Username để tương thích với form)
    /// </summary>
    public class UserLoginDto
    {
        public string Username { get; set; } // Thực tế chứa email
        public string Password { get; set; }
    }
}

