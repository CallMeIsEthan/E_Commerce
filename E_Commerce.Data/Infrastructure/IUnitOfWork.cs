namespace E_Commerce.Data.Infrastructure
{
    public interface IUnitOfWork
    {
        void Commit();
    }
}