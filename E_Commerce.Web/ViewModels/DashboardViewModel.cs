using System;
using System.Collections.Generic;

namespace E_Commerce.Web.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int TotalUsers { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int TodayOrders { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal WeekRevenue { get; set; }
        public decimal MonthRevenue { get; set; }
        public List<DashboardRevenuePoint> RevenueLast7Days { get; set; }
        public List<DashboardOrderItem> RecentOrders { get; set; }
    }

    public class DashboardRevenuePoint
    {
        public string Label { get; set; }
        public decimal Total { get; set; }
        public DateTime Date { get; set; }
    }

    public class DashboardOrderItem
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime OrderDate { get; set; }
        public int TotalItems { get; set; }
    }
}