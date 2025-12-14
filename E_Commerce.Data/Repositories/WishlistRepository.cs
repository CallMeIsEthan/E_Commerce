using E_Commerce.Data.Infrastructure;
using E_Commerce.Model.Models;

namespace E_Commerce.Data.Repositories
{
    public interface IWishlistRepository : IRepository<Wishlist>
    {
    }

    public class WishlistRepository : RepositoryBase<Wishlist>, IWishlistRepository
    {
        public WishlistRepository(IDbFactory dbFactory) : base(dbFactory)
        {
        }
    }
}


