using System;

namespace E_Commerce.Model.Models
{
    /// <summary>
    /// Model đại diện cho đánh giá sản phẩm
    /// </summary>
    public class Review
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        
        public int Rating { get; set; }                    // Đánh giá (1-5 sao)
        public string Comment { get; set; }
        public bool IsApproved { get; set; }
        public bool IsDeleted { get; set; }
        
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        public virtual Product Product { get; set; }
        public virtual User User { get; set; }
    }
}

