using System;

namespace E_Commerce.Dto
{
    /// <summary>
    /// DTO cho biến thể sản phẩm
    /// </summary>
    public class ProductVariantDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string SKU { get; set; }
        public string Size { get; set; }
        public string ColorName { get; set; }
        public string ColorCode { get; set; }
        public string Pattern { get; set; }
        // Ảnh chính được lấy từ ProductVariantImages với IsMain = true
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}

