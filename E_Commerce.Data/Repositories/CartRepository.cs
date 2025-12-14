using E_Commerce.Data.Infrastructure;
using E_Commerce.Model.Models;

namespace E_Commerce.Data.Repositories
{
    public interface ICartRepository : IRepository<Cart>
    {
    }

    public class CartRepository : RepositoryBase<Cart>, ICartRepository
    {
        public CartRepository(IDbFactory dbFactory) : base(dbFactory)
        {
        }
    }
}


