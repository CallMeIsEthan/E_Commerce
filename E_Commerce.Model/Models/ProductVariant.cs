using System;
using System.Collections.Generic;

namespace E_Commerce.Model.Models
{
    /// <summary>
    /// Model đại diện cho biến thể sản phẩm (Size, Color, Pattern)
    /// </summary>
    public class ProductVariant
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string SKU { get; set; }
        
        public string Size { get; set; }                    // Kích thước
        public string ColorName { get; set; }               // Tên màu
        public string ColorCode { get; set; }               // Mã màu (HEX)
        public string Pattern { get; set; }                 // Họa tiết
        // Ảnh chính được lưu trong ProductVariantImages với IsMain = true
        
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public virtual Product Product { get; set; }
        public virtual ICollection<ProductVariantImage> ProductVariantImages { get; set; }  // Nhiều ảnh cho variant
        public virtual ICollection<CartItem> CartItems { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}

