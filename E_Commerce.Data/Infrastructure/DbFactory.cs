namespace E_Commerce.Data.Infrastructure
{
    public class DbFactory : Disposable, IDbFactory
    {
        private E_CommerceDbContext dbContext;

        public E_CommerceDbContext Init()
        {
            return dbContext ?? (dbContext = new E_CommerceDbContext());
        }

        protected override void DisposeCore()
        {
            if (dbContext != null)
                dbContext.Dispose();
        }
    }
}