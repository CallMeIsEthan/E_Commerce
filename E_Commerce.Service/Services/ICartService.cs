using E_Commerce.Dto;
using System.Collections.Generic;

namespace E_Commerce.Service
{
    public interface ICartService
    {
        /// <summary>
        /// Lấy giỏ hàng của user
        /// </summary>
        CartDto GetCartByUserId(int userId);

        /// <summary>
        /// Thêm sản phẩm vào giỏ hàng
        /// </summary>
        CartItemDto AddToCart(int userId, CartItemCreateDto cartItemCreateDto);

        /// <summary>
        /// Cập nhật số lượng sản phẩm trong giỏ hàng
        /// </summary>
        CartItemDto UpdateCartItem(int cartItemId, int quantity);

        /// <summary>
        /// Xóa sản phẩm khỏi giỏ hàng
        /// </summary>
        bool RemoveFromCart(int cartItemId);

        /// <summary>
        /// Xóa toàn bộ giỏ hàng
        /// </summary>
        bool ClearCart(int userId);

        /// <summary>
        /// Lấy số lượng sản phẩm trong giỏ hàng
        /// </summary>
        int GetCartItemCount(int userId);
    }
}

