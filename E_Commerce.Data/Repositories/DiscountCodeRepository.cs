using E_Commerce.Data.Infrastructure;
using E_Commerce.Model.Models;

namespace E_Commerce.Data.Repositories
{
    public interface IDiscountCodeRepository : IRepository<DiscountCode>
    {
    }

    public class DiscountCodeRepository : RepositoryBase<DiscountCode>, IDiscountCodeRepository
    {
        public DiscountCodeRepository(IDbFactory dbFactory) : base(dbFactory)
        {
        }
    }
}


