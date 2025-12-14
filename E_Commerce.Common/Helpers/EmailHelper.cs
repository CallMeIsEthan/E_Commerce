using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Text;
using E_Commerce.Dto;

namespace E_Commerce.Common.Helpers
{
    /// <summary>
    /// Helper class để gửi email
    /// </summary>
    public static class EmailHelper
    {
        /// <summary>
        /// Lấy cấu hình email từ Web.config
        /// </summary>
        private static string GetSmtpHost() => ConfigurationManager.AppSettings["Email:SmtpHost"] ?? "smtp.gmail.com";
        private static int GetSmtpPort() => int.TryParse(ConfigurationManager.AppSettings["Email:SmtpPort"], out int port) ? port : 587;
        private static string GetSenderEmail() => ConfigurationManager.AppSettings["Email:SenderEmail"] ?? throw new Exception("Email:SenderEmail is missing in Web.config");
        private static string GetSenderPassword() => ConfigurationManager.AppSettings["Email:SenderPassword"] ?? throw new Exception("Email:SenderPassword is missing in Web.config");
        private static string GetSenderName() => ConfigurationManager.AppSettings["Email:SenderName"] ?? "NovaStore";

        /// <summary>
        /// Gửi email xác nhận đơn hàng cho khách hàng
        /// </summary>
        /// <param name="order">Thông tin đơn hàng</param>
        /// <param name="customerEmail">Email khách hàng</param>
        /// <param name="customerName">Tên khách hàng</param>
        /// <returns>True nếu gửi thành công, False nếu có lỗi</returns>
        public static async Task<bool> SendOrderConfirmationAsync(
            OrderDto order,
            string customerEmail,
            string customerName)
        {
            try
            {
                using (var client = new SmtpClient(GetSmtpHost(), GetSmtpPort()))
                {
                    client.EnableSsl = true;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(GetSenderEmail(), GetSenderPassword());

                    string subject = $"Xác nhận đơn hàng #{order.OrderNumber} - NovaStore";

                    string body = GenerateOrderConfirmationHtml(order, customerName);

                    using (var mailMessage = new MailMessage())
                    {
                        // Set From với display name "NovaStore" để hiển thị tên thay vì email
                        mailMessage.From = new MailAddress(GetSenderEmail(), GetSenderName(), Encoding.UTF8);
                        mailMessage.To.Add(new MailAddress(customerEmail, customerName, Encoding.UTF8));
                        mailMessage.Subject = subject;
                        mailMessage.Body = body;
                        mailMessage.IsBodyHtml = true;
                        mailMessage.BodyEncoding = Encoding.UTF8;
                        mailMessage.SubjectEncoding = Encoding.UTF8;
                        mailMessage.HeadersEncoding = Encoding.UTF8;

                        await client.SendMailAsync(mailMessage);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Send order confirmation email failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Gửi email thông báo đơn hàng đã được xác nhận
        /// </summary>
        public static async Task<bool> SendOrderConfirmedAsync(
            OrderDto order,
            string customerEmail,
            string customerName)
        {
            try
            {
                using (var client = new SmtpClient(GetSmtpHost(), GetSmtpPort()))
                {
                    client.EnableSsl = true;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(GetSenderEmail(), GetSenderPassword());

                    string subject = $"Đơn hàng #{order.OrderNumber} đã được xác nhận - NovaStore";
                    string body = GenerateOrderStatusHtml(order, customerName, "Đơn hàng đã được xác nhận", 
                        "Đơn hàng của bạn đã được xác nhận và đang được chuẩn bị để giao hàng.");

                    using (var mailMessage = new MailMessage())
                    {
                        mailMessage.From = new MailAddress(GetSenderEmail(), GetSenderName(), Encoding.UTF8);
                        mailMessage.To.Add(new MailAddress(customerEmail, customerName, Encoding.UTF8));
                        mailMessage.Subject = subject;
                        mailMessage.Body = body;
                        mailMessage.IsBodyHtml = true;
                        mailMessage.BodyEncoding = Encoding.UTF8;
                        mailMessage.SubjectEncoding = Encoding.UTF8;
                        mailMessage.HeadersEncoding = Encoding.UTF8;

                        await client.SendMailAsync(mailMessage);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Send order confirmed email failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gửi email thông báo đơn hàng đã được giao
        /// </summary>
        public static async Task<bool> SendOrderShippedAsync(
            OrderDto order,
            string customerEmail,
            string customerName,
            string trackingNumber = null)
        {
            try
            {
                using (var client = new SmtpClient(GetSmtpHost(), GetSmtpPort()))
                {
                    client.EnableSsl = true;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(GetSenderEmail(), GetSenderPassword());

                    string subject = $"Đơn hàng #{order.OrderNumber} đã được giao - NovaStore";
                    string statusMessage = "Đơn hàng của bạn đã được giao và đang trên đường đến với bạn.";
                    if (!string.IsNullOrWhiteSpace(trackingNumber))
                    {
                        statusMessage += $"<p style='margin: 15px 0;'><strong>Mã vận đơn:</strong> <span style='color: #007bff; font-size: 18px;'>{trackingNumber}</span></p>";
                    }
                    string body = GenerateOrderStatusHtml(order, customerName, "Đơn hàng đã được giao", statusMessage);

                    using (var mailMessage = new MailMessage())
                    {
                        mailMessage.From = new MailAddress(GetSenderEmail(), GetSenderName(), Encoding.UTF8);
                        mailMessage.To.Add(new MailAddress(customerEmail, customerName, Encoding.UTF8));
                        mailMessage.Subject = subject;
                        mailMessage.Body = body;
                        mailMessage.IsBodyHtml = true;
                        mailMessage.BodyEncoding = Encoding.UTF8;
                        mailMessage.SubjectEncoding = Encoding.UTF8;
                        mailMessage.HeadersEncoding = Encoding.UTF8;

                        await client.SendMailAsync(mailMessage);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Send order shipped email failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gửi email thông báo đơn hàng đã được giao thành công
        /// </summary>
        public static async Task<bool> SendOrderDeliveredAsync(
            OrderDto order,
            string customerEmail,
            string customerName)
        {
            try
            {
                using (var client = new SmtpClient(GetSmtpHost(), GetSmtpPort()))
                {
                    client.EnableSsl = true;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(GetSenderEmail(), GetSenderPassword());

                    string subject = $"Đơn hàng #{order.OrderNumber} đã được giao thành công - NovaStore";
                    string body = GenerateOrderStatusHtml(order, customerName, "Đơn hàng đã được giao thành công", 
                        "Cảm ơn bạn đã mua sắm tại NovaStore! Chúng tôi hy vọng bạn hài lòng với sản phẩm.");

                    using (var mailMessage = new MailMessage())
                    {
                        mailMessage.From = new MailAddress(GetSenderEmail(), GetSenderName(), Encoding.UTF8);
                        mailMessage.To.Add(new MailAddress(customerEmail, customerName, Encoding.UTF8));
                        mailMessage.Subject = subject;
                        mailMessage.Body = body;
                        mailMessage.IsBodyHtml = true;
                        mailMessage.BodyEncoding = Encoding.UTF8;
                        mailMessage.SubjectEncoding = Encoding.UTF8;
                        mailMessage.HeadersEncoding = Encoding.UTF8;

                        await client.SendMailAsync(mailMessage);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Send order delivered email failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gửi email thông báo đơn hàng đã bị hủy
        /// </summary>
        public static async Task<bool> SendOrderCancelledAsync(
            OrderDto order,
            string customerEmail,
            string customerName,
            string cancelReason = null)
        {
            try
            {
                using (var client = new SmtpClient(GetSmtpHost(), GetSmtpPort()))
                {
                    client.EnableSsl = true;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(GetSenderEmail(), GetSenderPassword());

                    string subject = $"Đơn hàng #{order.OrderNumber} đã bị hủy - NovaStore";
                    string statusMessage = "Đơn hàng của bạn đã bị hủy. Nếu bạn có thắc mắc, vui lòng liên hệ với chúng tôi.";
                    if (!string.IsNullOrWhiteSpace(cancelReason))
                    {
                        statusMessage += $"<p style='margin: 15px 0;'><strong>Lý do hủy:</strong> {cancelReason}</p>";
                    }
                    string body = GenerateOrderStatusHtml(order, customerName, "Đơn hàng đã bị hủy", statusMessage);

                    using (var mailMessage = new MailMessage())
                    {
                        mailMessage.From = new MailAddress(GetSenderEmail(), GetSenderName(), Encoding.UTF8);
                        mailMessage.To.Add(new MailAddress(customerEmail, customerName, Encoding.UTF8));
                        mailMessage.Subject = subject;
                        mailMessage.Body = body;
                        mailMessage.IsBodyHtml = true;
                        mailMessage.BodyEncoding = Encoding.UTF8;
                        mailMessage.SubjectEncoding = Encoding.UTF8;
                        mailMessage.HeadersEncoding = Encoding.UTF8;

                        await client.SendMailAsync(mailMessage);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Send order cancelled email failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Tạo HTML template cho email xác nhận đơn hàng
        /// </summary>
        private static string GenerateOrderConfirmationHtml(OrderDto order, string customerName)
        {
            var html = new StringBuilder();
            
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='vi'>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset='UTF-8'>");
            html.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.AppendLine("<style>");
            html.AppendLine(@"
                body {
                    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                    line-height: 1.6;
                    color: #333;
                    background-color: #f4f4f4;
                    margin: 0;
                    padding: 0;
                }
                .container {
                    max-width: 600px;
                    margin: 20px auto;
                    background: #ffffff;
                    border-radius: 10px;
                    overflow: hidden;
                    box-shadow: 0 2px 10px rgba(0,0,0,0.1);
                }
                .header {
                    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                    color: white;
                    padding: 30px;
                    text-align: center;
                }
                .header h1 {
                    margin: 0;
                    font-size: 28px;
                    font-weight: 600;
                }
                .content {
                    padding: 30px;
                }
                .greeting {
                    font-size: 18px;
                    color: #333;
                    margin-bottom: 20px;
                }
                .order-info {
                    background: #f8f9fa;
                    border-radius: 8px;
                    padding: 20px;
                    margin: 20px 0;
                }
                .info-row {
                    display: flex;
                    justify-content: space-between;
                    padding: 10px 0;
                    border-bottom: 1px solid #e0e0e0;
                }
                .info-row:last-child {
                    border-bottom: none;
                }
                .info-label {
                    font-weight: 600;
                    color: #555;
                }
                .info-value {
                    color: #333;
                }
                .order-number {
                    font-size: 24px;
                    font-weight: 700;
                    color: #667eea;
                }
                .items-table {
                    width: 100%;
                    border-collapse: collapse;
                    margin: 20px 0;
                }
                .items-table th {
                    background: #f8f9fa;
                    padding: 12px;
                    text-align: left;
                    border-bottom: 2px solid #e0e0e0;
                    font-weight: 600;
                    color: #555;
                }
                .items-table td {
                    padding: 12px;
                    border-bottom: 1px solid #e0e0e0;
                }
                .items-table tr:last-child td {
                    border-bottom: none;
                }
                .product-name {
                    font-weight: 600;
                    color: #333;
                }
                .product-variant {
                    font-size: 13px;
                    color: #777;
                    margin-top: 4px;
                }
                .text-right {
                    text-align: right;
                }
                .text-center {
                    text-align: center;
                }
                .summary {
                    background: #f8f9fa;
                    border-radius: 8px;
                    padding: 20px;
                    margin: 20px 0;
                }
                .summary-row {
                    display: flex;
                    justify-content: space-between;
                    padding: 8px 0;
                }
                .summary-total {
                    font-size: 20px;
                    font-weight: 700;
                    color: #667eea;
                    border-top: 2px solid #e0e0e0;
                    padding-top: 10px;
                    margin-top: 10px;
                }
                .shipping-info {
                    background: #fff3cd;
                    border-left: 4px solid #ffc107;
                    padding: 15px;
                    margin: 20px 0;
                    border-radius: 4px;
                }
                .status-badge {
                    display: inline-block;
                    padding: 6px 12px;
                    border-radius: 20px;
                    font-size: 13px;
                    font-weight: 600;
                }
                .status-pending {
                    background: #fff3cd;
                    color: #856404;
                }
                .status-processing {
                    background: #cfe2ff;
                    color: #084298;
                }
                .status-shipping {
                    background: #d1e7dd;
                    color: #0f5132;
                }
                .status-delivered {
                    background: #d1e7dd;
                    color: #0f5132;
                }
                .footer {
                    background: #f8f9fa;
                    padding: 20px;
                    text-align: center;
                    color: #777;
                    font-size: 14px;
                }
                .footer a {
                    color: #667eea;
                    text-decoration: none;
                }
            ");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("<div class='container'>");
            
            // Header
            html.AppendLine("<div class='header'>");
            html.AppendLine("<h1>🎉 Đơn hàng của bạn đã được đặt thành công!</h1>");
            html.AppendLine("</div>");
            
            // Content
            html.AppendLine("<div class='content'>");
            html.AppendLine($"<div class='greeting'>Xin chào <strong>{customerName}</strong>,</div>");
            html.AppendLine("<p>Cảm ơn bạn đã đặt hàng tại NovaStore. Chúng tôi đã nhận được đơn hàng của bạn và đang xử lý.</p>");
            
            // Order Info
            html.AppendLine("<div class='order-info'>");
            html.AppendLine("<div class='info-row'>");
            html.AppendLine("<span class='info-label'>Mã đơn hàng:</span>");
            html.AppendLine($"<span class='info-value order-number'>#{order.OrderNumber}</span>");
            html.AppendLine("</div>");
            html.AppendLine("<div class='info-row'>");
            html.AppendLine("<span class='info-label'>Ngày đặt hàng:</span>");
            html.AppendLine($"<span class='info-value'>{order.OrderDate:dd/MM/yyyy HH:mm}</span>");
            html.AppendLine("</div>");
            html.AppendLine("<div class='info-row'>");
            html.AppendLine("<span class='info-label'>Trạng thái:</span>");
            html.AppendLine($"<span class='info-value'><span class='status-badge status-{order.Status.ToLower()}'>{GetStatusText(order.Status)}</span></span>");
            html.AppendLine("</div>");
            html.AppendLine("<div class='info-row'>");
            html.AppendLine("<span class='info-label'>Phương thức thanh toán:</span>");
            html.AppendLine($"<span class='info-value'>{GetPaymentMethodText(order.PaymentMethod)}</span>");
            html.AppendLine("</div>");
            html.AppendLine("<div class='info-row'>");
            html.AppendLine("<span class='info-label'>Trạng thái thanh toán:</span>");
            html.AppendLine($"<span class='info-value'>{GetPaymentStatusText(order.PaymentStatus)}</span>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");
            
            // Order Items
            if (order.OrderDetails != null && order.OrderDetails.Count > 0)
            {
                html.AppendLine("<h3 style='margin-top: 30px; color: #333;'>Chi tiết đơn hàng</h3>");
                html.AppendLine("<table class='items-table'>");
                html.AppendLine("<thead>");
                html.AppendLine("<tr>");
                html.AppendLine("<th>Sản phẩm</th>");
                html.AppendLine("<th class='text-center'>Số lượng</th>");
                html.AppendLine("<th class='text-right'>Đơn giá</th>");
                html.AppendLine("<th class='text-right'>Thành tiền</th>");
                html.AppendLine("</tr>");
                html.AppendLine("</thead>");
                html.AppendLine("<tbody>");
                
                foreach (var item in order.OrderDetails)
                {
                    html.AppendLine("<tr>");
                    html.AppendLine("<td>");
                    html.AppendLine($"<div class='product-name'>{item.ProductName}</div>");
                    if (!string.IsNullOrEmpty(item.Color) || !string.IsNullOrEmpty(item.Size))
                    {
                        html.AppendLine($"<div class='product-variant'>");
                        if (!string.IsNullOrEmpty(item.Color))
                            html.AppendLine($"Màu: {item.Color}");
                        if (!string.IsNullOrEmpty(item.Size))
                            html.AppendLine($" | Size: {item.Size}");
                        html.AppendLine("</div>");
                    }
                    html.AppendLine("</td>");
                    html.AppendLine($"<td class='text-center'>{item.Quantity}</td>");
                    html.AppendLine($"<td class='text-right'>{item.UnitPrice:N0} đ</td>");
                    html.AppendLine($"<td class='text-right'><strong>{item.TotalPrice:N0} đ</strong></td>");
                    html.AppendLine("</tr>");
                }
                
                html.AppendLine("</tbody>");
                html.AppendLine("</table>");
            }
            
            // Summary
            html.AppendLine("<div class='summary'>");
            html.AppendLine("<div class='summary-row'>");
            html.AppendLine("<span>Tạm tính:</span>");
            html.AppendLine($"<span>{order.SubTotal:N0} đ</span>");
            html.AppendLine("</div>");
            if (order.DiscountAmount > 0)
            {
                html.AppendLine("<div class='summary-row'>");
                html.AppendLine($"<span>Giảm giá ({order.DiscountCode}):</span>");
                html.AppendLine($"<span style='color: #28a745;'>-{order.DiscountAmount:N0} đ</span>");
                html.AppendLine("</div>");
            }
            html.AppendLine("<div class='summary-row'>");
            html.AppendLine("<span>Phí vận chuyển:</span>");
            html.AppendLine($"<span>{order.ShippingFee:N0} đ</span>");
            html.AppendLine("</div>");
            if (order.TaxAmount > 0)
            {
                html.AppendLine("<div class='summary-row'>");
                html.AppendLine("<span>VAT (10%):</span>");
                html.AppendLine($"<span>{order.TaxAmount:N0} đ</span>");
                html.AppendLine("</div>");
            }
            html.AppendLine("<div class='summary-row summary-total'>");
            html.AppendLine("<span>Tổng cộng:</span>");
            html.AppendLine($"<span>{order.TotalAmount:N0} đ</span>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");
            
            // Shipping Info
            html.AppendLine("<div class='shipping-info'>");
            html.AppendLine("<h3 style='margin-top: 0; color: #856404;'>📦 Thông tin giao hàng</h3>");
            html.AppendLine($"<p><strong>Người nhận:</strong> {order.ShippingName}</p>");
            html.AppendLine($"<p><strong>Số điện thoại:</strong> {order.ShippingPhone}</p>");
            html.AppendLine($"<p><strong>Địa chỉ:</strong> {order.ShippingAddress}</p>");
            html.AppendLine("</div>");
            
            // Notes
            if (!string.IsNullOrEmpty(order.CustomerNotes))
            {
                html.AppendLine("<div style='background: #e7f3ff; border-left: 4px solid #2196F3; padding: 15px; margin: 20px 0; border-radius: 4px;'>");
                html.AppendLine("<strong>Ghi chú của bạn:</strong>");
                html.AppendLine($"<p>{order.CustomerNotes}</p>");
                html.AppendLine("</div>");
            }
            
            // Footer message
            html.AppendLine("<p style='margin-top: 30px;'>Chúng tôi sẽ gửi email cập nhật khi đơn hàng của bạn được xử lý và vận chuyển.</p>");
            html.AppendLine("<p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với chúng tôi qua email hoặc hotline.</p>");
            
            html.AppendLine("</div>"); // End content
            
            // Footer
            html.AppendLine("<div class='footer'>");
            html.AppendLine("<p><strong>NovaStore</strong></p>");
            html.AppendLine("<p>140 Lê Trọng Tấn, Tây Thạnh, Tân Phú, TP. Hồ Chí Minh</p>");
            html.AppendLine("<p>Email: info@novastore.com | Hotline: 1900-xxxx</p>");
            html.AppendLine("<p>Cảm ơn bạn đã tin tưởng và sử dụng dịch vụ của chúng tôi!</p>");
            html.AppendLine("</div>");
            
            html.AppendLine("</div>"); // End container
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            
            return html.ToString();
        }

        private static string GetStatusText(string status)
        {
            switch (status?.ToLower())
            {
                case "pending": return "Chờ xác nhận";
                case "processing": return "Đang xử lý";
                case "shipping": return "Đang giao hàng";
                case "delivered": return "Đã giao hàng";
                case "cancelled": return "Đã hủy";
                case "completed": return "Hoàn thành";
                default: return status ?? "N/A";
            }
        }

        private static string GetPaymentStatusText(string paymentStatus)
        {
            switch (paymentStatus?.ToLower())
            {
                case "pending": return "Chờ thanh toán";
                case "paid": return "Đã thanh toán";
                case "failed": return "Thanh toán thất bại";
                case "refunded": return "Đã hoàn tiền";
                default: return paymentStatus ?? "N/A";
            }
        }

        /// <summary>
        /// Tạo HTML template cho email thông báo trạng thái đơn hàng
        /// </summary>
        private static string GenerateOrderStatusHtml(OrderDto order, string customerName, string statusTitle, string statusMessage)
        {
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset='UTF-8'>");
            html.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.AppendLine("<style>");
            html.AppendLine(@"
                body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; background-color: #f4f4f4; }
                .container { max-width: 600px; margin: 0 auto; background-color: #ffffff; }
                .header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; }
                .header h1 { margin: 0; font-size: 24px; }
                .content { padding: 30px; }
                .status-box { background-color: #f8f9fa; border-left: 4px solid #667eea; padding: 20px; margin: 20px 0; border-radius: 5px; }
                .status-box h2 { margin: 0 0 10px 0; color: #667eea; }
                .order-info { background-color: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0; }
                .order-info p { margin: 8px 0; }
                .footer { text-align: center; padding: 20px; background-color: #f8f9fa; color: #777; font-size: 14px; }
            ");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("<div class='container'>");
            
            // Header
            html.AppendLine("<div class='header'>");
            html.AppendLine("<h1>NovaStore</h1>");
            html.AppendLine("</div>");
            
            // Content
            html.AppendLine("<div class='content'>");
            html.AppendLine($"<p>Xin chào <strong>{customerName}</strong>,</p>");
            
            // Status Box
            html.AppendLine("<div class='status-box'>");
            html.AppendLine($"<h2>{statusTitle}</h2>");
            html.AppendLine($"<p>{statusMessage}</p>");
            html.AppendLine("</div>");
            
            // Order Info
            html.AppendLine("<div class='order-info'>");
            html.AppendLine("<h3 style='margin-top: 0;'>Thông tin đơn hàng:</h3>");
            html.AppendLine($"<p><strong>Mã đơn hàng:</strong> #{order.OrderNumber}</p>");
            html.AppendLine($"<p><strong>Ngày đặt:</strong> {order.OrderDate:dd/MM/yyyy HH:mm}</p>");
            html.AppendLine($"<p><strong>Trạng thái:</strong> {GetStatusText(order.Status)}</p>");
            html.AppendLine($"<p><strong>Tổng tiền:</strong> {order.TotalAmount:N0} đ</p>");
            html.AppendLine("</div>");
            
            html.AppendLine("<p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với chúng tôi.</p>");
            html.AppendLine("</div>");
            
            // Footer
            html.AppendLine("<div class='footer'>");
            html.AppendLine("<p>© 2024 NovaStore. Tất cả quyền được bảo lưu.</p>");
            html.AppendLine("<p>Email: support@novastore.com | Hotline: 1900-xxxx</p>");
            html.AppendLine("</div>");
            
            html.AppendLine("</div>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            
            return html.ToString();
        }

        private static string GetPaymentMethodText(string paymentMethod)
        {
            switch (paymentMethod?.ToLower())
            {
                case "cod": return "Thanh toán khi nhận hàng (COD)";
                case "bank_transfer": return "Chuyển khoản ngân hàng";
                case "credit_card": return "Thẻ tín dụng";
                case "paypal": return "PayPal";
                default: return paymentMethod ?? "N/A";
            }
        }
    }
}

