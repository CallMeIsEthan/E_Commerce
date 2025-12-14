using System;

namespace E_Commerce.Dto
{
    /// <summary>
    /// DTO cho ảnh sản phẩm
    /// </summary>
    public class ProductImageDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ImageUrl { get; set; }
        public bool IsMain { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

