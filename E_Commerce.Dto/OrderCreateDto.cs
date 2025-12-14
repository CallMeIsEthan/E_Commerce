using System.Collections.Generic;

namespace E_Commerce.Dto
{
    /// <summary>
    /// DTO để tạo mới đơn hàng
    /// </summary>
    public class OrderCreateDto
    {
        public int UserId { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TaxAmount { get; set; } // VAT 10% - chỉ để tính toán
        public decimal FinalTotal { get; set; } // FinalTotal từ cart (SubTotal + ShippingFee + TaxAmount)
        public string PaymentMethod { get; set; }
        public string ShippingName { get; set; }
        public string ShippingPhone { get; set; }
        public string ShippingAddress { get; set; }
        public int? DiscountCodeId { get; set; }
        public string CustomerNotes { get; set; }
        public List<OrderDetailCreateDto> OrderDetails { get; set; }
    }
}

