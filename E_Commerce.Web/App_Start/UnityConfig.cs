using System.Web.Mvc;
using System.Web.Http;
using Unity;
using Unity.Mvc5;
using Unity.WebApi;
using E_Commerce.Data.Infrastructure;
using E_Commerce.Data.Repositories;
using E_Commerce.Service;
using Unity.Lifetime;
using AutoMapper;
using System;

namespace E_Commerce.Web
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
            var container = new UnityContainer();

            // Register Infrastructure
            container.RegisterType<IDbFactory, DbFactory>(new HierarchicalLifetimeManager());
            container.RegisterType<IUnitOfWork, UnitOfWork>(new HierarchicalLifetimeManager());

            // Register AutoMapper
            // Đảm bảo AutoMapper đã được configure trước khi đăng ký
            if (AutoMapperConfig.Mapper == null)
            {
                throw new InvalidOperationException("AutoMapperConfig.Mapper is null. Make sure AutoMapperConfig.Configure() is called before UnityConfig.RegisterComponents().");
            }
            container.RegisterInstance<IMapper>(AutoMapperConfig.Mapper);

            // Register Repositories
            container.RegisterType<IProductRepository, ProductRepository>();
            container.RegisterType<ICategoryRepository, CategoryRepository>();
            container.RegisterType<IBrandRepository, BrandRepository>();
            container.RegisterType<IProductVariantRepository, ProductVariantRepository>();
            container.RegisterType<IProductImageRepository, ProductImageRepository>();
            container.RegisterType<IProductVariantImageRepository, ProductVariantImageRepository>();
            container.RegisterType<IUserRepository, UserRepository>();
            container.RegisterType<IRoleRepository, RoleRepository>();
            container.RegisterType<IUserRoleRepository, UserRoleRepository>();
            container.RegisterType<IOrderRepository, OrderRepository>();
            container.RegisterType<IOrderDetailRepository, OrderDetailRepository>();
            container.RegisterType<ICartRepository, CartRepository>();
            container.RegisterType<ICartItemRepository, CartItemRepository>();
            container.RegisterType<IReviewRepository, ReviewRepository>();
            container.RegisterType<IWishlistRepository, WishlistRepository>();
            container.RegisterType<IDiscountCodeRepository, DiscountCodeRepository>();

            // Register Services
            container.RegisterType<IAccountService, AccountService>();
            container.RegisterType<IProductService, ProductService>();
            container.RegisterType<ICategoryService, CategoryService>();
            container.RegisterType<IBrandService, BrandService>();
            container.RegisterType<ICartService, CartService>();
            container.RegisterType<IOrderService, OrderService>();
            container.RegisterType<IReviewService, ReviewService>();
            container.RegisterType<IWishlistService, WishlistService>();
            container.RegisterType<IDiscountCodeService, DiscountCodeService>();
            // ... các service khác

            // Set MVC Dependency Resolver
            DependencyResolver.SetResolver(new Unity.Mvc5.UnityDependencyResolver(container));
            
            // Set Web API Dependency Resolver
            GlobalConfiguration.Configuration.DependencyResolver = new Unity.WebApi.UnityDependencyResolver(container);
        }
    }
}