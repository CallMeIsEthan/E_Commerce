using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Web.ViewModels
{
    /// <summary>
    /// ViewModel cho trang tạo/sửa thương hiệu
    /// </summary>
    public class BrandCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên thương hiệu")]
        [Display(Name = "Tên thương hiệu")]
        [StringLength(255)]
        public string Name { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Thứ tự hiển thị")]
        [Range(0, int.MaxValue, ErrorMessage = "Thứ tự phải lớn hơn hoặc bằng 0")]
        public int DisplayOrder { get; set; } = 0;
    }
}

