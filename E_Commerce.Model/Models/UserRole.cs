using System;

namespace E_Commerce.Model.Models
{
    /// <summary>
    /// Model đại diện cho quan hệ nhiều-nhiều giữa User và Role
    /// </summary>
    public class UserRole
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public DateTime AssignedDate { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
        public virtual Role Role { get; set; }
    }
}


