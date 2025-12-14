using System;

namespace E_Commerce.Dto
{
    /// <summary>
    /// DTO cho quan hệ User-Role
    /// </summary>
    public class UserRoleDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public DateTime AssignedDate { get; set; }
    }
}


