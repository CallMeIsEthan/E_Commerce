namespace E_Commerce.Dto
{
    /// <summary>
    /// DTO để tạo chi tiết đơn hàng
    /// </summary>
    public class OrderDetailCreateDto
    {
        public int ProductId { get; set; }
        public int? ProductVariantId { get; set; }
        public int Quantity { get; set; }
    }
}

