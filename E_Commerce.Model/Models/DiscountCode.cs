using System;

namespace E_Commerce.Model.Models
{
    /// <summary>
    /// Model đại diện cho mã giảm giá
    /// </summary>
    public class DiscountCode
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string DiscountType { get; set; }           // Percentage, FixedAmount
        public decimal DiscountValue { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public int? UsageLimit { get; set; }              // Giới hạn tổng số lần sử dụng (toàn bộ user)
        public int? PerUserLimit { get; set; }            // Giới hạn số lần mỗi user được sử dụng
        public int UsedCount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public virtual System.Collections.Generic.ICollection<Order> Orders { get; set; }
    }
}

