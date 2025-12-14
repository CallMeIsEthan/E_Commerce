using System.Collections.Generic;
using E_Commerce.Dto;

namespace E_Commerce.Service
{
    public interface IWishlistService
    {
        IEnumerable<WishlistDto> GetByUser(int userId);
        WishlistDto Add(int userId, int productId);
        bool Remove(int userId, int productId);
        bool Clear(int userId);
        int Count(int userId);
        bool Exists(int userId, int productId);
        List<int> GetWishlistProductIds(int userId);
    }
}

