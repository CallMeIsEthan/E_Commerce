using System;

namespace E_Commerce.Dto
{
    /// <summary>
    /// DTO cho chi tiết đơn hàng
    /// </summary>
    public class OrderDetailDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int? ProductVariantId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public string ProductImage { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

