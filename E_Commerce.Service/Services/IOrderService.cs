using E_Commerce.Dto;
using System;
using System.Collections.Generic;

namespace E_Commerce.Service
{
    public interface IOrderService
    {
        OrderDto CreateOrderFromCart(int userId, OrderCreateDto orderCreateDto);
        OrderDto GetOrderById(int orderId);
        List<OrderDto> GetOrdersByUserId(int userId);
        List<OrderDto> GetAllOrders();
        string GenerateOrderNumber();
        void UpdateOrder(OrderDto orderDto);
        
        // Status transition methods
        bool ConfirmOrder(int orderId);           // Pending -> Processing
        bool StartShipping(int orderId, string trackingNumber = null);  // Processing -> Shipping
        bool MarkDelivered(int orderId);          // Shipping -> Delivered
        bool CancelOrder(int orderId, string reason = null);  // Pending/Processing -> Cancelled
        
        // Payment methods
        bool MarkPaid(int orderId);
        bool MarkRefunded(int orderId);
        
        // Report methods
        decimal GetRevenueByDateRange(DateTime startDate, DateTime endDate);
        int GetOrderCountByDateRange(DateTime startDate, DateTime endDate);
        int GetOrderCountByStatus(string status);
        List<OrderDto> GetOrdersByDateRange(DateTime startDate, DateTime endDate);
        Dictionary<string, decimal> GetRevenueByMonth(int year);
        Dictionary<string, int> GetOrderCountByMonth(int year);
    }
}

