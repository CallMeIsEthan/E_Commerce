using E_Commerce.Data.Infrastructure;
using E_Commerce.Model.Models;

namespace E_Commerce.Data.Repositories
{
    public interface IReviewRepository : IRepository<Review>
    {
    }

    public class ReviewRepository : RepositoryBase<Review>, IReviewRepository
    {
        public ReviewRepository(IDbFactory dbFactory) : base(dbFactory)
        {
        }
    }
}


