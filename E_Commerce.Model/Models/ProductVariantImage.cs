using System;

namespace E_Commerce.Model.Models
{
    /// <summary>
    /// Model đại diện cho ảnh của biến thể sản phẩm
    /// </summary>
    public class ProductVariantImage
    {
        public int Id { get; set; }
        public int ProductVariantId { get; set; }
        public string ImageUrl { get; set; }
        public bool IsMain { get; set; }              // Ảnh chính của variant
        public int DisplayOrder { get; set; }        // Thứ tự hiển thị
        public DateTime CreatedDate { get; set; }

        // Navigation properties
        public virtual ProductVariant ProductVariant { get; set; }
    }
}
