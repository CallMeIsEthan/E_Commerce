using E_Commerce.Data.Infrastructure;
using E_Commerce.Model.Models;

namespace E_Commerce.Data.Repositories
{
    public class ProductVariantImageRepository : RepositoryBase<ProductVariantImage>, IProductVariantImageRepository
    {
        public ProductVariantImageRepository(IDbFactory dbFactory) : base(dbFactory)
        {
        }
    }
}
