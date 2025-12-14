using E_Commerce.Data.Infrastructure;
using E_Commerce.Model.Models;

namespace E_Commerce.Data.Repositories
{
    public interface IProductVariantRepository : IRepository<ProductVariant>
    {
    }

    public class ProductVariantRepository : RepositoryBase<ProductVariant>, IProductVariantRepository
    {
        public ProductVariantRepository(IDbFactory dbFactory) : base(dbFactory)
        {
        }
    }
}


