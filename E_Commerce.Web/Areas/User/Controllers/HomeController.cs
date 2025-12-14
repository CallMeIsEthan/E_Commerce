using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using E_Commerce.Service;
using E_Commerce.Dto;

namespace E_Commerce.Web.Areas.User.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IBrandService _brandService;
        private readonly IWishlistService _wishlistService;

        public HomeController(
            IProductService productService,
            ICategoryService categoryService,
            IBrandService brandService,
            IWishlistService wishlistService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _brandService = brandService;
            _wishlistService = wishlistService;
        }

        private int? GetCurrentUserId()
        {
            return Session["UserId"] as int?;
        }

        // GET: User/Home/Index
        public ActionResult Index()
        {
            // Lấy sản phẩm nổi bật
            var featuredProducts = _productService.GetFeaturedProducts().Take(8).ToList();
            
            // Lấy sản phẩm mới (sắp xếp theo ngày tạo)
            var newProducts = _productService.GetActiveProducts()
                .OrderByDescending(p => p.CreatedDate)
                .Take(8)
                .ToList();
            
            // Lấy sản phẩm đang giảm giá
            var saleProducts = _productService.GetOnSaleProducts().Take(8).ToList();
            
            // Lấy danh mục gốc (level 1) và danh mục con (level 2)
            var rootCategories = _categoryService.GetRootCategories();
            var allCategories = _categoryService.GetActiveCategories();
            
            // Lấy danh mục bậc 1 có HomeFlag = true để hiển thị trên trang chủ
            var homeCategories = _categoryService.GetHomeCategories();
            
            // Đếm số sản phẩm trong mỗi category
            var categoryProductCounts = new Dictionary<int, int>();
            foreach (var category in homeCategories)
            {
                var products = _productService.GetByCategoryId(category.Id);
                categoryProductCounts[category.Id] = products.Count(p => p.IsActive);
            }
            
            // Lấy ảnh chính cho mỗi sản phẩm
            var productMainImages = new Dictionary<int, string>();
            foreach (var product in featuredProducts.Concat(newProducts).Concat(saleProducts).Distinct())
            {
                var mainImage = _productService.GetProductMainImage(product.Id);
                if (mainImage != null)
                {
                    productMainImages[product.Id] = mainImage.ImageUrl;
                }
            }

            ViewBag.FeaturedProducts = featuredProducts;
            ViewBag.NewProducts = newProducts;
            ViewBag.SaleProducts = saleProducts;
            ViewBag.RootCategories = rootCategories;
            ViewBag.AllCategories = allCategories;
            ViewBag.HomeCategories = homeCategories;
            ViewBag.CategoryProductCounts = categoryProductCounts;
            ViewBag.ProductMainImages = productMainImages;

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

        // GET: User/Home/About
        public ActionResult About()
        {
            ViewBag.Title = "Giới thiệu";
            return View();
        }

        // GET: User/Home/Contact
        public ActionResult Contact()
        {
            ViewBag.Title = "Liên hệ";
            return View();
        }

        // POST: User/Home/Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contact(string name, string email, string subject, string message)
        {
            // TODO: Xử lý gửi email hoặc lưu vào database
            // Tạm thời chỉ trả về success message
            ViewBag.SuccessMessage = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi sớm nhất có thể.";
            ViewBag.Title = "Liên hệ";
            return View();
        }
    }
}