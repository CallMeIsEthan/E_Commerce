using System;

namespace E_Commerce.Dto
{
    public class DiscountCodeUpdateDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string DiscountType { get; set; } // Percentage, FixedAmount
        public decimal DiscountValue { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public int? UsageLimit { get; set; }              // Giới hạn tổng số lần sử dụng (toàn bộ user)
        public int? PerUserLimit { get; set; }            // Giới hạn số lần mỗi user được sử dụng
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}

