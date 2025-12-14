using System;
using System.Collections.Generic;

namespace E_Commerce.Dto
{
    /// <summary>
    /// DTO cho người dùng
    /// </summary>
    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        
        // Danh sách roles của user
        public List<string> Roles { get; set; }
        public List<RoleDto> RoleDetails { get; set; }
    }
}