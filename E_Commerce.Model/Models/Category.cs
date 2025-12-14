using System;
using System.Collections.Generic;

namespace E_Commerce.Model.Models
{
    /// <summary>
    /// Model đại diện cho danh mục sản phẩm
    /// </summary>
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }              // URL-friendly slug (ví dụ: "ao-thun-nam")
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        
        public int? ParentCategoryId { get; set; }
        
        public bool IsActive { get; set; }
        public bool HomeFlag { get; set; }                // Hiển thị trên trang chủ
        public int DisplayOrder { get; set; }
        public bool IsDeleted { get; set; }
        
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public virtual Category ParentCategory { get; set; }
        public virtual ICollection<Category> SubCategories { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}

