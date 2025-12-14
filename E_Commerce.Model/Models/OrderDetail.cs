using System;

namespace E_Commerce.Model.Models
{
    /// <summary>
    /// Model đại diện cho chi tiết đơn hàng
    /// </summary>
    public class OrderDetail
    {
        // Không có Id - DB dùng composite PK (OrderId, ProductId)
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int? ProductVariantId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public DateTime CreatedDate { get; set; }

        // Navigation properties
        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
        public virtual ProductVariant ProductVariant { get; set; }
    }
}

