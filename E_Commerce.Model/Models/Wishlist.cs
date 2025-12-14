using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Commerce.Model.Models
{
    /// <summary>
    /// Model đại diện cho danh sách yêu thích
    /// </summary>
    public class Wishlist
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }

        public DateTime CreatedDate { get; set; }

        // Navigation properties
        public virtual User User { get; set; }

        public virtual Product Product { get; set; }
    }
}