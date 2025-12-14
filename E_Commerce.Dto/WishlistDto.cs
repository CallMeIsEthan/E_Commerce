using System;

namespace E_Commerce.Dto
{
    /// <summary>
    /// DTO cho danh sách yêu thích
    /// </summary>
    public class WishlistDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal ProductPrice { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

