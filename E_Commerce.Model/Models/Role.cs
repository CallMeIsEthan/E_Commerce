using System;
using System.Collections.Generic;

namespace E_Commerce.Model.Models
{
    /// <summary>
    /// Model đại diện cho vai trò/quyền
    /// </summary>
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }                    // Admin, Customer, Manager, etc.
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}


