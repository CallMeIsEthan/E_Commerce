using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace E_Commerce.Web.ViewModels
{
    /// <summary>
    /// ViewModel cho trang tạo/sửa danh mục
    /// </summary>
    public class CategoryCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [Display(Name = "Tên danh mục")]
        [StringLength(255)]
        public string Name { get; set; }

        [Display(Name = "Alias (URL-friendly)")]
        [StringLength(255)]
        public string Alias { get; set; }              // Optional: tự động generate từ Name nếu để trống

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "Danh mục cha")]
        public int? ParentId { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Hiển thị trên trang chủ")]
        public bool HomeFlag { get; set; } = false;

        [Display(Name = "Thứ tự hiển thị")]
        [Range(0, int.MaxValue, ErrorMessage = "Thứ tự phải >= 0")]
        public int DisplayOrder { get; set; } = 0;

        // Dropdown cho danh mục cha
        public SelectList ParentCategories { get; set; }
    }
}


