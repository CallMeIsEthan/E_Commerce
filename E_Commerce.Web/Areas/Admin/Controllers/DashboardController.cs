using E_Commerce.Data.Repositories;
using E_Commerce.Model.Models;
using E_Commerce.Web.Attributes;
using E_Commerce.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace E_Commerce.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class DashboardController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;

        public DashboardController(
            IOrderRepository orderRepository,
            IOrderDetailRepository orderDetailRepository,
            IProductRepository productRepository,
            IUserRepository userRepository)
        {
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
        }

        // GET: Admin/Dashboard
        public ActionResult Index()
        {
            // TODO: Bật lại khi cần check phân quyền
            //if (Session["AdminUser"] == null)
            //{
            //    return RedirectToAction("Login", "Account", new { area = "Admin" });
            //}

            ViewBag.Title = "Dashboard";

            var orders = _orderRepository.GetAll().ToList();
            var today = DateTime.Today;
            var weekStart = today.AddDays(-6);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var paidOrders = orders
                .Where(o => o.PaymentStatus != null && o.PaymentStatus.ToLower() == "paid")
                .ToList();

            var revenueLast7Days = new List<DashboardRevenuePoint>();
            for (int i = 0; i < 7; i++)
            {
                var day = weekStart.AddDays(i);
                var total = paidOrders.Where(o => o.OrderDate.Date == day).Sum(o => o.TotalAmount);
                revenueLast7Days.Add(new DashboardRevenuePoint
                {
                    Date = day,
                    Label = day.ToString("dd/MM"),
                    Total = total
                });
            }

            var recentOrders = orders
                .OrderByDescending(o => o.OrderDate)
                .Take(8)
                .Select(o =>
                {
                    var itemCount = _orderDetailRepository.GetMulti(od => od.OrderId == o.Id).Sum(od => od.Quantity);
                    var customerName = o.User != null ? o.User.FullName : $"User #{o.UserId}";
                    return new DashboardOrderItem
                    {
                        Id = o.Id,
                        OrderNumber = o.OrderNumber,
                        CustomerName = customerName,
                        TotalAmount = o.TotalAmount,
                        Status = o.Status,
                        PaymentStatus = o.PaymentStatus,
                        OrderDate = o.OrderDate,
                        TotalItems = itemCount
                    };
                })
                .ToList();

            var totalRevenue = paidOrders.Sum(o => o.TotalAmount);
            var todayOrders = orders.Where(o => o.OrderDate.Date == today).ToList();
            var todayPaid = paidOrders.Where(o => o.OrderDate.Date == today).ToList();
            var weekRevenue = paidOrders.Where(o => o.OrderDate.Date >= weekStart).Sum(o => o.TotalAmount);
            var monthRevenue = paidOrders.Where(o => o.OrderDate.Date >= monthStart).Sum(o => o.TotalAmount);

            var viewModel = new DashboardViewModel
            {
                TotalProducts = _productRepository.GetAll().Count(),
                TotalOrders = orders.Count,
                TotalUsers = _userRepository.GetAll().Count(),
                TotalRevenue = totalRevenue,
                PendingOrders = orders.Count(o => o.Status != null && o.Status.ToLower().Contains("pending")),
                ProcessingOrders = orders.Count(o => o.Status != null && o.Status.ToLower().Contains("process")),
                CompletedOrders = orders.Count(o => o.Status != null && o.Status.ToLower().Contains("complete")),
                CancelledOrders = orders.Count(o => o.Status != null && o.Status.ToLower().Contains("cancel")),
                TodayOrders = todayOrders.Count,
                TodayRevenue = todayPaid.Sum(o => o.TotalAmount),
                WeekRevenue = weekRevenue,
                MonthRevenue = monthRevenue,
                RevenueLast7Days = revenueLast7Days,
                RecentOrders = recentOrders
            };

            ViewBag.PendingOrdersCount = viewModel.PendingOrders;

            return View(viewModel);
        }
    }
}