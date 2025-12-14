using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using E_Commerce.Service;
using E_Commerce.Dto;

namespace E_Commerce.Web.Areas.User.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IBrandService _brandService;
        private readonly IReviewService _reviewService;
        private readonly IWishlistService _wishlistService;

        public ProductController(
            IProductService productService,
            ICategoryService categoryService,
            IBrandService brandService,
            IReviewService reviewService,
            IWishlistService wishlistService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _brandService = brandService;
            _reviewService = reviewService;
            _wishlistService = wishlistService;
        }

        private int? GetCurrentUserId()
        {
            return Session["UserId"] as int?;
        }

        // GET: User/Product/Index
        public ActionResult Index(int? categoryId, int? brandId, string searchTerm = null, 
            decimal? minPrice = null, decimal? maxPrice = null, 
            string sortBy = "name", string sortOrder = "asc", int? page = null, int? pageSize = null)
        {
            // Lấy danh sách sản phẩm với filter
            var products = _productService.SearchProducts(
                searchTerm: searchTerm,
                isActive: true, // Chỉ hiển thị sản phẩm đang kích hoạt
                categoryId: categoryId,
                brandId: brandId,
                isFeatured: null,
                isOnSale: null,
                sortBy: sortBy,
                sortOrder: sortOrder
            );

            // Lọc theo giá nếu có
            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice.Value).ToList();
            }
            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice.Value).ToList();
            }

            // Lấy ảnh chính và ảnh phụ cho mỗi sản phẩm
            var productMainImages = new Dictionary<int, string>();
            var productHoverImages = new Dictionary<int, string>();
            foreach (var product in products)
            {
                var allImages = _productService.GetProductImages(product.Id);
                var mainImage = allImages.FirstOrDefault(img => img.IsMain) ?? allImages.FirstOrDefault();
                if (mainImage != null)
                {
                    productMainImages[product.Id] = mainImage.ImageUrl;
                }
                
                // Lấy ảnh phụ (ảnh thứ 2) để làm hover-image
                if (allImages.Count > 1)
                {
                    var hoverImage = allImages.Where(img => !img.IsMain).OrderBy(img => img.DisplayOrder).FirstOrDefault() 
                                    ?? allImages.Skip(1).FirstOrDefault();
                    if (hoverImage != null)
                    {
                        productHoverImages[product.Id] = hoverImage.ImageUrl;
                    }
                }
            }

            // Lấy thống kê đánh giá cho mỗi sản phẩm
            var productRatings = new Dictionary<int, double>();
            var productReviewCounts = new Dictionary<int, int>();
            foreach (var product in products)
            {
                var stats = _reviewService.GetProductReviewStatistics(product.Id);
                productRatings[product.Id] = stats.AverageRating;
                productReviewCounts[product.Id] = stats.TotalReviews;
            }

            // Phân trang
            var currentPage = page ?? 1;
            var currentPageSize = pageSize ?? 9;
            var totalProducts = products.Count;
            var totalPages = (int)Math.Ceiling((double)totalProducts / currentPageSize);
            var pagedProducts = products.Skip((currentPage - 1) * currentPageSize).Take(currentPageSize).ToList();

            // Lấy danh sách categories và brands cho sidebar
            var rootCategories = _categoryService.GetRootCategories();
            var allCategories = _categoryService.GetActiveCategories();
            var brands = _brandService.GetAll().Where(b => b.IsActive).ToList();
            
            // Lấy giá max để làm max cho range slider
            var allActiveProducts = _productService.GetActiveProducts();
            var maxProductPrice = allActiveProducts.Any() ? (int)allActiveProducts.Max(p => p.Price) : 10000000;

            // Đếm số sản phẩm trong mỗi category
            var categoryProductCounts = new Dictionary<int, int>();
            foreach (var category in rootCategories)
            {
                var categoryProducts = _productService.GetByCategoryId(category.Id);
                categoryProductCounts[category.Id] = categoryProducts.Count(p => p.IsActive);
                
                // Đếm cả subcategories
                var subCategories = allCategories.Where(c => c.ParentCategoryId == category.Id).ToList();
                foreach (var subCat in subCategories)
                {
                    var subProducts = _productService.GetByCategoryId(subCat.Id);
                    categoryProductCounts[category.Id] += subProducts.Count(p => p.IsActive);
                }
            }

            // Đếm số sản phẩm cho mỗi brand
            var brandProductCounts = new Dictionary<int, int>();
            foreach (var brand in brands)
            {
                var brandProducts = _productService.GetByBrandId(brand.Id);
                brandProductCounts[brand.Id] = brandProducts.Count(p => p.IsActive);
            }

            ViewBag.Products = pagedProducts;
            ViewBag.ProductMainImages = productMainImages;
            ViewBag.ProductHoverImages = productHoverImages;
            ViewBag.ProductRatings = productRatings;
            ViewBag.ProductReviewCounts = productReviewCounts;
            ViewBag.RootCategories = rootCategories;
            ViewBag.AllCategories = allCategories;
            ViewBag.Brands = brands;
            ViewBag.CategoryProductCounts = categoryProductCounts;
            ViewBag.BrandProductCounts = brandProductCounts;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SelectedBrandId = brandId;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;
            ViewBag.CurrentPage = currentPage;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.PageSize = currentPageSize;
            ViewBag.MaxProductPrice = maxProductPrice;

            // Lấy danh sách product IDs trong wishlist của user hiện tại
            var wishlistProductIds = new HashSet<int>();
            var userId = GetCurrentUserId();
            if (userId.HasValue)
            {
                wishlistProductIds = new HashSet<int>(_wishlistService.GetWishlistProductIds(userId.Value));
            }
            ViewBag.WishlistProductIds = wishlistProductIds;

            return View();
        }

        // GET: User/Product/Suggest?term=abc
        [HttpGet]
        public JsonResult Suggest(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return Json(new { success = false, items = new object[0] }, JsonRequestBehavior.AllowGet);
            }

            var products = _productService.SearchProducts(
                searchTerm: term,
                isActive: true,
                categoryId: null,
                brandId: null,
                isFeatured: null,
                isOnSale: null,
                sortBy: "name",
                sortOrder: "asc"
            )
            .Where(p => !p.IsDeleted)
            .Take(6)
            .ToList();

            var items = products.Select(p =>
            {
                var imgs = _productService.GetProductImages(p.Id);
                var main = imgs.FirstOrDefault(i => i.IsMain) ?? imgs.FirstOrDefault();
                var imageUrl = main != null ? main.ImageUrl : Url.Content("~/Content/images/default-product.png");
                var url = Url.Action("Details", "Product", new { area = "User", id = p.Id });

                return new
                {
                    id = p.Id,
                    name = p.Name,
                    price = p.Price,
                    image = imageUrl,
                    url = url
                };
            }).ToList();

            return Json(new { success = true, items = items }, JsonRequestBehavior.AllowGet);
        }

        // GET: User/Product/Details
        public ActionResult Details(int id)
        {
            var product = _productService.GetById(id);
            
            if (product == null || !product.IsActive)
            {
                return HttpNotFound();
            }

            // Lấy tất cả ảnh của sản phẩm
            var productImages = _productService.GetProductImages(product.Id);
            var mainImage = productImages.FirstOrDefault(img => img.IsMain) ?? productImages.FirstOrDefault();
            
            // Lấy tất cả variants của sản phẩm
            var variants = _productService.GetVariantsByProductId(product.Id).Where(v => v.IsActive).ToList();
            
            // Lấy ảnh cho mỗi variant
            var variantImages = new Dictionary<int, List<ProductVariantImageDto>>();
            foreach (var variant in variants)
            {
                var images = _productService.GetVariantImages(variant.Id);
                variantImages[variant.Id] = images;
            }

            // Nhóm variants theo màu
            var variantsByColor = variants
                .GroupBy(v => v.ColorName ?? "Default")
                .ToDictionary(g => g.Key, g => g.ToList());

            // Lấy sản phẩm liên quan (ưu tiên cùng category, sau đó cùng brand)
            var relatedProducts = new List<ProductDto>();
            var maxRelatedProducts = 8; // Hiển thị tối đa 8 sản phẩm
            var addedProductIds = new HashSet<int> { product.Id }; // Loại trừ sản phẩm hiện tại

            // Bước 1: Lấy sản phẩm cùng category (ưu tiên)
            if (product.CategoryId > 0)
            {
                var categoryProducts = _productService.GetByCategoryId(product.CategoryId)
                    .Where(p => p.IsActive && !addedProductIds.Contains(p.Id))
                    .OrderByDescending(p => p.IsFeatured) // Ưu tiên sản phẩm nổi bật
                    .ThenByDescending(p => p.CreatedDate) // Sau đó ưu tiên sản phẩm mới
                    .Take(maxRelatedProducts)
                    .ToList();

                foreach (var catProduct in categoryProducts)
                {
                    relatedProducts.Add(catProduct);
                    addedProductIds.Add(catProduct.Id);
                }
            }

            // Bước 2: Nếu chưa đủ, lấy thêm sản phẩm cùng brand
            if (relatedProducts.Count < maxRelatedProducts && product.BrandId.HasValue && product.BrandId.Value > 0)
            {
                var brandProducts = _productService.GetByBrandId(product.BrandId.Value)
                    .Where(p => p.IsActive && !addedProductIds.Contains(p.Id))
                    .OrderByDescending(p => p.IsFeatured)
                    .ThenByDescending(p => p.CreatedDate)
                    .Take(maxRelatedProducts - relatedProducts.Count)
                    .ToList();

                foreach (var brandProduct in brandProducts)
                {
                    relatedProducts.Add(brandProduct);
                    addedProductIds.Add(brandProduct.Id);
                }
            }

            // Bước 3: Nếu vẫn chưa đủ, lấy thêm sản phẩm nổi bật (featured)
            if (relatedProducts.Count < maxRelatedProducts)
            {
                var featuredProducts = _productService.SearchProducts(
                    isActive: true,
                    isFeatured: true,
                    sortBy: "createdDate",
                    sortOrder: "desc"
                )
                .Where(p => !addedProductIds.Contains(p.Id))
                .Take(maxRelatedProducts - relatedProducts.Count)
                .ToList();

                foreach (var featuredProduct in featuredProducts)
                {
                    relatedProducts.Add(featuredProduct);
                    addedProductIds.Add(featuredProduct.Id);
                }
            }

            // Lấy ảnh chính và ảnh hover cho sản phẩm liên quan
            var relatedProductImages = new Dictionary<int, string>();
            var relatedProductHoverImages = new Dictionary<int, string>();
            foreach (var relatedProduct in relatedProducts)
            {
                var allImages = _productService.GetProductImages(relatedProduct.Id);
                var relatedMainImage = allImages.FirstOrDefault(img => img.IsMain) ?? allImages.FirstOrDefault();
                if (relatedMainImage != null)
                {
                    relatedProductImages[relatedProduct.Id] = relatedMainImage.ImageUrl;
                }
                
                // Lấy ảnh phụ (ảnh thứ 2) để làm hover-image
                if (allImages.Count > 1)
                {
                    var relatedHoverImage = allImages.Where(img => !img.IsMain).OrderBy(img => img.DisplayOrder).FirstOrDefault() 
                                    ?? allImages.Skip(1).FirstOrDefault();
                    if (relatedHoverImage != null)
                    {
                        relatedProductHoverImages[relatedProduct.Id] = relatedHoverImage.ImageUrl;
                    }
                }
            }

            // Lấy review và thống kê review
            // Lấy review đã duyệt cho tất cả
            var reviews = _reviewService.GetByProductId(product.Id);
            
            // Kiểm tra user đã đánh giá và đã mua hàng chưa (nếu có user đăng nhập)
            var currentUserId = GetCurrentUserId();
            var hasUserReviewed = false;
            var hasUserPurchased = false;
            
            // Nếu có user đăng nhập, lấy thêm review của chính họ (dù chưa duyệt) để hiển thị
            if (currentUserId.HasValue)
            {
                hasUserReviewed = _reviewService.HasUserReviewedProduct(currentUserId.Value, product.Id);
                hasUserPurchased = _reviewService.HasUserPurchasedProduct(currentUserId.Value, product.Id);
                
                // Lấy review của user hiện tại (bao gồm cả chưa duyệt)
                var userReviews = _reviewService.GetByUserId(currentUserId.Value)
                    .Where(r => r.ProductId == product.Id).ToList();
                
                // Thêm review của user vào danh sách nếu chưa có (review chưa duyệt)
                foreach (var userReview in userReviews)
                {
                    if (!reviews.Any(r => r.Id == userReview.Id))
                    {
                        reviews.Add(userReview);
                    }
                }
                
                // Sắp xếp lại theo ngày tạo (mới nhất trước)
                reviews = reviews.OrderByDescending(r => r.CreatedDate).ToList();
            }
            
            // Tính lại thống kê (chỉ tính review đã duyệt)
            var reviewStatistics = _reviewService.GetProductReviewStatistics(product.Id);

            ViewBag.Product = product;
            ViewBag.ProductImages = productImages;
            ViewBag.MainImage = mainImage;
            ViewBag.Variants = variants;
            ViewBag.VariantImages = variantImages;
            ViewBag.VariantsByColor = variantsByColor;
            ViewBag.RelatedProducts = relatedProducts;
            ViewBag.RelatedProductImages = relatedProductImages;
            ViewBag.RelatedProductHoverImages = relatedProductHoverImages;
            ViewBag.Reviews = reviews;
            ViewBag.ReviewStatistics = reviewStatistics;
            ViewBag.CurrentUserId = currentUserId ?? 0;
            ViewBag.HasUserReviewed = hasUserReviewed;
            ViewBag.HasUserPurchased = hasUserPurchased;

            return View();
        }
    }
}

