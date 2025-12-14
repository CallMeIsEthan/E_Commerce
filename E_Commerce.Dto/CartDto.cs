using System;
using System.Collections.Generic;

namespace E_Commerce.Dto
{
    /// <summary>
    /// DTO cho giỏ hàng
    /// </summary>
    public class CartDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public List<CartItemDto> CartItems { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TaxAmount { get; set; } // VAT 10% - chỉ tính động, không lưu DB
        public decimal FinalTotal { get; set; } // TotalAmount + ShippingFee + TaxAmount
        public int TotalItems { get; set; }
    }
}

