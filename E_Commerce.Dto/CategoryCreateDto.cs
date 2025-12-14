namespace E_Commerce.Dto
{
    /// <summary>
    /// DTO để tạo mới danh mục
    /// </summary>
    public class CategoryCreateDto
    {
        public string Name { get; set; }
        public string Alias { get; set; }              // URL-friendly slug (tự động generate từ Name nếu null)
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int? ParentCategoryId { get; set; }
        public bool IsActive { get; set; }
        public bool HomeFlag { get; set; }                // Hiển thị trên trang chủ
        public int DisplayOrder { get; set; }
        public bool IsDeleted { get; set; }
    }
}

