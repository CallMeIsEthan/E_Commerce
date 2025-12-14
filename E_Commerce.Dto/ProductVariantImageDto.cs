using System;

namespace E_Commerce.Dto
{
    public class ProductVariantImageDto
    {
        public int Id { get; set; }
        public int ProductVariantId { get; set; }
        public string ImageUrl { get; set; }
        public bool IsMain { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

