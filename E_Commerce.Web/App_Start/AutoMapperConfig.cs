using AutoMapper;
using E_Commerce.Dto;
using E_Commerce.Model.Models;
using System;
using System.Linq;

namespace E_Commerce.Web
{
    public class AutoMapperConfig
    {
        public static IMapper Mapper { get; private set; }

        public static void Configure()
        {
            var config = new MapperConfiguration(cfg =>
            {
                // Product Mappings
                cfg.CreateMap<Product, ProductDto>()
                    .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
                    .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.Name : string.Empty));

                cfg.CreateMap<ProductCreateDto, Product>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore()) // Sẽ set khi tạo mới
                    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore()) // Sẽ set trong service
                    .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore()) // default false trong service/DB
                    .ForMember(dest => dest.StockQuantity, opt => opt.Ignore()) // Sẽ được tính từ variants
                    .ForMember(dest => dest.Category, opt => opt.Ignore())
                    .ForMember(dest => dest.Brand, opt => opt.Ignore())
                    .ForMember(dest => dest.ProductVariants, opt => opt.Ignore())
                    .ForMember(dest => dest.ProductImages, opt => opt.Ignore())
                    .ForMember(dest => dest.Reviews, opt => opt.Ignore())
                    .ForMember(dest => dest.Wishlists, opt => opt.Ignore());
                    
                cfg.CreateMap<ProductUpdateDto, Product>()
                    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore()) // Giữ nguyên giá trị cũ
                    .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore()) // Sẽ set trong service
                    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore()) // giữ cờ xóa, set trong service nếu cần
                    .ForMember(dest => dest.StockQuantity, opt => opt.Ignore()) // Sẽ được tính từ variants
                    .ForMember(dest => dest.Category, opt => opt.Ignore())
                    .ForMember(dest => dest.Brand, opt => opt.Ignore())
                    .ForMember(dest => dest.ProductVariants, opt => opt.Ignore())
                    .ForMember(dest => dest.ProductImages, opt => opt.Ignore())
                    .ForMember(dest => dest.Reviews, opt => opt.Ignore())
                    .ForMember(dest => dest.Wishlists, opt => opt.Ignore())
                    .ForMember(dest => dest.Alias, opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.Alias))); // Chỉ update alias nếu có giá trị

                // Category Mappings
                cfg.CreateMap<Category, CategoryDto>()
                    .ForMember(dest => dest.ParentCategoryName, opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : string.Empty))
                    .ForMember(dest => dest.ParentCategoryId, opt => opt.MapFrom(src => src.ParentCategoryId));

                cfg.CreateMap<CategoryCreateDto, Category>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore()) // Sẽ set khi tạo mới
                    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore()) // Sẽ set trong service
                    .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore()) // default false trong service/DB
                    .ForMember(dest => dest.ParentCategory, opt => opt.Ignore())
                    .ForMember(dest => dest.SubCategories, opt => opt.Ignore())
                    .ForMember(dest => dest.Products, opt => opt.Ignore());

                cfg.CreateMap<CategoryUpdateDto, Category>()
                    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore()) // Giữ nguyên giá trị cũ
                    .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore()) // Sẽ set trong service
                    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore()) // giữ cờ xóa, set trong service
                    .ForMember(dest => dest.ParentCategory, opt => opt.Ignore())
                    .ForMember(dest => dest.SubCategories, opt => opt.Ignore())
                    .ForMember(dest => dest.Products, opt => opt.Ignore())
                    .ForMember(dest => dest.Alias, opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.Alias))); // Chỉ update alias nếu có giá trị

                // Brand Mappings
                cfg.CreateMap<Brand, BrandDto>();

                cfg.CreateMap<BrandCreateDto, Brand>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore())
                    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                    .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                    .ForMember(dest => dest.Products, opt => opt.Ignore());

                cfg.CreateMap<BrandUpdateDto, Brand>()
                    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                    .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                    .ForMember(dest => dest.Products, opt => opt.Ignore());

                // ProductVariant Mappings
                cfg.CreateMap<ProductVariant, ProductVariantDto>()
                    .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
                    .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId));

                cfg.CreateMap<ProductVariantCreateDto, ProductVariant>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore()) // Sẽ set khi tạo mới
                    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore()) // Sẽ set trong service
                    .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore()) // default false trong service/DB
                    .ForMember(dest => dest.Product, opt => opt.Ignore())
                    .ForMember(dest => dest.ProductVariantImages, opt => opt.Ignore()) // Sẽ được thêm riêng sau khi tạo variant
                    .ForMember(dest => dest.CartItems, opt => opt.Ignore())
                    .ForMember(dest => dest.OrderDetails, opt => opt.Ignore());

                cfg.CreateMap<ProductVariantUpdateDto, ProductVariant>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore()) // Sẽ set trong service
                    .ForMember(dest => dest.ProductId, opt => opt.Ignore()) // Giữ nguyên
                    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore()) // Giữ nguyên
                    .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore()) // Sẽ set trong service
                    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore()) // giữ cờ xóa
                    .ForMember(dest => dest.Product, opt => opt.Ignore())
                    .ForMember(dest => dest.ProductVariantImages, opt => opt.Ignore()) // Sẽ được quản lý riêng
                    .ForMember(dest => dest.CartItems, opt => opt.Ignore())
                    .ForMember(dest => dest.OrderDetails, opt => opt.Ignore());

                cfg.CreateMap<E_Commerce.Web.ViewModels.ProductVariantViewModel, ProductVariantCreateDto>()
                    .ForMember(dest => dest.ProductId, opt => opt.Ignore()) // Sẽ set trong controller sau khi tạo product
                    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore()); // default false trong service/DB

                // User Mappings
                cfg.CreateMap<User, UserDto>()
                    .ForMember(dest => dest.Roles, opt => opt.Ignore()) // Sẽ map riêng trong service
                    .ForMember(dest => dest.RoleDetails, opt => opt.Ignore()); // Sẽ map riêng trong service

                // ViewModel -> DTO Mappings (for Controller layer)
                cfg.CreateMap<E_Commerce.Web.ViewModels.UserCreateViewModel, UserCreateDto>()
                    .ForMember(dest => dest.RoleIds, opt => opt.Ignore()); // Sẽ set mặc định trong controller
                    
                cfg.CreateMap<E_Commerce.Web.ViewModels.ProductCreateViewModel, ProductCreateDto>()
                    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore()); // IsDeleted set trong service
                // Ảnh chính được upload và lưu vào ProductImages với IsMain = true
                
                cfg.CreateMap<E_Commerce.Web.ViewModels.ProductCreateViewModel, ProductUpdateDto>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore()) // Id sẽ được set trong controller
                    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore()); // giữ cờ xóa
                // Mapping cho Edit: ProductCreateViewModel -> ProductUpdateDto

                cfg.CreateMap<E_Commerce.Web.ViewModels.CategoryCreateViewModel, CategoryCreateDto>()
                    .ForMember(dest => dest.ParentCategoryId, opt => opt.MapFrom(src => src.ParentId))
                    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                    .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()) // Sẽ set sau khi upload ảnh
                    .ForMember(dest => dest.DisplayOrder, opt => opt.Ignore()); // Sẽ set mặc định trong service

                cfg.CreateMap<E_Commerce.Web.ViewModels.BrandCreateViewModel, BrandCreateDto>()
                    .ForMember(dest => dest.LogoUrl, opt => opt.Ignore()) // Sẽ set sau khi upload logo
                    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
                    
                cfg.CreateMap<UserCreateDto, User>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore()) // Sẽ set khi tạo mới
                    .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Sẽ hash password trong service (từ Password trong DTO)
                    .ForMember(dest => dest.IsActive, opt => opt.Ignore()) // Sẽ set mặc định trong service
                    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore()) // Sẽ set trong service
                    .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                    .ForMember(dest => dest.Carts, opt => opt.Ignore())
                    .ForMember(dest => dest.Orders, opt => opt.Ignore())
                    .ForMember(dest => dest.Reviews, opt => opt.Ignore())
                    .ForMember(dest => dest.Wishlists, opt => opt.Ignore())
                    .ForMember(dest => dest.UserRoles, opt => opt.Ignore()); // Sẽ set trong service

                // Role Mappings
                cfg.CreateMap<Role, RoleDto>();

                // UserRole Mappings
                cfg.CreateMap<UserRole, UserRoleDto>()
                    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
                    .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : string.Empty));

                // Order Mappings
                cfg.CreateMap<Order, OrderDto>()
                    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
                    .ForMember(dest => dest.DiscountCode, opt => opt.MapFrom(src => src.DiscountCodeEntity != null ? src.DiscountCodeEntity.Code : string.Empty))
                    .ForMember(dest => dest.OrderDetails, opt => opt.Ignore()) // Sẽ map riêng trong service
                    // TaxAmount không có trong DB, tính lại từ TotalAmount - SubTotal - ShippingFee
                    .ForMember(dest => dest.TaxAmount, opt => opt.MapFrom(src => src.TotalAmount - src.SubTotal - src.ShippingFee));

                cfg.CreateMap<OrderCreateDto, Order>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore()) // Sẽ set khi tạo mới
                    .ForMember(dest => dest.OrderNumber, opt => opt.Ignore()) // Sẽ generate trong service
                    .ForMember(dest => dest.OrderDate, opt => opt.Ignore()) // Sẽ set trong service
                    .ForMember(dest => dest.SubTotal, opt => opt.Ignore()) // Sẽ tính trong service
                    // TaxAmount không có trong Order model - đã comment trong model
                    .ForMember(dest => dest.DiscountAmount, opt => opt.Ignore()) // Sẽ tính trong service
                    .ForMember(dest => dest.TotalAmount, opt => opt.Ignore()) // Sẽ tính trong service
                    .ForMember(dest => dest.Status, opt => opt.Ignore()) // Sẽ set mặc định trong service
                    .ForMember(dest => dest.PaymentStatus, opt => opt.Ignore()) // Sẽ set mặc định trong service
                    .ForMember(dest => dest.TrackingNumber, opt => opt.Ignore())
                    .ForMember(dest => dest.ShippedDate, opt => opt.Ignore())
                    .ForMember(dest => dest.DeliveredDate, opt => opt.Ignore())
                    .ForMember(dest => dest.CancelledAt, opt => opt.Ignore()) // Quản lý trong service
                    .ForMember(dest => dest.CancelReason, opt => opt.Ignore()) // Quản lý trong service
                    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore()) // Sẽ set trong service
                    .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                    .ForMember(dest => dest.OrderDetails, opt => opt.Ignore())
                    .ForMember(dest => dest.User, opt => opt.Ignore())
                    .ForMember(dest => dest.DiscountCodeEntity, opt => opt.Ignore());

                // OrderDetail Mappings
                // OrderDetail không có Id - DB dùng composite PK (OrderId, ProductId)
                // Tính Id cho DTO từ OrderId + ProductId để dùng làm key
                cfg.CreateMap<OrderDetail, OrderDetailDto>()
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.OrderId * 1000000 + src.ProductId)) // Tạo Id từ composite key
                    .ForMember(dest => dest.ProductImage, opt => opt.Ignore()); // Sẽ được set thủ công trong service
                cfg.CreateMap<OrderDetailCreateDto, OrderDetail>()
                    // Không có Id trong OrderDetail model
                    .ForMember(dest => dest.OrderId, opt => opt.Ignore()) // Sẽ set trong service
                    .ForMember(dest => dest.ProductName, opt => opt.Ignore()) // Sẽ set trong service
                    .ForMember(dest => dest.UnitPrice, opt => opt.Ignore()) // Sẽ set trong service
                    .ForMember(dest => dest.Size, opt => opt.Ignore()) // Sẽ set trong service
                    .ForMember(dest => dest.Color, opt => opt.Ignore()) // Sẽ set trong service
                    .ForMember(dest => dest.TotalPrice, opt => opt.Ignore()) // Sẽ tính trong service
                    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore()) // Sẽ set trong service
                    .ForMember(dest => dest.Order, opt => opt.Ignore())
                    .ForMember(dest => dest.Product, opt => opt.Ignore())
                    .ForMember(dest => dest.ProductVariant, opt => opt.Ignore());

                // Cart Mappings
                cfg.CreateMap<Cart, CartDto>()
                    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
                    .ForMember(dest => dest.CartItems, opt => opt.Ignore()) // Sẽ map riêng trong service
                    .ForMember(dest => dest.TotalAmount, opt => opt.Ignore()) // Sẽ tính trong service
                    .ForMember(dest => dest.ShippingFee, opt => opt.Ignore()) // Sẽ tính trong service
                    .ForMember(dest => dest.TaxAmount, opt => opt.Ignore()) // Sẽ tính trong service (không lưu DB)
                    .ForMember(dest => dest.FinalTotal, opt => opt.Ignore()) // Sẽ tính trong service
                    .ForMember(dest => dest.TotalItems, opt => opt.Ignore()); // Sẽ tính trong service

                // CartItem Mappings
                cfg.CreateMap<CartItem, CartItemDto>()
                    .ForMember(dest => dest.AvailableStock, opt => opt.Ignore())
                    .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
                    .ForMember(dest => dest.ProductImage, opt => opt.MapFrom(src => 
                        src.Product != null && src.Product.ProductImages != null
                            ? (src.Product.ProductImages.FirstOrDefault(pi => pi.IsMain) != null 
                                ? src.Product.ProductImages.FirstOrDefault(pi => pi.IsMain).ImageUrl 
                                : string.Empty)
                            : string.Empty))
                    .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.ProductVariant != null ? src.ProductVariant.Size : string.Empty))
                    .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.ProductVariant != null ? src.ProductVariant.ColorName : string.Empty))
                    .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.UnitPrice * src.Quantity))
                    .ForMember(dest => dest.CartId, opt => opt.MapFrom(src => src.CartId))
                    .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                    .ForMember(dest => dest.ProductVariantId, opt => opt.MapFrom(src => src.ProductVariantId));

                cfg.CreateMap<CartItemCreateDto, CartItem>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore()) // Sẽ set khi tạo mới
                    .ForMember(dest => dest.CartId, opt => opt.Ignore()) // Sẽ set trong service
                    .ForMember(dest => dest.UnitPrice, opt => opt.Ignore()) // Sẽ set trong service
                    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore()) // Sẽ set trong service
                    .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                    .ForMember(dest => dest.Cart, opt => opt.Ignore())
                    .ForMember(dest => dest.Product, opt => opt.Ignore())
                    .ForMember(dest => dest.ProductVariant, opt => opt.Ignore());

                // Review Mappings
                cfg.CreateMap<Review, ReviewDto>()
                    .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
                    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
                    .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                    .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));

                cfg.CreateMap<ReviewCreateDto, Review>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore()) // Sẽ set khi tạo mới
                    .ForMember(dest => dest.IsApproved, opt => opt.Ignore()) // Sẽ set mặc định trong service
                    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore()) // Sẽ set trong service
                    .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore()) // default false
                    .ForMember(dest => dest.Product, opt => opt.Ignore())
                    .ForMember(dest => dest.User, opt => opt.Ignore());

                // DiscountCode Mappings
                cfg.CreateMap<DiscountCode, DiscountCodeDto>()
                    .ForMember(dest => dest.IsValid, opt => opt.MapFrom(src => 
                        src.IsActive && 
                        src.StartDate <= DateTime.Now && 
                        src.EndDate >= DateTime.Now &&
                        (src.UsageLimit == null || src.UsedCount < src.UsageLimit)));

                cfg.CreateMap<E_Commerce.Web.ViewModels.DiscountCodeCreateViewModel, DiscountCodeCreateDto>();
                
                cfg.CreateMap<DiscountCodeCreateDto, DiscountCode>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore())
                    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                    .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                    .ForMember(dest => dest.UsedCount, opt => opt.Ignore())
                    .ForMember(dest => dest.IsDeleted, opt => opt.Ignore()) // Sẽ set trong service
                    .ForMember(dest => dest.Orders, opt => opt.Ignore());

                // ProductImage Mappings
                cfg.CreateMap<ProductImage, ProductImageDto>()
                    .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId));
                    
                cfg.CreateMap<ProductImageCreateDto, ProductImage>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore()) // Sẽ set khi tạo mới
                    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore()) // Sẽ set trong service
                    .ForMember(dest => dest.Product, opt => opt.Ignore());

                // ProductVariantImage Mappings
                cfg.CreateMap<ProductVariantImage, ProductVariantImageDto>()
                    .ForMember(dest => dest.ProductVariantId, opt => opt.MapFrom(src => src.ProductVariantId));

                cfg.CreateMap<ProductVariantImageCreateDto, ProductVariantImage>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore()) // Sẽ set khi tạo mới
                    .ForMember(dest => dest.CreatedDate, opt => opt.Ignore()) // Sẽ set trong service
                    .ForMember(dest => dest.ProductVariant, opt => opt.Ignore());

                // Wishlist Mappings
                cfg.CreateMap<Wishlist, WishlistDto>()
                    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
                    .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
                    .ForMember(dest => dest.ProductImage, opt => opt.MapFrom(src => 
                        src.Product != null && src.Product.ProductImages != null
                            ? (src.Product.ProductImages.FirstOrDefault(pi => pi.IsMain) != null 
                                ? src.Product.ProductImages.FirstOrDefault(pi => pi.IsMain).ImageUrl 
                                : string.Empty)
                            : string.Empty))
                    .ForMember(dest => dest.ProductPrice, opt => opt.MapFrom(src => src.Product != null ? src.Product.Price : 0))
                    .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                    .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId));
            });

            // Validate configuration
            try
            {
                config.AssertConfigurationIsValid();
            }
            catch (AutoMapperConfigurationException ex)
            {
                // Log the error details for debugging
                var errorMessage = "AutoMapper Configuration Error:\n" + ex.Message;
                
                // Log detailed errors
                if (ex.Errors != null && ex.Errors.Any())
                {
                    errorMessage += "\n\nDetailed Errors:";
                    foreach (var error in ex.Errors)
                    {
                        errorMessage += $"\n- {error.TypeMap?.SourceType?.Name} -> {error.TypeMap?.DestinationType?.Name}";
                        if (error.UnmappedPropertyNames != null && error.UnmappedPropertyNames.Any())
                        {
                            errorMessage += $"\n  Unmapped Properties: {string.Join(", ", error.UnmappedPropertyNames)}";
                        }
                    }
                }
                
                System.Diagnostics.Debug.WriteLine(errorMessage);
                System.Console.WriteLine(errorMessage);
                
                // Throw exception with detailed message
                throw new Exception(errorMessage, ex);
            }
            
            Mapper = config.CreateMapper();
        }
    }
}

