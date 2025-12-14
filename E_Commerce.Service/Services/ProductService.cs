using AutoMapper;
using E_Commerce.Data.Infrastructure;
using E_Commerce.Data.Repositories;
using E_Commerce.Dto;
using E_Commerce.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace E_Commerce.Service
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICategoryService _categoryService;
        private readonly IBrandRepository _brandRepository;
        private readonly IProductImageRepository _productImageRepository;
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IProductVariantImageRepository _productVariantImageRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductService(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            ICategoryService categoryService,
            IBrandRepository brandRepository,
            IProductImageRepository productImageRepository,
            IProductVariantRepository productVariantRepository,
            IProductVariantImageRepository productVariantImageRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _categoryService = categoryService;
            _brandRepository = brandRepository;
            _productImageRepository = productImageRepository;
            _productVariantRepository = productVariantRepository;
            _productVariantImageRepository = productVariantImageRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public ProductDto Create(ProductCreateDto productCreateDto)
        {
            // Validate category exists
            var category = _categoryRepository.GetSingleById(productCreateDto.CategoryId);
            if (category == null)
            {
                throw new Exception("Danh mục không tồn tại");
            }

            // Validate brand if provided
            if (productCreateDto.BrandId.HasValue)
            {
                var brand = _brandRepository.GetSingleById(productCreateDto.BrandId.Value);
                if (brand == null)
                {
                    throw new Exception("Thương hiệu không tồn tại");
                }
            }

            // Map từ DTO sang Model
            var product = _mapper.Map<ProductCreateDto, Product>(productCreateDto);

            // Set các giá trị mặc định
            product.CreatedDate = DateTime.Now;
            product.IsActive = productCreateDto.IsActive;
            product.StockQuantity = 0; // Ban đầu = 0, sẽ được tính từ variants sau

            // Thêm vào database
            _productRepository.Add(product);
            _unitOfWork.Commit();

            // Map lại sang DTO để trả về
            var productDto = _mapper.Map<Product, ProductDto>(product);
            productDto.StockQuantity = 0; // Ban đầu = 0, sẽ được tính từ variants sau
            return productDto;
        }

        /// <summary>
        /// Thêm ảnh phụ cho sản phẩm
        /// </summary>
        public ProductImageDto AddProductImage(ProductImageCreateDto productImageCreateDto)
        {
            // Validate product exists
            var product = _productRepository.GetSingleById(productImageCreateDto.ProductId);
            if (product == null)
            {
                throw new Exception("Sản phẩm không tồn tại");
            }

            // Map từ DTO sang Model
            var productImage = _mapper.Map<ProductImageCreateDto, ProductImage>(productImageCreateDto);
            productImage.CreatedDate = DateTime.Now;

            // Thêm vào database
            _productImageRepository.Add(productImage);
            _unitOfWork.Commit();

            // Map lại sang DTO để trả về
            return _mapper.Map<ProductImage, ProductImageDto>(productImage);
        }

        /// <summary>
        /// Thêm nhiều ảnh phụ cho sản phẩm
        /// </summary>
        public List<ProductImageDto> AddProductImages(int productId, List<string> imageUrls)
        {
            // Validate product exists
            var product = _productRepository.GetSingleById(productId);
            if (product == null)
            {
                throw new Exception("Sản phẩm không tồn tại");
            }

            var productImages = new List<ProductImage>();
            int displayOrder = 0;

            foreach (var imageUrl in imageUrls)
            {
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    var productImage = new ProductImage
                    {
                        ProductId = productId,
                        ImageUrl = imageUrl,
                        IsMain = false,
                        DisplayOrder = displayOrder++,
                        CreatedDate = DateTime.Now
                    };
                    productImages.Add(productImage);
                }
            }

            if (productImages.Any())
            {
                foreach (var image in productImages)
                {
                    _productImageRepository.Add(image);
                }
                _unitOfWork.Commit();
            }

            return _mapper.Map<List<ProductImage>, List<ProductImageDto>>(productImages);
        }

        /// <summary>
        /// Thêm ảnh chính cho sản phẩm (IsMain = true)
        /// </summary>
        public ProductImageDto AddProductMainImage(int productId, string imageUrl)
        {
            // Validate product exists
            var product = _productRepository.GetSingleById(productId);
            if (product == null)
            {
                throw new Exception("Sản phẩm không tồn tại");
            }

            // Kiểm tra xem đã có ảnh chính chưa, nếu có thì set IsMain = false cho ảnh cũ
            var existingMainImage = _productImageRepository.GetSingleByCondition(pi => pi.ProductId == productId && pi.IsMain);
            if (existingMainImage != null)
            {
                existingMainImage.IsMain = false;
                _productImageRepository.Update(existingMainImage);
            }

            // Tạo ảnh chính mới
            var productImage = new ProductImage
            {
                ProductId = productId,
                ImageUrl = imageUrl,
                IsMain = true,
                DisplayOrder = 0,
                CreatedDate = DateTime.Now
            };

            _productImageRepository.Add(productImage);
            _unitOfWork.Commit();

            return _mapper.Map<ProductImage, ProductImageDto>(productImage);
        }

        /// <summary>
        /// Lấy ảnh chính của sản phẩm (IsMain = true)
        /// </summary>
        public ProductImageDto GetProductMainImage(int productId)
        {
            var mainImage = _productImageRepository.GetSingleByCondition(pi => pi.ProductId == productId && pi.IsMain);
            if (mainImage == null)
            {
                return null;
            }
            return _mapper.Map<ProductImage, ProductImageDto>(mainImage);
        }

        /// <summary>
        /// Lấy tất cả ảnh của sản phẩm
        /// </summary>
        public List<ProductImageDto> GetProductImages(int productId)
        {
            var images = _productImageRepository.GetMulti(pi => pi.ProductId == productId).OrderBy(pi => pi.DisplayOrder).ToList();
            return _mapper.Map<List<ProductImage>, List<ProductImageDto>>(images);
        }

        public ProductDto Update(int id, ProductUpdateDto productUpdateDto)
        {
            var product = _productRepository.GetSingleById(id);
            if (product == null)
            {
                throw new Exception("Sản phẩm không tồn tại");
            }

            // Validate category exists
            if (productUpdateDto.CategoryId != product.CategoryId)
            {
                var category = _categoryRepository.GetSingleById(productUpdateDto.CategoryId);
                if (category == null)
                {
                    throw new Exception("Danh mục không tồn tại");
                }
            }

            // Validate brand if provided
            if (productUpdateDto.BrandId.HasValue && productUpdateDto.BrandId != product.BrandId)
            {
                var brand = _brandRepository.GetSingleById(productUpdateDto.BrandId.Value);
                if (brand == null)
                {
                    throw new Exception("Thương hiệu không tồn tại");
                }
            }

            // Map từ DTO sang Model (giữ nguyên CreatedDate)
            _mapper.Map(productUpdateDto, product);
            product.UpdatedDate = DateTime.Now;

            // Cập nhật vào database
            _productRepository.Update(product);
            _unitOfWork.Commit();

            // Map lại sang DTO để trả về
            var productDto = _mapper.Map<Product, ProductDto>(product);
            // Tính StockQuantity từ tổng Stock của variants
            productDto.StockQuantity = CalculateProductStockQuantity(id);
            return productDto;
        }

        public bool Delete(int id)
        {
            var product = _productRepository.GetSingleById(id);
            if (product == null)
            {
                throw new Exception("Sản phẩm không tồn tại");
            }

            // Soft delete: chỉ set IsDeleted = true (không chạm IsActive)
            product.IsDeleted = true;
            product.UpdatedDate = DateTime.Now;

            _productRepository.Update(product);
            _unitOfWork.Commit();

            return true;
        }

        public ProductDto GetById(int id)
        {
            var product = _productRepository.GetSingleById(id);
            if (product == null)
            {
                return null;
            }

            var productDto = _mapper.Map<Product, ProductDto>(product);
            // Tính StockQuantity từ tổng Stock của variants
            productDto.StockQuantity = CalculateProductStockQuantity(id);
            return productDto;
        }

        public List<ProductDto> GetAll()
        {
            var products = _productRepository.GetMulti(p => !p.IsDeleted).ToList();
            var productDtos = _mapper.Map<List<Product>, List<ProductDto>>(products);
            // Tính StockQuantity từ tổng Stock của variants cho mỗi product
            foreach (var productDto in productDtos)
            {
                productDto.StockQuantity = CalculateProductStockQuantity(productDto.Id);
            }
            return productDtos;
        }

        public List<ProductDto> GetByCategoryId(int categoryId)
        {
            // Lấy tất cả category con (bao gồm cả categoryId chính)
            var categoryIds = _categoryService.GetAllChildCategoryIds(categoryId);
            
            // Lấy tất cả sản phẩm thuộc category và các category con
            var products = _productRepository.GetMulti(p => categoryIds.Contains(p.CategoryId) && !p.IsDeleted).ToList();
            var productDtos = _mapper.Map<List<Product>, List<ProductDto>>(products);
            foreach (var productDto in productDtos)
            {
                productDto.StockQuantity = CalculateProductStockQuantity(productDto.Id);
            }
            return productDtos;
        }

        public List<ProductDto> GetByBrandId(int brandId)
        {
            var products = _productRepository.GetMulti(p => p.BrandId == brandId && !p.IsDeleted).ToList();
            var productDtos = _mapper.Map<List<Product>, List<ProductDto>>(products);
            foreach (var productDto in productDtos)
            {
                productDto.StockQuantity = CalculateProductStockQuantity(productDto.Id);
            }
            return productDtos;
        }

        public List<ProductDto> GetActiveProducts()
        {
            var products = _productRepository.GetMulti(p => p.IsActive && !p.IsDeleted).ToList();
            var productDtos = _mapper.Map<List<Product>, List<ProductDto>>(products);
            foreach (var productDto in productDtos)
            {
                productDto.StockQuantity = CalculateProductStockQuantity(productDto.Id);
            }
            return productDtos;
        }

        public List<ProductDto> GetFeaturedProducts()
        {
            var products = _productRepository.GetMulti(p => p.IsActive && p.IsFeatured && !p.IsDeleted).ToList();
            var productDtos = _mapper.Map<List<Product>, List<ProductDto>>(products);
            foreach (var productDto in productDtos)
            {
                productDto.StockQuantity = CalculateProductStockQuantity(productDto.Id);
            }
            return productDtos;
        }

        public List<ProductDto> GetOnSaleProducts()
        {
            var products = _productRepository.GetMulti(p => p.IsActive && p.IsOnSale && !p.IsDeleted).ToList();
            var productDtos = _mapper.Map<List<Product>, List<ProductDto>>(products);
            foreach (var productDto in productDtos)
            {
                productDto.StockQuantity = CalculateProductStockQuantity(productDto.Id);
            }
            return productDtos;
        }

        public List<ProductDto> SearchProducts(string searchTerm = null, bool? isActive = null, int? categoryId = null, int? brandId = null, bool? isFeatured = null, bool? isOnSale = null, string sortBy = "name", string sortOrder = "asc")
        {
            // Bắt đầu với tất cả products
            var query = _productRepository.GetAll().AsQueryable();

            // Mặc định ẩn sản phẩm đã xóa mềm
            query = query.Where(p => !p.IsDeleted);

            // Lọc theo từ khóa tìm kiếm (tên, SKU, mô tả)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string search = searchTerm.Trim();
                query = query.Where(p => p.Name.Contains(search) ||
                                         (p.SKU != null && p.SKU.Contains(search)) ||
                                         (p.Description != null && p.Description.Contains(search)));
            }

            // Lọc theo trạng thái
            if (isActive.HasValue)
            {
                query = query.Where(p => p.IsActive == isActive.Value);
            }

            // Lọc theo danh mục (bao gồm cả category con)
            if (categoryId.HasValue)
            {
                // Lấy tất cả category con (bao gồm cả categoryId chính)
                var categoryIds = _categoryService.GetAllChildCategoryIds(categoryId.Value);
                query = query.Where(p => categoryIds.Contains(p.CategoryId));
            }

            // Lọc theo thương hiệu
            if (brandId.HasValue)
            {
                query = query.Where(p => p.BrandId == brandId.Value);
            }

            // Lọc sản phẩm nổi bật
            if (isFeatured.HasValue)
            {
                query = query.Where(p => p.IsFeatured == isFeatured.Value);
            }

            // Lọc sản phẩm đang giảm giá
            if (isOnSale.HasValue)
            {
                query = query.Where(p => p.IsOnSale == isOnSale.Value);
            }

            // Sắp xếp
            switch (sortBy.ToLower())
            {
                case "price":
                    query = sortOrder.ToLower() == "desc"
                        ? query.OrderByDescending(p => p.Price)
                        : query.OrderBy(p => p.Price);
                    break;
                case "createddate":
                    query = sortOrder.ToLower() == "desc"
                        ? query.OrderByDescending(p => p.CreatedDate)
                        : query.OrderBy(p => p.CreatedDate);
                    break;
                case "stockquantity":
                    // StockQuantity được tính từ variants, nên cần sort sau khi map
                    query = sortOrder.ToLower() == "desc"
                        ? query.OrderByDescending(p => p.StockQuantity)
                        : query.OrderBy(p => p.StockQuantity);
                    break;
                case "name":
                default:
                    query = sortOrder.ToLower() == "desc"
                        ? query.OrderByDescending(p => p.Name)
                        : query.OrderBy(p => p.Name);
                    break;
            }

            var products = query.ToList();
            var productDtos = _mapper.Map<List<Product>, List<ProductDto>>(products);
            
            // Tính StockQuantity từ tổng Stock của variants cho mỗi product
            foreach (var productDto in productDtos)
            {
                productDto.StockQuantity = CalculateProductStockQuantity(productDto.Id);
            }

            // Nếu sort theo stockQuantity, cần sort lại sau khi tính toán
            if (sortBy.ToLower() == "stockquantity")
            {
                if (sortOrder.ToLower() == "desc")
                {
                    productDtos = productDtos.OrderByDescending(p => p.StockQuantity).ToList();
                }
                else
                {
                    productDtos = productDtos.OrderBy(p => p.StockQuantity).ToList();
                }
            }

            return productDtos;
        }

        /// <summary>
        /// Tạo biến thể sản phẩm (màu, size, giá, stock)
        /// Nếu đã có variant cùng màu (cùng ColorName/ColorCode và Pattern), sẽ tự động copy ảnh từ variant đó
        /// </summary>
        public ProductVariantDto CreateVariant(ProductVariantCreateDto variantCreateDto)
        {
            // Validate product exists
            var product = _productRepository.GetSingleById(variantCreateDto.ProductId);
            if (product == null)
            {
                throw new Exception("Sản phẩm không tồn tại");
            }

            // Map từ DTO sang Model
            var variant = _mapper.Map<ProductVariantCreateDto, ProductVariant>(variantCreateDto);
            variant.CreatedDate = DateTime.Now;
            variant.IsActive = variantCreateDto.IsActive;

            // Thêm vào database
            _productVariantRepository.Add(variant);
            _unitOfWork.Commit();

            // Nếu đã có variant khác cùng màu (cùng ColorName/ColorCode và Pattern), copy ảnh từ variant đó
            if (!string.IsNullOrWhiteSpace(variantCreateDto.ColorName) || !string.IsNullOrWhiteSpace(variantCreateDto.ColorCode))
            {
                // Lưu giá trị vào biến để tránh lỗi LINQ to Entities
                string colorName = variantCreateDto.ColorName;
                string colorCode = variantCreateDto.ColorCode;
                string pattern = variantCreateDto.Pattern;
                
                var existingVariantWithSameColor = _productVariantRepository.GetMulti(
                    v => v.ProductId == variantCreateDto.ProductId 
                    && v.Id != variant.Id // Không phải chính nó
                    && (
                        (colorName != null && colorName.Trim() != "" && v.ColorName == colorName)
                        || (colorCode != null && colorCode.Trim() != "" && v.ColorCode == colorCode)
                    )
                    && v.Pattern == pattern // Cùng pattern
                ).FirstOrDefault();

                if (existingVariantWithSameColor != null)
                {
                    // Lấy tất cả ảnh của variant cùng màu
                    var existingImages = _productVariantImageRepository.GetMulti(
                        vi => vi.ProductVariantId == existingVariantWithSameColor.Id
                    ).ToList();

                    // Copy ảnh sang variant mới
                    foreach (var existingImage in existingImages)
                    {
                        var newImage = new ProductVariantImage
                        {
                            ProductVariantId = variant.Id,
                            ImageUrl = existingImage.ImageUrl,
                            IsMain = existingImage.IsMain,
                            DisplayOrder = existingImage.DisplayOrder,
                            CreatedDate = DateTime.Now
                        };
                        _productVariantImageRepository.Add(newImage);
                    }
                    _unitOfWork.Commit();
                }
            }

            // Cập nhật StockQuantity của Product = tổng Stock của tất cả variants
            UpdateProductStockQuantity(variantCreateDto.ProductId);

            // Map lại sang DTO để trả về
            var variantDto = _mapper.Map<ProductVariant, ProductVariantDto>(variant);
            variantDto.ProductName = product.Name;
            return variantDto;
        }

        /// <summary>
        /// Tính StockQuantity của Product từ tổng Stock của tất cả variants (chỉ tính variants đang active)
        /// </summary>
        private int CalculateProductStockQuantity(int productId)
        {
            return _productVariantRepository.GetMulti(
                v => v.ProductId == productId && v.IsActive
            ).Sum(v => v.Stock);
        }

        /// <summary>
        /// Cập nhật StockQuantity của Product = tổng Stock của tất cả variants
        /// </summary>
        private void UpdateProductStockQuantity(int productId)
        {
            var product = _productRepository.GetSingleById(productId);
            if (product == null)
            {
                return;
            }

            // Tính tổng Stock của tất cả variants đang active
            var totalStock = CalculateProductStockQuantity(productId);

            product.StockQuantity = totalStock;
            product.UpdatedDate = DateTime.Now;
            _productRepository.Update(product);
            _unitOfWork.Commit();
        }

        /// <summary>
        /// Tạo nhiều biến thể sản phẩm cùng lúc
        /// Các variant cùng màu (cùng ColorName/ColorCode và Pattern) sẽ tự động dùng chung ảnh
        /// Variant đầu tiên của mỗi màu sẽ có ảnh riêng, các variant sau (cùng màu) sẽ copy ảnh từ variant đầu tiên
        /// </summary>
        public List<ProductVariantDto> CreateVariants(int productId, List<ProductVariantCreateDto> variantCreateDtos)
        {
            // Validate product exists
            var product = _productRepository.GetSingleById(productId);
            if (product == null)
            {
                throw new Exception("Sản phẩm không tồn tại");
            }

            var variants = new List<ProductVariant>();
            var colorGroups = new Dictionary<string, ProductVariant>(); // Key: ColorName_Pattern hoặc ColorCode_Pattern, Value: Variant đầu tiên của màu đó

            foreach (var variantDto in variantCreateDtos)
            {
                variantDto.ProductId = productId;
                var variant = _mapper.Map<ProductVariantCreateDto, ProductVariant>(variantDto);
                variant.CreatedDate = DateTime.Now;
                variant.IsActive = variantDto.IsActive;
                variants.Add(variant);
            }

            if (variants.Any())
            {
                // Thêm tất cả variants vào database trước
                foreach (var variant in variants)
                {
                    _productVariantRepository.Add(variant);
                }
                _unitOfWork.Commit();

                // Nhóm variants theo màu và copy ảnh
                foreach (var variant in variants)
                {
                    // Tạo key để nhóm theo màu (ColorName hoặc ColorCode) và Pattern
                    string colorKey = null;
                    if (!string.IsNullOrWhiteSpace(variant.ColorName))
                    {
                        colorKey = $"{variant.ColorName}_{variant.Pattern ?? ""}";
                    }
                    else if (!string.IsNullOrWhiteSpace(variant.ColorCode))
                    {
                        colorKey = $"{variant.ColorCode}_{variant.Pattern ?? ""}";
                    }

                    if (!string.IsNullOrWhiteSpace(colorKey))
                    {
                        if (!colorGroups.ContainsKey(colorKey))
                        {
                            // Variant đầu tiên của màu này - sẽ có ảnh riêng (sẽ được upload sau trong Controller)
                            colorGroups[colorKey] = variant;
                        }
                        else
                        {
                            // Variant sau của cùng màu - copy ảnh từ variant đầu tiên
                            var firstVariantOfColor = colorGroups[colorKey];
                            var existingImages = _productVariantImageRepository.GetMulti(
                                vi => vi.ProductVariantId == firstVariantOfColor.Id
                            ).ToList();

                            // Copy ảnh sang variant mới
                            foreach (var existingImage in existingImages)
                            {
                                var newImage = new ProductVariantImage
                                {
                                    ProductVariantId = variant.Id,
                                    ImageUrl = existingImage.ImageUrl,
                                    IsMain = existingImage.IsMain,
                                    DisplayOrder = existingImage.DisplayOrder,
                                    CreatedDate = DateTime.Now
                                };
                                _productVariantImageRepository.Add(newImage);
                            }
                        }
                    }
                }

                // Commit nếu có ảnh được copy
                var hasImagesToCopy = colorGroups.Values.Any(v => 
                {
                    var images = _productVariantImageRepository.GetMulti(vi => vi.ProductVariantId == v.Id).ToList();
                    return images.Any();
                });

                if (hasImagesToCopy)
                {
                    _unitOfWork.Commit();
                }
            }

            // Cập nhật StockQuantity của Product = tổng Stock của tất cả variants
            UpdateProductStockQuantity(productId);

            var result = _mapper.Map<List<ProductVariant>, List<ProductVariantDto>>(variants);
            foreach (var variantDto in result)
            {
                variantDto.ProductName = product.Name;
            }
            return result;
        }

        /// <summary>
        /// Lấy tất cả variants của một sản phẩm
        /// </summary>
        public List<ProductVariantDto> GetVariantsByProductId(int productId)
        {
            var variants = _productVariantRepository.GetMulti(v => v.ProductId == productId && !v.IsDeleted, new[] { "Product" }).ToList();
            return _mapper.Map<List<ProductVariant>, List<ProductVariantDto>>(variants);
        }

        /// <summary>
        /// Lấy variant theo ID
        /// </summary>
        public ProductVariantDto GetVariantById(int id)
        {
            var variant = _productVariantRepository.GetSingleById(id);
            if (variant == null)
            {
                return null;
            }
            return _mapper.Map<ProductVariant, ProductVariantDto>(variant);
        }

        /// <summary>
        /// Cập nhật variant
        /// </summary>
        public ProductVariantDto UpdateVariant(int id, ProductVariantUpdateDto variantUpdateDto)
        {
            var variant = _productVariantRepository.GetSingleById(id);
            if (variant == null)
            {
                throw new Exception("Biến thể sản phẩm không tồn tại");
            }

            // Map từ DTO sang Model (giữ nguyên ProductId và CreatedDate)
            _mapper.Map(variantUpdateDto, variant);
            variant.UpdatedDate = DateTime.Now;

            _productVariantRepository.Update(variant);
            _unitOfWork.Commit();

            // Cập nhật StockQuantity của Product = tổng Stock của tất cả variants
            UpdateProductStockQuantity(variant.ProductId);

            return _mapper.Map<ProductVariant, ProductVariantDto>(variant);
        }

        /// <summary>
        /// Xóa variant
        /// </summary>
        public bool DeleteVariant(int id)
        {
            var variant = _productVariantRepository.GetSingleById(id);
            if (variant == null)
            {
                throw new Exception("Biến thể sản phẩm không tồn tại");
            }

            // Kiểm tra xem variant có đang được sử dụng trong Cart hoặc Order không
            // (Có thể thêm validation sau nếu cần)

            var productId = variant.ProductId; // Lưu ProductId trước khi "xóa"

            // Soft delete: set IsDeleted = true và cập nhật UpdatedDate (không động IsActive)
            variant.IsDeleted = true;
            variant.UpdatedDate = DateTime.Now;
            _productVariantRepository.Update(variant);
            _unitOfWork.Commit();

            // Cập nhật StockQuantity của Product = tổng Stock của tất cả variants
            UpdateProductStockQuantity(productId);

            return true;
        }

        /// <summary>
        /// Thêm ảnh cho variant
        /// </summary>
        public ProductVariantImageDto AddVariantImage(ProductVariantImageCreateDto variantImageCreateDto)
        {
            // Validate variant exists
            var variant = _productVariantRepository.GetSingleById(variantImageCreateDto.ProductVariantId);
            if (variant == null)
            {
                throw new Exception("Biến thể sản phẩm không tồn tại");
            }

            // Map từ DTO sang Model
            var variantImage = _mapper.Map<ProductVariantImageCreateDto, ProductVariantImage>(variantImageCreateDto);
            variantImage.CreatedDate = DateTime.Now;

            // Nếu đây là ảnh chính, set IsMain = false cho ảnh chính cũ
            if (variantImage.IsMain)
            {
                var existingMainImage = _productVariantImageRepository.GetSingleByCondition(vi => vi.ProductVariantId == variantImage.ProductVariantId && vi.IsMain);
                if (existingMainImage != null)
                {
                    existingMainImage.IsMain = false;
                    _productVariantImageRepository.Update(existingMainImage);
                }
            }

            _productVariantImageRepository.Add(variantImage);
            _unitOfWork.Commit();

            return _mapper.Map<ProductVariantImage, ProductVariantImageDto>(variantImage);
        }

        /// <summary>
        /// Thêm nhiều ảnh cho variant
        /// </summary>
        public List<ProductVariantImageDto> AddVariantImages(int variantId, List<string> imageUrls)
        {
            // Validate variant exists
            var variant = _productVariantRepository.GetSingleById(variantId);
            if (variant == null)
            {
                throw new Exception("Biến thể sản phẩm không tồn tại");
            }

            var variantImages = new List<ProductVariantImage>();
            int displayOrder = 0;

            foreach (var imageUrl in imageUrls)
            {
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    var variantImage = new ProductVariantImage
                    {
                        ProductVariantId = variantId,
                        ImageUrl = imageUrl,
                        IsMain = false, // Ảnh phụ, ảnh chính sẽ được set riêng
                        DisplayOrder = displayOrder++,
                        CreatedDate = DateTime.Now
                    };
                    variantImages.Add(variantImage);
                }
            }

            if (variantImages.Any())
            {
                foreach (var image in variantImages)
                {
                    _productVariantImageRepository.Add(image);
                }
                _unitOfWork.Commit();
            }

            return _mapper.Map<List<ProductVariantImage>, List<ProductVariantImageDto>>(variantImages);
        }

        /// <summary>
        /// Thêm ảnh chính cho variant (IsMain = true)
        /// </summary>
        public ProductVariantImageDto AddVariantMainImage(int variantId, string imageUrl)
        {
            // Validate variant exists
            var variant = _productVariantRepository.GetSingleById(variantId);
            if (variant == null)
            {
                throw new Exception("Biến thể sản phẩm không tồn tại");
            }

            // Kiểm tra xem đã có ảnh chính chưa, nếu có thì set IsMain = false cho ảnh cũ
            var existingMainImage = _productVariantImageRepository.GetSingleByCondition(vi => vi.ProductVariantId == variantId && vi.IsMain);
            if (existingMainImage != null)
            {
                existingMainImage.IsMain = false;
                _productVariantImageRepository.Update(existingMainImage);
            }

            // Tạo ảnh chính mới
            var variantImage = new ProductVariantImage
            {
                ProductVariantId = variantId,
                ImageUrl = imageUrl,
                IsMain = true,
                DisplayOrder = 0,
                CreatedDate = DateTime.Now
            };

            _productVariantImageRepository.Add(variantImage);
            _unitOfWork.Commit();

            return _mapper.Map<ProductVariantImage, ProductVariantImageDto>(variantImage);
        }

        /// <summary>
        /// Lấy tất cả ảnh của variant
        /// </summary>
        public List<ProductVariantImageDto> GetVariantImages(int variantId)
        {
            var images = _productVariantImageRepository.GetMulti(vi => vi.ProductVariantId == variantId).OrderBy(vi => vi.DisplayOrder).ToList();
            return _mapper.Map<List<ProductVariantImage>, List<ProductVariantImageDto>>(images);
        }

        /// <summary>
        /// Xóa ảnh variant
        /// </summary>
        public bool DeleteVariantImage(int imageId)
        {
            var variantImage = _productVariantImageRepository.GetSingleById(imageId);
            if (variantImage == null)
            {
                throw new Exception("Ảnh biến thể không tồn tại");
            }

            _productVariantImageRepository.Delete(variantImage);
            _unitOfWork.Commit();

            return true;
        }

        /// <summary>
        /// Copy ảnh từ Product sang Variant
        /// </summary>
        public List<ProductVariantImageDto> CopyProductImagesToVariant(int productId, int variantId)
        {
            // Validate product và variant
            var product = _productRepository.GetSingleById(productId);
            if (product == null)
            {
                throw new Exception("Sản phẩm không tồn tại");
            }

            var variant = _productVariantRepository.GetSingleById(variantId);
            if (variant == null)
            {
                throw new Exception("Biến thể sản phẩm không tồn tại");
            }

            // Lấy tất cả ảnh của product
            var productImages = _productImageRepository.GetMulti(pi => pi.ProductId == productId).OrderBy(pi => pi.IsMain ? 0 : 1).ThenBy(pi => pi.DisplayOrder).ToList();
            
            if (!productImages.Any())
            {
                throw new Exception("Sản phẩm chưa có ảnh để copy");
            }

            // Copy ảnh sang variant
            var variantImages = new List<ProductVariantImage>();
            int displayOrder = 0;

            foreach (var productImage in productImages)
            {
                var variantImage = new ProductVariantImage
                {
                    ProductVariantId = variantId,
                    ImageUrl = productImage.ImageUrl,
                    IsMain = productImage.IsMain, // Giữ nguyên IsMain từ product
                    DisplayOrder = displayOrder++,
                    CreatedDate = DateTime.Now
                };
                variantImages.Add(variantImage);
            }

            // Đảm bảo chỉ có 1 ảnh chính
            var mainImages = variantImages.Where(vi => vi.IsMain).ToList();
            if (mainImages.Count > 1)
            {
                // Chỉ giữ ảnh đầu tiên là main, các ảnh khác set IsMain = false
                for (int i = 1; i < mainImages.Count; i++)
                {
                    mainImages[i].IsMain = false;
                }
            }
            else if (mainImages.Count == 0 && variantImages.Any())
            {
                // Nếu không có ảnh main, set ảnh đầu tiên là main
                variantImages[0].IsMain = true;
            }

            // Lưu vào database
            foreach (var image in variantImages)
            {
                _productVariantImageRepository.Add(image);
            }
            _unitOfWork.Commit();

            return _mapper.Map<List<ProductVariantImage>, List<ProductVariantImageDto>>(variantImages);
        }
    }
}

