namespace E_Commerce.Dto
{
    /// <summary>
    /// DTO để thêm item vào giỏ hàng
    /// </summary>
    public class CartItemCreateDto
    {
        public int ProductId { get; set; }
        public int? ProductVariantId { get; set; }
        public int Quantity { get; set; }
    }
}

