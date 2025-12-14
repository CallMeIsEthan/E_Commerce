using System;
using System.Linq;
using System.Web.Mvc;
using E_Commerce.Dto;
using E_Commerce.Service;
using E_Commerce.Data.Repositories;

namespace E_Commerce.Web.Areas.User.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IDiscountCodeService _discountCodeService;

        public CartController(ICartService cartService, IDiscountCodeService discountCodeService)
        {
            _cartService = cartService;
            _discountCodeService = discountCodeService;
        }

        // Helper method to get current user ID
        private int? GetCurrentUserId()
        {
            return Session["UserId"] as int?;
        }

        // GET: User/Cart/Index
        public ActionResult Index()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account", new { area = "User", returnUrl = Url.Action("Index", "Cart") });
            }

            var cart = _cartService.GetCartByUserId(userId.Value);
            return View(cart);
        }

        // POST: User/Cart/AddToCart (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AddToCart(CartItemCreateDto cartItemCreateDto)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                if (cartItemCreateDto.Quantity <= 0)
                {
                    return Json(new { success = false, message = "Số lượng phải lớn hơn 0." }, JsonRequestBehavior.AllowGet);
                }

                var cartItem = _cartService.AddToCart(userId.Value, cartItemCreateDto);
                var cartItemCount = _cartService.GetCartItemCount(userId.Value);

                return Json(new 
                { 
                    success = true, 
                    message = "Đã thêm sản phẩm vào giỏ hàng!", 
                    cartItemCount = cartItemCount,
                    cartItem = cartItem
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: User/Cart/UpdateCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateCart(int cartItemId, int quantity)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var cartItem = _cartService.UpdateCartItem(cartItemId, quantity);
                var cart = _cartService.GetCartByUserId(userId.Value);

                return Json(new 
                { 
                    success = true, 
                    message = "Đã cập nhật giỏ hàng!",
                    cartItem = cartItem,
                    totalAmount = cart.TotalAmount,
                    shippingFee = cart.ShippingFee,
                    taxAmount = cart.TaxAmount,
                    finalTotal = cart.FinalTotal,
                    totalItems = cart.TotalItems
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: User/Cart/RemoveFromCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RemoveFromCart(int cartItemId)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var result = _cartService.RemoveFromCart(cartItemId);
                if (result)
                {
                    var cart = _cartService.GetCartByUserId(userId.Value);
                    return Json(new 
                    { 
                        success = true, 
                        message = "Đã xóa sản phẩm khỏi giỏ hàng!",
                        totalAmount = cart.TotalAmount,
                        shippingFee = cart.ShippingFee,
                        taxAmount = cart.TaxAmount,
                        finalTotal = cart.FinalTotal,
                        totalItems = cart.TotalItems
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ hàng." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: User/Cart/ClearCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ClearCart()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var result = _cartService.ClearCart(userId.Value);
                if (result)
                {
                    return Json(new { success = true, message = "Đã xóa toàn bộ giỏ hàng!" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = false, message = "Giỏ hàng đã trống." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: User/Cart/GetCartCount (AJAX)
        [HttpGet]
        public JsonResult GetCartCount()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Json(new { count = 0 }, JsonRequestBehavior.AllowGet);
            }

            var count = _cartService.GetCartItemCount(userId.Value);
            return Json(new { count = count }, JsonRequestBehavior.AllowGet);
        }

    }
}

