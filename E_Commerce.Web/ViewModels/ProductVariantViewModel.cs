using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Web.ViewModels
{
    /// <summary>
    /// ViewModel cho biến thể sản phẩm (màu, size, giá, stock)
    /// </summary>
    public class ProductVariantViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập giá")]
        [Display(Name = "Giá")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng tồn kho")]
        [Display(Name = "Số lượng")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 0")]
        public int Stock { get; set; }

        [Display(Name = "SKU")]
        public string SKU { get; set; }

        [Display(Name = "Kích thước")]
        public string Size { get; set; }

        [Display(Name = "Tên màu")]
        public string ColorName { get; set; }

        [Display(Name = "Mã màu (HEX)")]
        [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Mã màu phải là mã HEX hợp lệ (ví dụ: #FF0000)")]
        public string ColorCode { get; set; }

        [Display(Name = "Họa tiết")]
        public string Pattern { get; set; }

        // Ảnh được upload và lưu vào ProductVariantImages với IsMain = true cho ảnh chính

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; } = true;
    }
}

