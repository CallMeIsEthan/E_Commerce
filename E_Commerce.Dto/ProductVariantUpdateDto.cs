namespace E_Commerce.Dto
{
    public class ProductVariantUpdateDto
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string SKU { get; set; }
        public string Size { get; set; }
        public string ColorName { get; set; }
        public string ColorCode { get; set; }
        public string Pattern { get; set; }
        // Ảnh được lưu trong ProductVariantImages với IsMain = true cho ảnh chính
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}

