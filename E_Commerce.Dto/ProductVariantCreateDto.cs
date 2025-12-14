namespace E_Commerce.Dto
{
    /// <summary>
    /// DTO để tạo biến thể sản phẩm (màu, size, giá, stock)
    /// </summary>
    public class ProductVariantCreateDto
    {
        public int ProductId { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string SKU { get; set; }
        public string Size { get; set; }
        public string ColorName { get; set; }
        public string ColorCode { get; set; }
        public string Pattern { get; set; }
        // Ảnh được lưu trong ProductVariantImages với IsMain = true cho ảnh chính
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; }
    }
}

