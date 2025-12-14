using E_Commerce.Data.Repositories;
using E_Commerce.Service;
using E_Commerce.Web.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace E_Commerce.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class ReportController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        private readonly IWishlistRepository _wishlistRepository;

        public ReportController(
            IOrderService orderService,
            IOrderRepository orderRepository,
            IOrderDetailRepository orderDetailRepository,
            IProductRepository productRepository,
            IUserRepository userRepository,
            IWishlistRepository wishlistRepository)
        {
            _orderService = orderService;
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
            _wishlistRepository = wishlistRepository;
        }

        // GET: Admin/Report
        public ActionResult Index()
        {
            ViewBag.Title = "Báo cáo";

            var today = DateTime.Today;
            var thisMonthStart = new DateTime(today.Year, today.Month, 1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);
            var lastMonthEnd = thisMonthStart.AddDays(-1);
            var thisYearStart = new DateTime(today.Year, 1, 1);
            var lastYearStart = thisYearStart.AddYears(-1);
            var lastYearEnd = thisYearStart.AddDays(-1);

            // Doanh thu
            var revenueToday = _orderService.GetRevenueByDateRange(today, today.AddDays(1).AddSeconds(-1));
            var revenueThisMonth = _orderService.GetRevenueByDateRange(thisMonthStart, today);
            var revenueLastMonth = _orderService.GetRevenueByDateRange(lastMonthStart, lastMonthEnd);
            var revenueThisYear = _orderService.GetRevenueByDateRange(thisYearStart, today);
            var revenueLastYear = _orderService.GetRevenueByDateRange(lastYearStart, lastYearEnd);

            // Số lượng đơn hàng
            var orderCountToday = _orderService.GetOrderCountByDateRange(today, today.AddDays(1).AddSeconds(-1));
            var orderCountThisMonth = _orderService.GetOrderCountByDateRange(thisMonthStart, today);
            var orderCountLastMonth = _orderService.GetOrderCountByDateRange(lastMonthStart, lastMonthEnd);
            var orderCountThisYear = _orderService.GetOrderCountByDateRange(thisYearStart, today);

            // Đơn hàng theo trạng thái
            var pendingCount = _orderService.GetOrderCountByStatus("Pending");
            var processingCount = _orderService.GetOrderCountByStatus("Processing");
            var shippingCount = _orderService.GetOrderCountByStatus("Shipping");
            var deliveredCount = _orderService.GetOrderCountByStatus("Delivered");
            var cancelledCount = _orderService.GetOrderCountByStatus("Cancelled");

            // Doanh thu theo tháng (năm hiện tại)
            var revenueByMonth = _orderService.GetRevenueByMonth(today.Year);
            var orderCountByMonth = _orderService.GetOrderCountByMonth(today.Year);

            // Sản phẩm bán chạy (top 10)
            var bestSellingProducts = GetBestSellingProducts(10);

            // Sản phẩm được yêu thích (top 10) - dựa trên số lượt thêm vào wishlist
            var mostFavoriteProducts = GetMostFavoriteProducts(10);

            // Tổng số khách hàng
            var totalCustomers = _userRepository.GetAll().Count();

            // Khách hàng mới trong tháng
            var newCustomersThisMonth = _userRepository.GetMulti(u =>
                u.CreatedDate >= thisMonthStart &&
                u.CreatedDate <= today).Count();

            ViewBag.RevenueToday = revenueToday;
            ViewBag.RevenueThisMonth = revenueThisMonth;
            ViewBag.RevenueLastMonth = revenueLastMonth;
            ViewBag.RevenueThisYear = revenueThisYear;
            ViewBag.RevenueLastYear = revenueLastYear;

            ViewBag.OrderCountToday = orderCountToday;
            ViewBag.OrderCountThisMonth = orderCountThisMonth;
            ViewBag.OrderCountLastMonth = orderCountLastMonth;
            ViewBag.OrderCountThisYear = orderCountThisYear;

            ViewBag.PendingCount = pendingCount;
            ViewBag.ProcessingCount = processingCount;
            ViewBag.ShippingCount = shippingCount;
            ViewBag.DeliveredCount = deliveredCount;
            ViewBag.CancelledCount = cancelledCount;

            ViewBag.RevenueByMonth = revenueByMonth;
            ViewBag.OrderCountByMonth = orderCountByMonth;
            ViewBag.CurrentYear = today.Year;

            ViewBag.BestSellingProducts = bestSellingProducts;
            ViewBag.MostFavoriteProducts = mostFavoriteProducts;
            ViewBag.TotalCustomers = totalCustomers;
            ViewBag.NewCustomersThisMonth = newCustomersThisMonth;

            return View();
        }

        // GET: Admin/Report/GetRevenueChartData
        [HttpGet]
        public JsonResult GetRevenueChartData(int? year = null)
        {
            var targetYear = year ?? DateTime.Now.Year;
            var revenueByMonth = _orderService.GetRevenueByMonth(targetYear);
            var orderCountByMonth = _orderService.GetOrderCountByMonth(targetYear);

            return Json(new
            {
                labels = revenueByMonth.Keys.ToList(),
                revenue = revenueByMonth.Values.ToList(),
                orderCount = orderCountByMonth.Values.ToList()
            }, JsonRequestBehavior.AllowGet);
        }

        private List<BestSellingProductViewModel> GetBestSellingProducts(int topCount)
        {
            var orderDetails = _orderDetailRepository.GetAll()
                .Where(od => od.Order != null &&
                            od.Order.Status != "Cancelled" &&
                            od.Order.PaymentStatus == "Paid")
                .GroupBy(od => od.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(od => od.Quantity),
                    TotalRevenue = g.Sum(od => od.TotalPrice)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(topCount)
                .ToList();

            var result = new List<BestSellingProductViewModel>();
            foreach (var item in orderDetails)
            {
                var product = _productRepository.GetSingleById(item.ProductId);
                if (product != null && !product.IsDeleted)
                {
                    result.Add(new BestSellingProductViewModel
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        TotalQuantity = item.TotalQuantity,
                        TotalRevenue = item.TotalRevenue
                    });
                }
            }

            return result;
        }

        private List<MostFavoriteProductViewModel> GetMostFavoriteProducts(int topCount)
        {
            var wishlistCounts = _wishlistRepository.GetAll()
                .Where(w => w.Product != null && !w.Product.IsDeleted && w.Product.IsActive)
                .GroupBy(w => w.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    WishlistCount = g.Count()
                })
                .OrderByDescending(x => x.WishlistCount)
                .Take(topCount)
                .ToList();

            var result = new List<MostFavoriteProductViewModel>();
            foreach (var item in wishlistCounts)
            {
                var product = _productRepository.GetSingleById(item.ProductId);
                if (product != null && !product.IsDeleted)
                {
                    result.Add(new MostFavoriteProductViewModel
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        WishlistCount = item.WishlistCount
                    });
                }
            }

            return result;
        }
    }

    public class BestSellingProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class MostFavoriteProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int WishlistCount { get; set; }
    }
}