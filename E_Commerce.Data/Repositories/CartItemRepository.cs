using E_Commerce.Data.Infrastructure;
using E_Commerce.Model.Models;

namespace E_Commerce.Data.Repositories
{
    public interface ICartItemRepository : IRepository<CartItem>
    {
    }

    public class CartItemRepository : RepositoryBase<CartItem>, ICartItemRepository
    {
        public CartItemRepository(IDbFactory dbFactory) : base(dbFactory)
        {
        }
    }
}


