using E_Commerce.Dto;
using System.Collections.Generic;

namespace E_Commerce.Service
{
    public interface IProductService
    {
        /// <summary>
        /// Tạo mới sản phẩm
        /// </summary>
        ProductDto Create(ProductCreateDto productCreateDto);

        /// <summary>
        /// Cập nhật sản phẩm
        /// </summary>
        ProductDto Update(int id, ProductUpdateDto productUpdateDto);

        /// <summary>
        /// Xóa sản phẩm (soft delete)
        /// </summary>
        bool Delete(int id);

        /// <summary>
        /// Lấy sản phẩm theo ID
        /// </summary>
        ProductDto GetById(int id);

        /// <summary>
        /// Lấy tất cả sản phẩm
        /// </summary>
        List<ProductDto> GetAll();

        /// <summary>
        /// Lấy sản phẩm theo danh mục
        /// </summary>
        List<ProductDto> GetByCategoryId(int categoryId);

        /// <summary>
        /// Lấy sản phẩm theo thương hiệu
        /// </summary>
        List<ProductDto> GetByBrandId(int brandId);

        /// <summary>
        /// Lấy sản phẩm đang kích hoạt
        /// </summary>
        List<ProductDto> GetActiveProducts();

        /// <summary>
        /// Lấy sản phẩm nổi bật
        /// </summary>
        List<ProductDto> GetFeaturedProducts();

        /// <summary>
        /// Lấy sản phẩm đang giảm giá
        /// </summary>
        List<ProductDto> GetOnSaleProducts();

        /// <summary>
        /// Thêm ảnh phụ cho sản phẩm
        /// </summary>
        ProductImageDto AddProductImage(ProductImageCreateDto productImageCreateDto);

        /// <summary>
        /// Thêm nhiều ảnh phụ cho sản phẩm
        /// </summary>
        List<ProductImageDto> AddProductImages(int productId, List<string> imageUrls);

        /// <summary>
        /// Thêm ảnh chính cho sản phẩm (IsMain = true)
        /// </summary>
        ProductImageDto AddProductMainImage(int productId, string imageUrl);

        /// <summary>
        /// Lấy ảnh chính của sản phẩm (IsMain = true)
        /// </summary>
        ProductImageDto GetProductMainImage(int productId);

        /// <summary>
        /// Lấy tất cả ảnh của sản phẩm
        /// </summary>
        List<ProductImageDto> GetProductImages(int productId);

        /// <summary>
        /// Tạo biến thể sản phẩm (màu, size, giá, stock)
        /// </summary>
        ProductVariantDto CreateVariant(ProductVariantCreateDto variantCreateDto);

        /// <summary>
        /// Tạo nhiều biến thể sản phẩm cùng lúc
        /// </summary>
        List<ProductVariantDto> CreateVariants(int productId, List<ProductVariantCreateDto> variantCreateDtos);

        /// <summary>
        /// Lấy tất cả variants của một sản phẩm
        /// </summary>
        List<ProductVariantDto> GetVariantsByProductId(int productId);

        /// <summary>
        /// Lấy variant theo ID
        /// </summary>
        ProductVariantDto GetVariantById(int id);

        /// <summary>
        /// Cập nhật variant
        /// </summary>
        ProductVariantDto UpdateVariant(int id, ProductVariantUpdateDto variantUpdateDto);

        /// <summary>
        /// Xóa variant
        /// </summary>
        bool DeleteVariant(int id);

        /// <summary>
        /// Thêm ảnh cho variant
        /// </summary>
        ProductVariantImageDto AddVariantImage(ProductVariantImageCreateDto variantImageCreateDto);

        /// <summary>
        /// Thêm nhiều ảnh cho variant
        /// </summary>
        List<ProductVariantImageDto> AddVariantImages(int variantId, List<string> imageUrls);

        /// <summary>
        /// Thêm ảnh chính cho variant (IsMain = true)
        /// </summary>
        ProductVariantImageDto AddVariantMainImage(int variantId, string imageUrl);

        /// <summary>
        /// Lấy tất cả ảnh của variant
        /// </summary>
        List<ProductVariantImageDto> GetVariantImages(int variantId);

        /// <summary>
        /// Xóa ảnh variant
        /// </summary>
        bool DeleteVariantImage(int imageId);

        /// <summary>
        /// Copy ảnh từ Product sang Variant
        /// </summary>
        List<ProductVariantImageDto> CopyProductImagesToVariant(int productId, int variantId);

        /// <summary>
        /// Tìm kiếm và lọc sản phẩm
        /// </summary>
        /// <param name="searchTerm">Từ khóa tìm kiếm (tên, SKU, mô tả)</param>
        /// <param name="isActive">Lọc theo trạng thái (null = tất cả, true = kích hoạt, false = tắt)</param>
        /// <param name="categoryId">Lọc theo danh mục (null = tất cả)</param>
        /// <param name="brandId">Lọc theo thương hiệu (null = tất cả)</param>
        /// <param name="isFeatured">Lọc sản phẩm nổi bật (null = tất cả)</param>
        /// <param name="isOnSale">Lọc sản phẩm đang giảm giá (null = tất cả)</param>
        /// <param name="sortBy">Sắp xếp theo (name, price, createdDate, stockQuantity)</param>
        /// <param name="sortOrder">Thứ tự sắp xếp (asc, desc)</param>
        /// <returns>Danh sách sản phẩm đã lọc</returns>
        List<ProductDto> SearchProducts(string searchTerm = null, bool? isActive = null, int? categoryId = null, int? brandId = null, bool? isFeatured = null, bool? isOnSale = null, string sortBy = "name", string sortOrder = "asc");
    }
}

