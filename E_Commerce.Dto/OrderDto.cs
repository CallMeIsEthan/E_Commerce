using System;
using System.Collections.Generic;

namespace E_Commerce.Dto
{
    /// <summary>
    /// DTO cho đơn hàng
    /// </summary>
    public class OrderDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TaxAmount { get; set; } // VAT 10%
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentMethod { get; set; }
        public string ShippingName { get; set; }
        public string ShippingPhone { get; set; }
        public string ShippingAddress { get; set; }
        public string TrackingNumber { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string CancelReason { get; set; }
        public int? DiscountCodeId { get; set; }
        public string DiscountCode { get; set; }
        public string CustomerNotes { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public List<OrderDetailDto> OrderDetails { get; set; }
    }
}

