namespace E_Commerce.Dto
{
    public class ProductVariantImageCreateDto
    {
        public int ProductVariantId { get; set; }
        public string ImageUrl { get; set; }
        public bool IsMain { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;
    }
}

