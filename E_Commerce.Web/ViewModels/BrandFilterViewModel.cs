using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Web.ViewModels
{
    public class BrandFilterViewModel
    {
        [Display(Name = "Tìm kiếm")]
        public string SearchTerm { get; set; }

        [Display(Name = "Trạng thái")]
        public bool? IsActive { get; set; } // null = tất cả, true = kích hoạt, false = tắt

        [Display(Name = "Sắp xếp theo")]
        public string SortBy { get; set; } = "name"; // name, createdDate, displayOrder

        [Display(Name = "Thứ tự")]
        public string SortOrder { get; set; } = "asc"; // asc, desc
    }
}

