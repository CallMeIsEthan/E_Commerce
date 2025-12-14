using System;
using System.Collections.Generic;

namespace E_Commerce.Model.Models
{
    /// <summary>
    /// Model đại diện cho sản phẩm quần áo thời trang
    /// </summary>
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }              // URL-friendly slug (ví dụ: "ao-thun-nam-cao-cap")
        public string Description { get; set; }        // Mô tả ngắn (dùng cho SEO, preview, listing)
        public string Content { get; set; }            // Mô tả chi tiết (HTML, rich text - dùng cho trang chi tiết sản phẩm)
        
        public decimal Price { get; set; }
        public decimal? CompareAtPrice { get; set; }        // Giá so sánh (giá cũ)
        
        // Ảnh chính được lưu trong ProductImages với IsMain = true
        public int CategoryId { get; set; }
        public int? BrandId { get; set; }
        
        // Các thuộc tính dành cho quần áo
        public string Material { get; set; }                // Chất liệu
        public string Origin { get; set; }                  // Xuất xứ
        public string Style { get; set; }                   // Phong cách
        public string Season { get; set; }                  // Mùa
        public string Gender { get; set; }                  // Giới tính
        
        public int StockQuantity { get; set; }
        public string SKU { get; set; }
        
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsOnSale { get; set; }
        public bool IsDeleted { get; set; }
        
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public virtual Category Category { get; set; }
        public virtual Brand Brand { get; set; }
        public virtual ICollection<ProductVariant> ProductVariants { get; set; }
        public virtual ICollection<ProductImage> ProductImages { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Wishlist> Wishlists { get; set; }
    }
}

