namespace E_Commerce.Dto
{
    /// <summary>
    /// DTO để tạo mới sản phẩm
    /// </summary>
    public class ProductCreateDto
    {
        public string Name { get; set; }
        public string Alias { get; set; }              // URL-friendly slug (tự động generate từ Name nếu null)
        public string Description { get; set; }        // Mô tả ngắn
        public string Content { get; set; }           // Mô tả chi tiết (HTML)
        public decimal Price { get; set; }
        public decimal? CompareAtPrice { get; set; }
        // Ảnh chính được lưu trong ProductImages với IsMain = true
        public int CategoryId { get; set; }
        public int? BrandId { get; set; }
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
    }
}

