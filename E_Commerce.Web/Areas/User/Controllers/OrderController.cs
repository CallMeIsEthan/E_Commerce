using System;
using System.Linq;
using System.Web.Mvc;
using E_Commerce.Dto;
using E_Commerce.Service;
using E_Commerce.Web.ViewModels;

namespace E_Commerce.Web.Areas.User.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly IDiscountCodeService _discountCodeService;

        public OrderController(IOrderService orderService, ICartService cartService, IDiscountCodeService discountCodeService)
        {
            _orderService = orderService;
            _cartService = cartService;
            _discountCodeService = discountCodeService;
        }

        // Helper method to get current user ID
        private int? GetCurrentUserId()
        {
            return Session["UserId"] as int?;
        }

        // GET: User/Order/Checkout
        [HttpGet]
        public ActionResult Checkout(int? discountCodeId = null)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account", new { area = "User", returnUrl = Url.Action("Checkout", "Order") });
            }

            // Lấy giỏ hàng hiện tại
            var cart = _cartService.GetCartByUserId(userId.Value);
            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng trống!";
                return RedirectToAction("Index", "Cart");
            }

            // Lấy thông tin user từ session
            var userName = Session["Name"] as string;
            var userEmail = Session["Email"] as string;
            var userPhone = Session["Phone"] as string;

            // Tạo ViewModel với thông tin từ session và cart
            var viewModel = new CheckoutViewModel
            {
                ShippingName = userName ?? string.Empty,
                ShippingPhone = userPhone ?? string.Empty,
                ShippingFee = cart.ShippingFee,
                TaxAmount = cart.TaxAmount,
                DiscountCodeId = discountCodeId
            };

            ViewBag.Cart = cart;
            ViewBag.UserName = userName;
            ViewBag.UserEmail = userEmail;
            ViewBag.UserPhone = userPhone;

            return View(viewModel);
        }

        // POST: User/Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(CheckoutViewModel viewModel)
        {
            // Debug: Log received parameters
            System.Diagnostics.Debug.WriteLine("=== CHECKOUT POST REQUEST ===");
            System.Diagnostics.Debug.WriteLine($"ShippingName: {viewModel?.ShippingName}");
            System.Diagnostics.Debug.WriteLine($"ShippingPhone: {viewModel?.ShippingPhone}");
            System.Diagnostics.Debug.WriteLine($"ShippingAddress: {viewModel?.ShippingAddress}");
            System.Diagnostics.Debug.WriteLine($"PaymentMethod: {viewModel?.PaymentMethod}");
            System.Diagnostics.Debug.WriteLine($"CustomerNotes: {viewModel?.CustomerNotes}");
            System.Diagnostics.Debug.WriteLine($"ShippingFee: {viewModel?.ShippingFee}");
            System.Diagnostics.Debug.WriteLine($"TaxAmount: {viewModel?.TaxAmount}");
            
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập!" }, JsonRequestBehavior.AllowGet);
            }

            // Validate model
            if (!ModelState.IsValid)
            {
                var errors = string.Join("<br/>", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                System.Diagnostics.Debug.WriteLine($"ModelState Errors: {errors}");
                return Json(new { success = false, message = errors }, JsonRequestBehavior.AllowGet);
            }

            // Lấy giỏ hàng
            var cart = _cartService.GetCartByUserId(userId.Value);
            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
            {
                return Json(new { success = false, message = "Giỏ hàng trống!" }, JsonRequestBehavior.AllowGet);
            }

            // Debug: Log cart values
            System.Diagnostics.Debug.WriteLine($"Cart ShippingFee: {cart.ShippingFee}");
            System.Diagnostics.Debug.WriteLine($"Cart TaxAmount: {cart.TaxAmount}");
            System.Diagnostics.Debug.WriteLine($"Cart FinalTotal: {cart.FinalTotal}");
            System.Diagnostics.Debug.WriteLine($"Cart Item Count: {cart.CartItems?.Count ?? 0}");

            try
            {
                // Chuyển từ ViewModel sang OrderCreateDto
                var orderCreateDto = new OrderCreateDto
                {
                    UserId = userId.Value,
                    ShippingName = viewModel.ShippingName?.Trim(),
                    ShippingPhone = viewModel.ShippingPhone?.Trim(),
                    ShippingAddress = viewModel.ShippingAddress?.Trim(),
                    PaymentMethod = viewModel.PaymentMethod?.Trim(),
                    CustomerNotes = viewModel.CustomerNotes?.Trim(),
                    ShippingFee = cart.ShippingFee, // Use cart value, not form value
                    TaxAmount = cart.TaxAmount, // Use cart value, not form value
                    FinalTotal = cart.FinalTotal, // Lấy FinalTotal từ cart (đã tính sẵn)
                    DiscountCodeId = viewModel.DiscountCodeId // Mã giảm giá đã áp dụng
                };
                
                // Debug: Log OrderCreateDto
                System.Diagnostics.Debug.WriteLine("=== OrderCreateDto ===");
                System.Diagnostics.Debug.WriteLine($"UserId: {orderCreateDto.UserId}");
                System.Diagnostics.Debug.WriteLine($"ShippingName: {orderCreateDto.ShippingName}");
                System.Diagnostics.Debug.WriteLine($"ShippingPhone: {orderCreateDto.ShippingPhone}");
                System.Diagnostics.Debug.WriteLine($"ShippingAddress: {orderCreateDto.ShippingAddress}");
                System.Diagnostics.Debug.WriteLine($"PaymentMethod: {orderCreateDto.PaymentMethod}");
                System.Diagnostics.Debug.WriteLine($"ShippingFee: {orderCreateDto.ShippingFee}");
                System.Diagnostics.Debug.WriteLine($"TaxAmount: {orderCreateDto.TaxAmount}");
                System.Diagnostics.Debug.WriteLine($"FinalTotal: {orderCreateDto.FinalTotal}");
                
                    // OrderDetails sẽ được tạo tự động từ cart trong OrderService

                // Tạo đơn hàng
                var order = _orderService.CreateOrderFromCart(userId.Value, orderCreateDto);

                return Json(new 
                { 
                    success = true, 
                    message = "Đặt hàng thành công!",
                    orderId = order.Id,
                    orderNumber = order.OrderNumber
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Order Checkout Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                // Get detailed error message
                var errorMessage = ex.Message;
                var innerEx = ex.InnerException;
                while (innerEx != null)
                {
                    errorMessage += " " + innerEx.Message;
                    innerEx = innerEx.InnerException;
                }
                
                // Clean up error message for user display
                if (errorMessage.Contains("See the inner exception for details"))
                {
                    errorMessage = errorMessage.Replace("See the inner exception for details", "").Trim();
                    if (ex.InnerException != null)
                    {
                        errorMessage = ex.InnerException.Message;
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"Full Error Message: {errorMessage}");
                
                return Json(new { success = false, message = $"Đặt hàng thất bại: {errorMessage}" }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: User/Order/Reorder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reorder(int id)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập!" }, JsonRequestBehavior.AllowGet);
            }

            var order = _orderService.GetOrderById(id);
            if (order == null || order.UserId != userId.Value)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng!" }, JsonRequestBehavior.AllowGet);
            }

            if (order.OrderDetails == null || !order.OrderDetails.Any())
            {
                return Json(new { success = false, message = "Đơn hàng trống, không thể đặt lại." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                foreach (var od in order.OrderDetails)
                {
                    var dto = new CartItemCreateDto
                    {
                        ProductId = od.ProductId,
                        ProductVariantId = od.ProductVariantId,
                        Quantity = od.Quantity
                    };
                    _cartService.AddToCart(userId.Value, dto);
                }

                return Json(new
                {
                    success = true,
                    message = "Đã thêm lại sản phẩm vào giỏ hàng.",
                    redirect = Url.Action("Index", "Cart", new { area = "User" })
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: User/Order/Confirm/{orderId}
        [HttpGet]
        public ActionResult Confirm(int id)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account", new { area = "User" });
            }

            var order = _orderService.GetOrderById(id);
            if (order == null || order.UserId != userId.Value)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng!";
                return RedirectToAction("Index", "Home");
            }

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmReceived(int id)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập!" }, JsonRequestBehavior.AllowGet);
            }

            var order = _orderService.GetOrderById(id);
            if (order == null || order.UserId != userId.Value)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng!" }, JsonRequestBehavior.AllowGet);
            }

            // Chỉ cho phép khi chưa completed
            if (order.Status != null && order.Status.ToLower().Contains("complete"))
            {
                return Json(new { success = false, message = "Đơn hàng đã hoàn tất." }, JsonRequestBehavior.AllowGet);
            }

            // Không cho xác nhận nếu chưa thanh toán (chỉ cho đơn đã Paid)
            if (order.PaymentStatus == null || order.PaymentStatus.ToLower() != "paid")
            {
                return Json(new { success = false, message = "Vui lòng thanh toán trước khi xác nhận đã nhận hàng." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                order.Status = "Completed";
                // TODO: nếu cần, lưu lịch sử trạng thái vào bảng History (chưa có)

                _orderService.UpdateOrder(order); // cần phương thức cập nhật trong service/repo

                return Json(new { success = true, message = "Cảm ơn bạn đã xác nhận. Đơn hàng đã hoàn tất." }, JsonRequestBehavior.AllowGet);
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "Không thể xác nhận: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: User/Order/Cancel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int id, string reason = null)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập!" }, JsonRequestBehavior.AllowGet);
            }

            var order = _orderService.GetOrderById(id);
            if (order == null || order.UserId != userId.Value)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng!" }, JsonRequestBehavior.AllowGet);
            }

            // Chỉ cho phép hủy khi Pending hoặc Processing
            var status = order.Status?.ToLower();
            if (status != "pending" && status != "processing")
            {
                return Json(new { success = false, message = "Chỉ hủy được đơn đang chờ xác nhận/đang xử lý." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                _orderService.CancelOrder(id, reason);
                return Json(new { success = true, message = "Đơn hàng đã được hủy." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: User/Order/MyOrders
        [HttpGet]
        public ActionResult MyOrders()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account", new { area = "User" });
            }

            var orders = _orderService.GetOrdersByUserId(userId.Value);
            return View(orders);
        }

        // POST: User/Order/ValidateDiscountCode
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ValidateDiscountCode(string code, decimal totalAmount)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Json(new { success = false, message = "Vui lòng nhập mã giảm giá." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập." }, JsonRequestBehavior.AllowGet);
                }

                var discountCodeDto = _discountCodeService.ValidateDiscountCode(code, totalAmount, userId);

                if (discountCodeDto == null)
                {
                    // Kiểm tra chi tiết để trả về message phù hợp
                    var discountCode = _discountCodeService.GetAll()
                        .FirstOrDefault(dc => dc.Code.ToUpper() == code.ToUpper().Trim());
                    
                    if (discountCode == null)
                    {
                        return Json(new { success = false, message = "Mã giảm giá không tồn tại hoặc đã hết hạn." }, JsonRequestBehavior.AllowGet);
                    }

                    var now = DateTime.Now;
                    if (discountCode.StartDate > now || discountCode.EndDate < now)
                    {
                        return Json(new { success = false, message = "Mã giảm giá không còn hiệu lực." }, JsonRequestBehavior.AllowGet);
                    }

                    if (discountCode.UsageLimit.HasValue && discountCode.UsedCount >= discountCode.UsageLimit.Value)
                    {
                        return Json(new { success = false, message = "Mã giảm giá đã hết lượt sử dụng (tổng)." }, JsonRequestBehavior.AllowGet);
                    }

                    if (userId.HasValue && discountCode.PerUserLimit.HasValue)
                    {
                        // Kiểm tra số lần user đã sử dụng mã này
                        var userOrders = _orderService.GetOrdersByUserId(userId.Value);
                        var userUsageCount = userOrders.Count(o => o.DiscountCodeId == discountCode.Id);
                        
                        if (userUsageCount >= discountCode.PerUserLimit.Value)
                        {
                            return Json(new { success = false, message = $"Bạn đã sử dụng hết lượt ({discountCode.PerUserLimit.Value} lần/người dùng)." }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    if (discountCode.MinOrderAmount.HasValue && totalAmount < discountCode.MinOrderAmount.Value)
                    {
                        return Json(new { 
                            success = false, 
                            message = $"Đơn hàng tối thiểu {discountCode.MinOrderAmount.Value.ToString("N0")}đ để sử dụng mã này." 
                        }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { success = false, message = "Mã giảm giá không hợp lệ." }, JsonRequestBehavior.AllowGet);
                }

                // Tính toán số tiền giảm
                decimal discountAmount = 0;
                if (discountCodeDto.DiscountType == "Percentage")
                {
                    discountAmount = totalAmount * (discountCodeDto.DiscountValue / 100);
                }
                else if (discountCodeDto.DiscountType == "FixedAmount")
                {
                    discountAmount = discountCodeDto.DiscountValue;
                    // Không được giảm quá tổng tiền
                    if (discountAmount > totalAmount)
                    {
                        discountAmount = totalAmount;
                    }
                }

                return Json(new
                {
                    success = true,
                    message = $"Đã áp dụng mã giảm giá '{discountCodeDto.Name}'!",
                    discountCode = discountCodeDto,
                    discountAmount = discountAmount
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}

