namespace E_Commerce.Dto
{
    /// <summary>
    /// DTO để cập nhật danh mục
    /// </summary>
    public class CategoryUpdateDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }              // URL-friendly slug
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int? ParentCategoryId { get; set; }
        public bool IsActive { get; set; }
        public bool HomeFlag { get; set; }                // Hiển thị trên trang chủ
        public int DisplayOrder { get; set; }
        public bool IsDeleted { get; set; }
    }
}

