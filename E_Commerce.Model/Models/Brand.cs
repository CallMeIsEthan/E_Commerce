using System;
using System.Collections.Generic;

namespace E_Commerce.Model.Models
{
    /// <summary>
    /// Model đại diện cho thương hiệu
    /// </summary>
    public class Brand
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string LogoUrl { get; set; }
        
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsDeleted { get; set; }
        
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public virtual ICollection<Product> Products { get; set; }
    }
}

