using System;
using System.Linq;
using System.Web.Mvc;
using E_Commerce.Data.Repositories;
using E_Commerce.Model.Models;
using E_Commerce.Data.Infrastructure;
using E_Commerce.Service;
using E_Commerce.Web.Attributes;

namespace E_Commerce.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IProductImageRepository _productImageRepository;
        private readonly IProductVariantImageRepository _productVariantImageRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderService _orderService;

        public OrderController(
            IOrderRepository orderRepository,
            IOrderDetailRepository orderDetailRepository,
            IProductImageRepository productImageRepository,
            IProductVariantImageRepository productVariantImageRepository,
            IUnitOfWork unitOfWork,
            IOrderService orderService)
        {
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _productImageRepository = productImageRepository;
            _productVariantImageRepository = productVariantImageRepository;
            _unitOfWork = unitOfWork;
            _orderService = orderService;
        }

        // GET: Admin/Order
        public ActionResult Index()
        {
            var orders = _orderRepository.GetAll()
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            ViewBag.PendingOrdersCount = orders.Count(o => o.Status != null && o.Status.ToLower().Contains("pending"));
            return View(orders);
        }

        // GET: Admin/Order/Details/5
        public ActionResult Details(int id)
        {
            var order = _orderRepository.GetSingleById(id);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Index");
            }

            var orderDetails = _orderDetailRepository.GetMulti(od => od.OrderId == order.Id).ToList();
            var images = orderDetails.ToDictionary(
                od => od.ProductId,
                od => GetProductImage(od.ProductId, od.ProductVariantId));

            ViewBag.OrderDetails = orderDetails;
            ViewBag.ProductImages = images;

            return View(order);
        }

        #region AJAX Status Actions

        // POST: Admin/Order/Confirm/5
        [HttpPost]
        public ActionResult Confirm(int id)
        {
            try
            {
                _orderService.ConfirmOrder(id);
                return Json(new { success = true, message = "Đã xác nhận đơn hàng!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Admin/Order/StartShipping/5
        [HttpPost]
        public ActionResult StartShipping(int id, string trackingNumber = null)
        {
            try
            {
                _orderService.StartShipping(id, trackingNumber);
                return Json(new { success = true, message = "Đã chuyển sang trạng thái đang giao!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Admin/Order/MarkDelivered/5
        [HttpPost]
        public ActionResult MarkDelivered(int id)
        {
            try
            {
                _orderService.MarkDelivered(id);
                return Json(new { success = true, message = "Đã đánh dấu giao hàng thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Admin/Order/Cancel/5
        [HttpPost]
        public ActionResult Cancel(int id, string reason = null)
        {
            try
            {
                _orderService.CancelOrder(id, reason);
                return Json(new { success = true, message = "Đã hủy đơn hàng!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Admin/Order/MarkPaid/5
        [HttpPost]
        public ActionResult MarkPaid(int id)
        {
            try
            {
                _orderService.MarkPaid(id);
                return Json(new { success = true, message = "Đã đánh dấu thanh toán!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Admin/Order/MarkRefunded/5
        [HttpPost]
        public ActionResult MarkRefunded(int id)
        {
            try
            {
                _orderService.MarkRefunded(id);
                return Json(new { success = true, message = "Đã đánh dấu hoàn tiền!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        private string GetProductImage(int productId, int? variantId)
        {
            string imageUrl = null;

            if (variantId.HasValue)
            {
                var mainVariantImage = _productVariantImageRepository.GetSingleByCondition(
                    vi => vi.ProductVariantId == variantId.Value && vi.IsMain);
                if (mainVariantImage != null && !string.IsNullOrWhiteSpace(mainVariantImage.ImageUrl))
                {
                    imageUrl = mainVariantImage.ImageUrl;
                }
                else
                {
                    var firstVariantImage = _productVariantImageRepository.GetSingleByCondition(
                        vi => vi.ProductVariantId == variantId.Value);
                    if (firstVariantImage != null && !string.IsNullOrWhiteSpace(firstVariantImage.ImageUrl))
                    {
                        imageUrl = firstVariantImage.ImageUrl;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                var mainProductImage = _productImageRepository.GetSingleByCondition(
                    pi => pi.ProductId == productId && pi.IsMain);
                if (mainProductImage != null && !string.IsNullOrWhiteSpace(mainProductImage.ImageUrl))
                {
                    imageUrl = mainProductImage.ImageUrl;
                }
                else
                {
                    var firstProductImage = _productImageRepository.GetSingleByCondition(
                        pi => pi.ProductId == productId);
                    if (firstProductImage != null && !string.IsNullOrWhiteSpace(firstProductImage.ImageUrl))
                    {
                        imageUrl = firstProductImage.ImageUrl;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                imageUrl = "/Content/images/default-product.png";
            }

            return imageUrl;
        }
    }
}

