using System;

namespace E_Commerce.Dto
{
    /// <summary>
    /// DTO cho mã giảm giá
    /// </summary>
    public class DiscountCodeDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public int? UsageLimit { get; set; }              // Giới hạn tổng số lần sử dụng (toàn bộ user)
        public int? PerUserLimit { get; set; }            // Giới hạn số lần mỗi user được sử dụng
        public int UsedCount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsValid { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}

