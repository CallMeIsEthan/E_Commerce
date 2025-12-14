using System;

namespace E_Commerce.Model.Models
{
    /// <summary>
    /// Model đại diện cho ảnh sản phẩm
    /// </summary>
    public class ProductImage
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ImageUrl { get; set; }
        public bool IsMain { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedDate { get; set; }

        // Navigation properties
        public virtual Product Product { get; set; }
    }
}

