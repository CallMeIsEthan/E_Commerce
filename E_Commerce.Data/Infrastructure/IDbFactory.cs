using System;

namespace E_Commerce.Data.Infrastructure
{
    public interface IDbFactory : IDisposable
    {
        E_CommerceDbContext Init();
    }
}