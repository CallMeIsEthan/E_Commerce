using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using E_Commerce.Service;

namespace E_Commerce.Web.Areas.User.Controllers
{
    public class WishlistController : Controller
    {
        private readonly IWishlistService _wishlistService;
        private readonly IProductService _productService;

        public WishlistController(IWishlistService wishlistService, IProductService productService)
        {
            _wishlistService = wishlistService;
            _productService = productService;
        }

        private int? GetCurrentUserId()
        {
            return Session["UserId"] as int?;
        }

        // GET: User/Wishlist
        public ActionResult Index()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account", new { area = "User", returnUrl = Url.Action("Index", "Wishlist", new { area = "User" }) });
            }

            var items = _wishlistService.GetByUser(userId.Value);
            
            // Lấy ảnh chính và ảnh hover cho mỗi sản phẩm trong wishlist
            var productMainImages = new Dictionary<int, string>();
            var productHoverImages = new Dictionary<int, string>();
            foreach (var item in items)
            {
                var allImages = _productService.GetProductImages(item.ProductId);
                var mainImage = allImages.FirstOrDefault(img => img.IsMain) ?? allImages.FirstOrDefault();
                if (mainImage != null)
                {
                    productMainImages[item.ProductId] = mainImage.ImageUrl;
                }
                
                // Lấy ảnh phụ (ảnh thứ 2) để làm hover-image
                if (allImages.Count > 1)
                {
                    var hoverImage = allImages.Where(img => !img.IsMain).OrderBy(img => img.DisplayOrder).FirstOrDefault() 
                                    ?? allImages.Skip(1).FirstOrDefault();
                    if (hoverImage != null)
                    {
                        productHoverImages[item.ProductId] = hoverImage.ImageUrl;
                    }
                }
            }
            
            ViewBag.ProductMainImages = productMainImages;
            ViewBag.ProductHoverImages = productHoverImages;
            
            return View(items);
        }

        // POST: User/Wishlist/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Add(int productId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để thêm vào yêu thích." }, JsonRequestBehavior.AllowGet);
                }

                if (productId <= 0)
                {
                    return Json(new { success = false, message = "Sản phẩm không hợp lệ." }, JsonRequestBehavior.AllowGet);
                }

                var item = _wishlistService.Add(userId.Value, productId);
                var count = _wishlistService.Count(userId.Value);

                return Json(new
                {
                    success = true,
                    message = "Đã thêm vào danh sách yêu thích.",
                    count,
                    item
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log error (có thể dùng logger sau)
                System.Diagnostics.Debug.WriteLine($"Wishlist Add Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                return Json(new 
                { 
                    success = false, 
                    message = !string.IsNullOrEmpty(ex.Message) ? ex.Message : "Đã xảy ra lỗi. Vui lòng thử lại." 
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: User/Wishlist/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Remove(int productId)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để xóa khỏi yêu thích." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var removed = _wishlistService.Remove(userId.Value, productId);
                var count = _wishlistService.Count(userId.Value);

                return Json(new
                {
                    success = removed,
                    message = removed ? "Đã xóa khỏi danh sách yêu thích." : "Không tìm thấy sản phẩm trong danh sách yêu thích.",
                    count
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: User/Wishlist/GetCount
        [HttpGet]
        public JsonResult GetCount()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Json(new { count = 0 }, JsonRequestBehavior.AllowGet);
            }

            var count = _wishlistService.Count(userId.Value);
            return Json(new { count }, JsonRequestBehavior.AllowGet);
        }

        // GET: User/Wishlist/GetProductIds
        [HttpGet]
        public JsonResult GetProductIds()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Json(new { productIds = new int[0] }, JsonRequestBehavior.AllowGet);
            }

            var productIds = _wishlistService.GetWishlistProductIds(userId.Value);
            return Json(new { productIds }, JsonRequestBehavior.AllowGet);
        }

        // GET: User/Wishlist/CheckHasVariants
        [HttpGet]
        public JsonResult CheckHasVariants(int productId)
        {
            try
            {
                var variants = _productService.GetVariantsByProductId(productId);
                var hasVariants = variants != null && variants.Any();
                return Json(new { hasVariants, variantCount = variants?.Count ?? 0 }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { hasVariants = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}

