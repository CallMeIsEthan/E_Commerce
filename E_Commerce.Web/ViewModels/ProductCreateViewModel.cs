using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace E_Commerce.Web.ViewModels
{
    /// <summary>
    /// ViewModel cho trang tạo/sửa sản phẩm - cần dropdown Categories và Brands
    /// </summary>
    public class ProductCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [Display(Name = "Tên sản phẩm")]
        [StringLength(255)]
        public string Name { get; set; }

        [Display(Name = "Alias (URL-friendly)")]
        [StringLength(255)]
        public string Alias { get; set; }              // Optional: tự động generate từ Name nếu để trống

        [Display(Name = "Mô tả ngắn")]
        [StringLength(500)]
        public string Description { get; set; }

        [Display(Name = "Mô tả chi tiết")]
        [AllowHtml]
        public string Content { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá")]
        [Display(Name = "Giá")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal Price { get; set; }

        [Display(Name = "Giá so sánh")]
        public decimal? CompareAtPrice { get; set; }

        // Ảnh chính được upload và lưu vào ProductImages với IsMain = true

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        [Display(Name = "Thương hiệu")]
        public int? BrandId { get; set; }

        [Display(Name = "Chất liệu")]
        public string Material { get; set; }

        [Display(Name = "Xuất xứ")]
        public string Origin { get; set; }

        [Display(Name = "Phong cách")]
        public string Style { get; set; }

        [Display(Name = "Mùa")]
        public string Season { get; set; }

        [Display(Name = "Giới tính")]
        public string Gender { get; set; }

        [Display(Name = "Số lượng tồn kho")]
        public int StockQuantity { get; set; } // Chỉ đọc, được tính tự động từ tổng Stock của variants

        [Display(Name = "SKU")]
        public string SKU { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Nổi bật")]
        public bool IsFeatured { get; set; }

        [Display(Name = "Đang giảm giá")]
        public bool IsOnSale { get; set; }

        // Dropdown lists cho view
        public SelectList Categories { get; set; }
        public SelectList Brands { get; set; }
        public SelectList Genders { get; set; }
        public SelectList Seasons { get; set; }

        // Danh sách biến thể sản phẩm (màu, size, giá, stock)
        public List<ProductVariantViewModel> Variants { get; set; } = new List<ProductVariantViewModel>();
    }
}


