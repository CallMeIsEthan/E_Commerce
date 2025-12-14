using System;
using System.Collections.Generic;

namespace E_Commerce.Dto
{
    /// <summary>
    /// DTO cho vai trò/quyền
    /// </summary>
    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}


