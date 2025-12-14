namespace E_Commerce.Dto
{
    /// <summary>
    /// DTO để tạo đánh giá
    /// </summary>
    public class ReviewCreateDto
    {
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
}

