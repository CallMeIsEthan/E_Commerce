namespace E_Commerce.Web.ViewModels
{
    /// <summary>
    /// ViewModel cho bộ lọc và tìm kiếm sản phẩm
    /// </summary>
    public class ProductFilterViewModel
    {
        public string SearchTerm { get; set; }          // Tìm kiếm theo tên, SKU, mô tả
        public bool? IsActive { get; set; }             // Lọc theo trạng thái
        public int? CategoryId { get; set; }           // Lọc theo danh mục
        public int? BrandId { get; set; }              // Lọc theo thương hiệu
        public bool? IsFeatured { get; set; }          // Lọc sản phẩm nổi bật
        public bool? IsOnSale { get; set; }            // Lọc sản phẩm đang giảm giá
        public string SortBy { get; set; } = "name";    // Sắp xếp theo: name, price, createdDate, stockQuantity
        public string SortOrder { get; set; } = "asc"; // Thứ tự: asc, desc
    }
}

