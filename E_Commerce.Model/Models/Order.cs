using System;
using System.Collections.Generic;

namespace E_Commerce.Model.Models
{
    /// <summary>
    /// Model đại diện cho đơn hàng
    /// </summary>
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string OrderNumber { get; set; }
        
        public DateTime OrderDate { get; set; }
        
        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        // TaxAmount không có trong DB, chỉ dùng để tính toán và hiển thị trên giao diện
        // Nếu DB có cột này thì sẽ báo lỗi - cần xóa cột TaxAmount khỏi DB schema
        // Hoặc ignore trong mapping configuration
        // public decimal TaxAmount { get; set; } // Đã comment - không có trong DB
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        
        public string Status { get; set; }                  // Pending, Processing, Shipped, Delivered, Cancelled
        public string PaymentStatus { get; set; }           // Pending, Paid, Failed
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
        public string CustomerNotes { get; set; }
        
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
        public virtual DiscountCode DiscountCodeEntity { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}

