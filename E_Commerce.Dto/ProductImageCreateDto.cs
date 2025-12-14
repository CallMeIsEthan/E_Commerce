namespace E_Commerce.Dto
{
    /// <summary>
    /// DTO để tạo mới ảnh sản phẩm
    /// </summary>
    public class ProductImageCreateDto
    {
        public int ProductId { get; set; }
        public string ImageUrl { get; set; }
        public bool IsMain { get; set; }
        public int DisplayOrder { get; set; }
    }
}

