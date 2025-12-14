using System.Collections.Generic;

namespace E_Commerce.Dto
{
    /// <summary>
    /// DTO để tạo mới người dùng
    /// </summary>
    public class UserCreateDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public List<int> RoleIds { get; set; } // Danh sách ID các role được gán cho user
    }
}

