using System;

namespace E_Commerce.Dto
{
    /// <summary>
    /// DTO cho sản phẩm
    /// </summary>
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }              // URL-friendly slug
        public string Description { get; set; }        // Mô tả ngắn
        public string Content { get; set; }            // Mô tả chi tiết (HTML)
        public decimal Price { get; set; }
        public decimal? CompareAtPrice { get; set; }
        // Ảnh chính được lấy từ ProductImages với IsMain = true
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int? BrandId { get; set; }
        public string BrandName { get; set; }
        public string Material { get; set; }
        public string Origin { get; set; }
        public string Style { get; set; }
        public string Season { get; set; }
        public string Gender { get; set; }
        public int StockQuantity { get; set; }
        public string SKU { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsOnSale { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}

