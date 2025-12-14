using System;
using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Web.ViewModels
{
    public class DiscountCodeCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã giảm giá")]
        [Display(Name = "Mã giảm giá")]
        [StringLength(50, ErrorMessage = "Mã giảm giá không được vượt quá 50 ký tự")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên mã giảm giá")]
        [Display(Name = "Tên mã giảm giá")]
        [StringLength(255)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại giảm giá")]
        [Display(Name = "Loại giảm giá")]
        public string DiscountType { get; set; } // Percentage, FixedAmount

        [Required(ErrorMessage = "Vui lòng nhập giá trị giảm giá")]
        [Display(Name = "Giá trị giảm giá")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá trị giảm giá phải lớn hơn 0")]
        public decimal DiscountValue { get; set; }

        [Display(Name = "Đơn hàng tối thiểu")]
        [Range(0, double.MaxValue, ErrorMessage = "Đơn hàng tối thiểu phải lớn hơn hoặc bằng 0")]
        public decimal? MinOrderAmount { get; set; }

        [Display(Name = "Giới hạn tổng số lần sử dụng")]
        [Range(1, int.MaxValue, ErrorMessage = "Giới hạn tổng số lần sử dụng phải lớn hơn 0")]
        public int? UsageLimit { get; set; }

        [Display(Name = "Giới hạn số lần mỗi user")]
        [Range(1, int.MaxValue, ErrorMessage = "Giới hạn số lần mỗi user phải lớn hơn 0")]
        public int? PerUserLimit { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu")]
        [Display(Name = "Ngày bắt đầu")]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc")]
        [Display(Name = "Ngày kết thúc")]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(1);

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; } = true;
    }
}

