using AutoMapper;
using E_Commerce.Data.Infrastructure;
using E_Commerce.Data.Repositories;
using E_Commerce.Dto;
using E_Commerce.Model.Models;
using E_Commerce.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace E_Commerce.Service
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly ICartRepository _cartRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IProductRepository _productRepository;
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IProductImageRepository _productImageRepository;
        private readonly IProductVariantImageRepository _productVariantImageRepository;
        private readonly IDiscountCodeRepository _discountCodeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderService(
            IOrderRepository orderRepository,
            IOrderDetailRepository orderDetailRepository,
            ICartRepository cartRepository,
            ICartItemRepository cartItemRepository,
            IProductRepository productRepository,
            IProductVariantRepository productVariantRepository,
            IProductImageRepository productImageRepository,
            IProductVariantImageRepository productVariantImageRepository,
            IDiscountCodeRepository discountCodeRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _productRepository = productRepository;
            _productVariantRepository = productVariantRepository;
            _productImageRepository = productImageRepository;
            _productVariantImageRepository = productVariantImageRepository;
            _discountCodeRepository = discountCodeRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public OrderDto CreateOrderFromCart(int userId, OrderCreateDto orderCreateDto)
        {
            // Lấy giỏ hàng hiện tại
            var cart = _cartRepository.GetSingleByCondition(c => c.UserId == userId);
            if (cart == null)
            {
                throw new Exception("Giỏ hàng trống!");
            }

            var cartItems = _cartItemRepository.GetMulti(c => c.CartId == cart.Id).ToList();
            if (cartItems == null || !cartItems.Any())
            {
                throw new Exception("Giỏ hàng trống!");
            }

            // Nếu OrderDetails chưa có, lấy từ cart
            if (orderCreateDto.OrderDetails == null || !orderCreateDto.OrderDetails.Any())
            {
                orderCreateDto.OrderDetails = cartItems.Select(ci => new OrderDetailCreateDto
                {
                    ProductId = ci.ProductId,
                    ProductVariantId = ci.ProductVariantId,
                    Quantity = ci.Quantity
                }).ToList();
            }

            // Tính toán tổng tiền
            var subTotal = orderCreateDto.OrderDetails.Sum(od =>
            {
                var product = _productRepository.GetSingleById(od.ProductId);
                var variant = od.ProductVariantId.HasValue
                    ? _productVariantRepository.GetSingleById(od.ProductVariantId.Value)
                    : null;

                if (product == null || product.IsDeleted || !product.IsActive)
                    throw new Exception($"Sản phẩm ID {od.ProductId} không tồn tại hoặc đã ngừng bán!");
                if (variant != null && (variant.IsDeleted || !variant.IsActive))
                    throw new Exception($"Biến thể sản phẩm ID {variant.Id} không hợp lệ!");

                // Kiểm tra tồn kho
                var availableStock = variant != null ? variant.Stock : product.StockQuantity;
                if (availableStock <= 0)
                    throw new Exception($"Sản phẩm \"{product.Name}\" đã hết hàng!");
                if (od.Quantity > availableStock)
                    throw new Exception($"Sản phẩm \"{product.Name}\" chỉ còn {availableStock} trong kho.");

                var unitPrice = variant != null && variant.Price > 0 ? variant.Price : (product?.Price ?? 0);
                return unitPrice * od.Quantity;
            });

            // Tính toán discount amount nếu có discount code
            decimal discountAmount = 0;
            if (orderCreateDto.DiscountCodeId.HasValue)
            {
                var discountCode = _discountCodeRepository.GetSingleById(orderCreateDto.DiscountCodeId.Value);
                if (discountCode != null && !discountCode.IsDeleted && discountCode.IsActive)
                {
                    // Kiểm tra thời gian hiệu lực
                    var now = DateTime.Now;
                    if (discountCode.StartDate <= now && discountCode.EndDate >= now)
                    {
                        // Tính discount amount
                        if (discountCode.DiscountType == "Percentage")
                        {
                            discountAmount = subTotal * (discountCode.DiscountValue / 100);
                        }
                        else if (discountCode.DiscountType == "FixedAmount")
                        {
                            discountAmount = discountCode.DiscountValue;
                            // Không được giảm quá tổng tiền
                            if (discountAmount > subTotal)
                            {
                                discountAmount = subTotal;
                            }
                        }

                        // Tăng UsedCount của discount code
                        discountCode.UsedCount++;
                        discountCode.UpdatedDate = DateTime.Now;
                        _discountCodeRepository.Update(discountCode);
                    }
                }
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(orderCreateDto.ShippingName))
                throw new Exception("Họ và tên không được để trống!");
            if (string.IsNullOrWhiteSpace(orderCreateDto.ShippingPhone))
                throw new Exception("Số điện thoại không được để trống!");
            if (string.IsNullOrWhiteSpace(orderCreateDto.ShippingAddress))
                throw new Exception("Địa chỉ giao hàng không được để trống!");
            if (string.IsNullOrWhiteSpace(orderCreateDto.PaymentMethod))
                throw new Exception("Phương thức thanh toán không được để trống!");

            // Tạo đơn hàng mới
            // TotalAmount = FinalTotal từ giao diện (đã tính sẵn SubTotal + ShippingFee + TaxAmount)
            var order = new Order
            {
                UserId = userId,
                OrderNumber = GenerateOrderNumber(),
                OrderDate = DateTime.Now,
                SubTotal = subTotal,
                ShippingFee = orderCreateDto.ShippingFee,
                DiscountAmount = discountAmount,
                TotalAmount = orderCreateDto.FinalTotal > 0
                    ? orderCreateDto.FinalTotal
                    : (subTotal + orderCreateDto.ShippingFee + orderCreateDto.TaxAmount - discountAmount), // Trừ discount amount
                Status = "Pending",
                PaymentStatus = "Pending",
                PaymentMethod = orderCreateDto.PaymentMethod?.Trim() ?? string.Empty,
                ShippingName = orderCreateDto.ShippingName?.Trim() ?? string.Empty,
                ShippingPhone = orderCreateDto.ShippingPhone?.Trim() ?? string.Empty,
                ShippingAddress = orderCreateDto.ShippingAddress?.Trim() ?? string.Empty,
                TrackingNumber = null, // Sẽ được set khi ship
                DiscountCodeId = orderCreateDto.DiscountCodeId,
                CustomerNotes = !string.IsNullOrWhiteSpace(orderCreateDto.CustomerNotes)
                    ? orderCreateDto.CustomerNotes.Trim()
                    : null,
                CreatedDate = DateTime.Now
            };

            _orderRepository.Add(order);
            _unitOfWork.Commit();

            // Tạo chi tiết đơn hàng
            var orderDetails = new List<OrderDetail>();
            if (orderCreateDto.OrderDetails == null || !orderCreateDto.OrderDetails.Any())
            {
                throw new Exception("Không có sản phẩm nào trong đơn hàng!");
            }

            foreach (var detailDto in orderCreateDto.OrderDetails)
            {
                var product = _productRepository.GetSingleById(detailDto.ProductId);
                if (product == null)
                {
                    throw new Exception($"Sản phẩm ID {detailDto.ProductId} không tồn tại!");
                }

                var variant = detailDto.ProductVariantId.HasValue
                    ? _productVariantRepository.GetSingleById(detailDto.ProductVariantId.Value)
                    : null;

                if (product.IsDeleted || !product.IsActive)
                    throw new Exception($"Sản phẩm \"{product.Name}\" không còn bán!");
                if (variant != null && (variant.IsDeleted || !variant.IsActive))
                    throw new Exception($"Biến thể sản phẩm ID {variant.Id} không hợp lệ!");

                var unitPrice = variant != null && variant.Price > 0 ? variant.Price : product.Price;

                // Validate required fields for OrderDetail
                if (product.Name == null)
                    throw new Exception($"Tên sản phẩm ID {detailDto.ProductId} không hợp lệ!");
                if (detailDto.Quantity <= 0)
                    throw new Exception($"Số lượng sản phẩm phải lớn hơn 0!");

                // Kiểm tra tồn kho
                var availableStock = variant != null ? variant.Stock : product.StockQuantity;
                if (availableStock <= 0)
                    throw new Exception($"Sản phẩm \"{product.Name}\" đã hết hàng!");
                if (detailDto.Quantity > availableStock)
                    throw new Exception($"Sản phẩm \"{product.Name}\" chỉ còn {availableStock} trong kho.");

                // Lấy Size và Color từ variant
                string size = null;
                string color = null;
                if (variant != null)
                {
                    // Lấy Size
                    if (!string.IsNullOrWhiteSpace(variant.Size))
                    {
                        size = variant.Size.Trim();
                    }

                    // Lấy Color từ ColorName
                    if (!string.IsNullOrWhiteSpace(variant.ColorName))
                    {
                        color = variant.ColorName.Trim();
                    }
                    // Nếu không có ColorName, thử lấy từ ColorCode
                    else if (!string.IsNullOrWhiteSpace(variant.ColorCode))
                    {
                        color = variant.ColorCode.Trim();
                    }
                }

                var orderDetail = new OrderDetail
                {
                    OrderId = order.Id,
                    ProductId = detailDto.ProductId,
                    ProductVariantId = detailDto.ProductVariantId,
                    ProductName = product.Name.Trim(),
                    Quantity = detailDto.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = unitPrice * detailDto.Quantity,
                    Size = size, // Có thể null - DB cho phép NULL
                    Color = color, // Có thể null - DB cho phép NULL
                    CreatedDate = DateTime.Now
                };

                orderDetails.Add(orderDetail);
                _orderDetailRepository.Add(orderDetail);

                // Trừ tồn kho
                if (variant != null)
                {
                    variant.Stock -= detailDto.Quantity;
                    if (variant.Stock < 0) variant.Stock = 0;
                    variant.UpdatedDate = DateTime.Now;
                    _productVariantRepository.Update(variant);
                }
                else
                {
                    product.StockQuantity -= detailDto.Quantity;
                    if (product.StockQuantity < 0) product.StockQuantity = 0;
                    product.UpdatedDate = DateTime.Now;
                    _productRepository.Update(product);
                }
            }

            // Xóa giỏ hàng sau khi tạo đơn hàng thành công
            foreach (var cartItem in cartItems)
            {
                _cartItemRepository.Delete(cartItem);
            }
            _cartRepository.Delete(cart);

            // Commit tất cả: OrderDetails và xóa Cart
            _unitOfWork.Commit();

            // Map và trả về OrderDto
            var orderDto = _mapper.Map<Order, OrderDto>(order);

            // Map OrderDetails và thêm ảnh sản phẩm
            var orderDetailDtos = new List<OrderDetailDto>();
            foreach (var orderDetail in orderDetails)
            {
                var orderDetailDto = _mapper.Map<OrderDetail, OrderDetailDto>(orderDetail);

                // Lấy ảnh sản phẩm - ưu tiên ảnh variant nếu có, không thì lấy ảnh chính của product
                orderDetailDto.ProductImage = null; // Reset để đảm bảo logic fallback hoạt động đúng

                if (orderDetail.ProductVariantId.HasValue)
                {
                    // Ưu tiên ảnh variant có IsMain = true
                    var variantImage = _productVariantImageRepository.GetSingleByCondition(
                        vi => vi.ProductVariantId == orderDetail.ProductVariantId.Value && vi.IsMain);
                    if (variantImage != null && !string.IsNullOrWhiteSpace(variantImage.ImageUrl))
                    {
                        orderDetailDto.ProductImage = variantImage.ImageUrl;
                    }
                    else
                    {
                        // Nếu không có ảnh main, lấy ảnh đầu tiên của variant
                        var firstVariantImage = _productVariantImageRepository.GetSingleByCondition(
                            vi => vi.ProductVariantId == orderDetail.ProductVariantId.Value);
                        if (firstVariantImage != null && !string.IsNullOrWhiteSpace(firstVariantImage.ImageUrl))
                        {
                            orderDetailDto.ProductImage = firstVariantImage.ImageUrl;
                        }
                    }
                }

                // Nếu chưa có ảnh, lấy ảnh chính của product
                if (string.IsNullOrWhiteSpace(orderDetailDto.ProductImage))
                {
                    var mainProductImage = _productImageRepository.GetSingleByCondition(
                        pi => pi.ProductId == orderDetail.ProductId && pi.IsMain);
                    if (mainProductImage != null && !string.IsNullOrWhiteSpace(mainProductImage.ImageUrl))
                    {
                        orderDetailDto.ProductImage = mainProductImage.ImageUrl;
                    }
                    else
                    {
                        // Nếu không có ảnh main, lấy ảnh đầu tiên của product
                        var firstProductImage = _productImageRepository.GetSingleByCondition(
                            pi => pi.ProductId == orderDetail.ProductId);
                        if (firstProductImage != null && !string.IsNullOrWhiteSpace(firstProductImage.ImageUrl))
                        {
                            orderDetailDto.ProductImage = firstProductImage.ImageUrl;
                        }
                        else
                        {
                            orderDetailDto.ProductImage = "/Content/images/default-product.png"; // Default image
                        }
                    }
                }

                orderDetailDtos.Add(orderDetailDto);
            }
            orderDto.OrderDetails = orderDetailDtos;

            // Lấy thông tin user TRƯỚC khi vào Task.Run để tránh DbContext disposed
            string customerEmail = null;
            string customerName = null;
            try
            {
                var user = _userRepository.GetSingleById(userId);
                if (user != null && !string.IsNullOrWhiteSpace(user.Email))
                {
                    customerEmail = user.Email;
                    customerName = !string.IsNullOrWhiteSpace(orderDto.UserName)
                        ? orderDto.UserName
                        : (!string.IsNullOrWhiteSpace(user.FullName) ? user.FullName : user.Email);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to get user info for email: {ex.Message}");
            }

            // Gửi email xác nhận đơn hàng (fire and forget - không chờ kết quả)
            if (!string.IsNullOrWhiteSpace(customerEmail))
            {
                // Capture local variables để tránh closure issues
                var email = customerEmail;
                var name = customerName;
                var orderForEmail = orderDto; // Đổi tên để tránh conflict với biến 'order' ở scope ngoài

                Task.Run(async () =>
                {
                    try
                    {
                        await EmailHelper.SendOrderConfirmationAsync(
                            order: orderForEmail,
                            customerEmail: email,
                            customerName: name
                        );
                    }
                    catch (Exception ex)
                    {
                        // Log lỗi nhưng không throw để không ảnh hưởng đến quá trình đặt hàng
                        System.Diagnostics.Debug.WriteLine($"Failed to send order confirmation email: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                    }
                });
            }

            return orderDto;
        }

        public OrderDto GetOrderById(int orderId)
        {
            var order = _orderRepository.GetSingleByCondition(o => o.Id == orderId);
            if (order == null)
            {
                return null;
            }

            var orderDto = _mapper.Map<Order, OrderDto>(order);
            var orderDetails = _orderDetailRepository.GetMulti(od => od.OrderId == orderId).ToList();
            var orderDetailDtos = new List<OrderDetailDto>();

            foreach (var orderDetail in orderDetails)
            {
                var orderDetailDto = _mapper.Map<OrderDetail, OrderDetailDto>(orderDetail);

                // Lấy ảnh sản phẩm - ưu tiên ảnh variant nếu có, không thì lấy ảnh chính của product
                orderDetailDto.ProductImage = null; // Reset để đảm bảo logic fallback hoạt động đúng

                if (orderDetail.ProductVariantId.HasValue)
                {
                    // Ưu tiên ảnh variant có IsMain = true
                    var variantImage = _productVariantImageRepository.GetSingleByCondition(
                        vi => vi.ProductVariantId == orderDetail.ProductVariantId.Value && vi.IsMain);
                    if (variantImage != null && !string.IsNullOrWhiteSpace(variantImage.ImageUrl))
                    {
                        orderDetailDto.ProductImage = variantImage.ImageUrl;
                    }
                    else
                    {
                        // Nếu không có ảnh main, lấy ảnh đầu tiên của variant
                        var firstVariantImage = _productVariantImageRepository.GetSingleByCondition(
                            vi => vi.ProductVariantId == orderDetail.ProductVariantId.Value);
                        if (firstVariantImage != null && !string.IsNullOrWhiteSpace(firstVariantImage.ImageUrl))
                        {
                            orderDetailDto.ProductImage = firstVariantImage.ImageUrl;
                        }
                    }
                }

                // Nếu chưa có ảnh, lấy ảnh chính của product
                if (string.IsNullOrWhiteSpace(orderDetailDto.ProductImage))
                {
                    var mainProductImage = _productImageRepository.GetSingleByCondition(
                        pi => pi.ProductId == orderDetail.ProductId && pi.IsMain);
                    if (mainProductImage != null && !string.IsNullOrWhiteSpace(mainProductImage.ImageUrl))
                    {
                        orderDetailDto.ProductImage = mainProductImage.ImageUrl;
                    }
                    else
                    {
                        // Nếu không có ảnh main, lấy ảnh đầu tiên của product
                        var firstProductImage = _productImageRepository.GetSingleByCondition(
                            pi => pi.ProductId == orderDetail.ProductId);
                        if (firstProductImage != null && !string.IsNullOrWhiteSpace(firstProductImage.ImageUrl))
                        {
                            orderDetailDto.ProductImage = firstProductImage.ImageUrl;
                        }
                        else
                        {
                            orderDetailDto.ProductImage = "/Content/images/default-product.png"; // Default image
                        }
                    }
                }

                orderDetailDtos.Add(orderDetailDto);
            }
            orderDto.OrderDetails = orderDetailDtos;

            return orderDto;
        }

        public void UpdateOrder(OrderDto orderDto)
        {
            var order = _orderRepository.GetSingleById(orderDto.Id);
            if (order == null) throw new Exception("Order not found");

            order.Status = orderDto.Status;
            order.PaymentStatus = orderDto.PaymentStatus;
            order.UpdatedDate = DateTime.Now;

            _orderRepository.Update(order);
            _unitOfWork.Commit();
        }

        public List<OrderDto> GetOrdersByUserId(int userId)
        {
            var orders = _orderRepository.GetMulti(o => o.UserId == userId).OrderByDescending(o => o.OrderDate).ToList();
            var orderDtos = new List<OrderDto>();

            foreach (var order in orders)
            {
                var orderDto = _mapper.Map<Order, OrderDto>(order);
                var orderDetails = _orderDetailRepository.GetMulti(od => od.OrderId == order.Id).ToList();
                var orderDetailDtos = new List<OrderDetailDto>();

                foreach (var orderDetail in orderDetails)
                {
                    var orderDetailDto = _mapper.Map<OrderDetail, OrderDetailDto>(orderDetail);

                    // Lấy ảnh sản phẩm - ưu tiên ảnh variant nếu có, không thì lấy ảnh chính của product
                    orderDetailDto.ProductImage = null; // Reset để đảm bảo logic fallback hoạt động đúng

                    if (orderDetail.ProductVariantId.HasValue)
                    {
                        // Ưu tiên ảnh variant có IsMain = true
                        var variantImage = _productVariantImageRepository.GetSingleByCondition(
                            vi => vi.ProductVariantId == orderDetail.ProductVariantId.Value && vi.IsMain);
                        if (variantImage != null && !string.IsNullOrWhiteSpace(variantImage.ImageUrl))
                        {
                            orderDetailDto.ProductImage = variantImage.ImageUrl;
                        }
                        else
                        {
                            // Nếu không có ảnh main, lấy ảnh đầu tiên của variant
                            var firstVariantImage = _productVariantImageRepository.GetSingleByCondition(
                                vi => vi.ProductVariantId == orderDetail.ProductVariantId.Value);
                            if (firstVariantImage != null && !string.IsNullOrWhiteSpace(firstVariantImage.ImageUrl))
                            {
                                orderDetailDto.ProductImage = firstVariantImage.ImageUrl;
                            }
                        }
                    }

                    // Nếu chưa có ảnh, lấy ảnh chính của product
                    if (string.IsNullOrWhiteSpace(orderDetailDto.ProductImage))
                    {
                        var mainProductImage = _productImageRepository.GetSingleByCondition(
                            pi => pi.ProductId == orderDetail.ProductId && pi.IsMain);
                        if (mainProductImage != null && !string.IsNullOrWhiteSpace(mainProductImage.ImageUrl))
                        {
                            orderDetailDto.ProductImage = mainProductImage.ImageUrl;
                        }
                        else
                        {
                            // Nếu không có ảnh main, lấy ảnh đầu tiên của product
                            var firstProductImage = _productImageRepository.GetSingleByCondition(
                                pi => pi.ProductId == orderDetail.ProductId);
                            if (firstProductImage != null && !string.IsNullOrWhiteSpace(firstProductImage.ImageUrl))
                            {
                                orderDetailDto.ProductImage = firstProductImage.ImageUrl;
                            }
                            else
                            {
                                orderDetailDto.ProductImage = "/Content/images/default-product.png"; // Default image
                            }
                        }
                    }

                    orderDetailDtos.Add(orderDetailDto);
                }
                orderDto.OrderDetails = orderDetailDtos;
                orderDtos.Add(orderDto);
            }

            return orderDtos;
        }

        public string GenerateOrderNumber()
        {
            // Format: ORD + YYYYMMDD + 6 số random
            var dateStr = DateTime.Now.ToString("yyyyMMdd");
            var random = new Random();
            var randomStr = random.Next(100000, 999999).ToString();
            return $"ORD{dateStr}{randomStr}";
        }

        public List<OrderDto> GetAllOrders()
        {
            var orders = _orderRepository.GetAll().OrderByDescending(o => o.OrderDate).ToList();
            return orders.Select(o => _mapper.Map<Order, OrderDto>(o)).ToList();
        }

        #region Status Transition Methods

        /// <summary>
        /// Xác nhận đơn hàng: Pending -> Processing
        /// </summary>
        public bool ConfirmOrder(int orderId)
        {
            var order = _orderRepository.GetSingleById(orderId);
            if (order == null)
                throw new Exception("Đơn hàng không tồn tại!");

            if (!string.Equals(order.Status, "Pending", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Chỉ có thể xác nhận đơn hàng đang ở trạng thái 'Chờ xác nhận'!");

            order.Status = "Processing";
            order.UpdatedDate = DateTime.Now;

            _orderRepository.Update(order);
            _unitOfWork.Commit();

            // Gửi email thông báo đơn hàng đã được xác nhận
            SendOrderStatusEmailAsync(orderId, "confirmed");

            return true;
        }

        /// <summary>
        /// Bắt đầu giao hàng: Processing -> Shipping
        /// </summary>
        public bool StartShipping(int orderId, string trackingNumber = null)
        {
            var order = _orderRepository.GetSingleById(orderId);
            if (order == null)
                throw new Exception("Đơn hàng không tồn tại!");

            if (!string.Equals(order.Status, "Processing", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Chỉ có thể giao đơn hàng đang ở trạng thái 'Đang xử lý'!");

            order.Status = "Shipping";
            order.ShippedDate = DateTime.Now;
            order.TrackingNumber = trackingNumber;
            order.UpdatedDate = DateTime.Now;

            _orderRepository.Update(order);
            _unitOfWork.Commit();

            // Gửi email thông báo đơn hàng đã được giao
            SendOrderStatusEmailAsync(orderId, "shipped", trackingNumber);

            return true;
        }

        /// <summary>
        /// Đánh dấu đã giao: Shipping -> Delivered
        /// </summary>
        public bool MarkDelivered(int orderId)
        {
            var order = _orderRepository.GetSingleById(orderId);
            if (order == null)
                throw new Exception("Đơn hàng không tồn tại!");

            if (!string.Equals(order.Status, "Shipping", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Chỉ có thể đánh dấu đã giao cho đơn hàng đang ở trạng thái 'Đang giao'!");

            order.Status = "Delivered";
            order.DeliveredDate = DateTime.Now;
            order.UpdatedDate = DateTime.Now;

            // Nếu là COD, tự động đánh dấu đã thanh toán
            if (string.Equals(order.PaymentMethod, "COD", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(order.PaymentMethod, "Thanh toán khi nhận hàng", StringComparison.OrdinalIgnoreCase))
            {
                order.PaymentStatus = "Paid";
            }

            _orderRepository.Update(order);
            _unitOfWork.Commit();

            // Gửi email thông báo đơn hàng đã được giao thành công
            SendOrderStatusEmailAsync(orderId, "delivered");

            return true;
        }

        /// <summary>
        /// Hủy đơn hàng: Pending/Processing -> Cancelled
        /// </summary>
        public bool CancelOrder(int orderId, string reason = null)
        {
            var order = _orderRepository.GetSingleById(orderId);
            if (order == null)
                throw new Exception("Đơn hàng không tồn tại!");

            var currentStatus = order.Status?.ToLower();
            if (currentStatus != "pending" && currentStatus != "processing")
                throw new Exception("Chỉ có thể hủy đơn hàng đang ở trạng thái 'Chờ xác nhận' hoặc 'Đang xử lý'!");

            // Hoàn tồn kho cho tất cả order details
            var orderDetails = _orderDetailRepository.GetMulti(od => od.OrderId == orderId).ToList();
            foreach (var od in orderDetails)
            {
                if (od.ProductVariantId.HasValue)
                {
                    var variant = _productVariantRepository.GetSingleById(od.ProductVariantId.Value);
                    if (variant != null)
                    {
                        variant.Stock += od.Quantity;
                        variant.UpdatedDate = DateTime.Now;
                        _productVariantRepository.Update(variant);
                    }
                    else
                    {
                        // fallback: nếu variant bị xóa, cộng vào product
                        var product = _productRepository.GetSingleById(od.ProductId);
                        if (product != null)
                        {
                            product.StockQuantity += od.Quantity;
                            product.UpdatedDate = DateTime.Now;
                            _productRepository.Update(product);
                        }
                    }
                }
                else
                {
                    var product = _productRepository.GetSingleById(od.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += od.Quantity;
                        product.UpdatedDate = DateTime.Now;
                        _productRepository.Update(product);
                    }
                }
            }

            order.Status = "Cancelled";
            order.CancelledAt = DateTime.Now;
            order.CancelReason = reason;
            order.UpdatedDate = DateTime.Now;

            // TODO: Hoàn tiền nếu đã thanh toán online

            _orderRepository.Update(order);
            _unitOfWork.Commit();

            // Gửi email thông báo đơn hàng đã bị hủy
            SendOrderStatusEmailAsync(orderId, "cancelled", null, reason);

            return true;
        }

        /// <summary>
        /// Helper method để gửi email thông báo trạng thái đơn hàng (fire and forget)
        /// </summary>
        private void SendOrderStatusEmailAsync(int orderId, string statusType, string trackingNumber = null, string cancelReason = null)
        {
            try
            {
                // Lấy thông tin order và user TRƯỚC khi vào Task.Run để tránh DbContext disposed
                var order = _orderRepository.GetSingleById(orderId);
                if (order == null) return;

                var orderDto = GetOrderById(orderId);
                if (orderDto == null) return;

                var user = _userRepository.GetSingleById(order.UserId);
                if (user == null || string.IsNullOrWhiteSpace(user.Email)) return;

                string customerEmail = user.Email;
                string customerName = !string.IsNullOrWhiteSpace(orderDto.UserName)
                    ? orderDto.UserName
                    : (!string.IsNullOrWhiteSpace(user.FullName) ? user.FullName : user.Email);

                // Capture local variables để tránh closure issues
                var email = customerEmail;
                var name = customerName;
                var orderForEmail = orderDto;
                var tracking = trackingNumber;
                var reason = cancelReason;

                Task.Run(async () =>
                {
                    try
                    {
                        switch (statusType.ToLower())
                        {
                            case "confirmed":
                                await EmailHelper.SendOrderConfirmedAsync(orderForEmail, email, name);
                                break;
                            case "shipped":
                                await EmailHelper.SendOrderShippedAsync(orderForEmail, email, name, tracking);
                                break;
                            case "delivered":
                                await EmailHelper.SendOrderDeliveredAsync(orderForEmail, email, name);
                                break;
                            case "cancelled":
                                await EmailHelper.SendOrderCancelledAsync(orderForEmail, email, name, reason);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to send order status email ({statusType}): {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to prepare order status email ({statusType}): {ex.Message}");
            }
        }

        #endregion Status Transition Methods

        #region Payment Methods

        /// <summary>
        /// Đánh dấu đã thanh toán
        /// </summary>
        public bool MarkPaid(int orderId)
        {
            var order = _orderRepository.GetSingleById(orderId);
            if (order == null)
                throw new Exception("Đơn hàng không tồn tại!");

            if (string.Equals(order.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Đơn hàng đã được thanh toán!");

            // Chỉ cho phép đánh dấu thanh toán khi đơn đã được xác nhận/đang giao/đã giao
            var status = order.Status?.ToLower();
            if (status == "pending" || status == "cancelled")
                throw new Exception("Chỉ đánh dấu thanh toán khi đơn đã được xác nhận.");

            order.PaymentStatus = "Paid";
            order.UpdatedDate = DateTime.Now;

            // Nếu đang Pending, tự động chuyển sang Processing
            if (string.Equals(order.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                order.Status = "Processing";
            }

            _orderRepository.Update(order);
            _unitOfWork.Commit();
            return true;
        }

        /// <summary>
        /// Đánh dấu đã hoàn tiền
        /// </summary>
        public bool MarkRefunded(int orderId)
        {
            var order = _orderRepository.GetSingleById(orderId);
            if (order == null)
                throw new Exception("Đơn hàng không tồn tại!");

            if (!string.Equals(order.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Chỉ có thể hoàn tiền cho đơn hàng đã thanh toán!");

            order.PaymentStatus = "Refunded";
            order.UpdatedDate = DateTime.Now;

            _orderRepository.Update(order);
            _unitOfWork.Commit();
            return true;
        }

        #endregion Payment Methods

        #region Report Methods

        public decimal GetRevenueByDateRange(DateTime startDate, DateTime endDate)
        {
            var orders = _orderRepository.GetMulti(o => 
                o.OrderDate >= startDate && 
                o.OrderDate <= endDate && 
                o.Status != "Cancelled" &&
                o.PaymentStatus == "Paid")
                .ToList();
            
            return orders.Sum(o => o.TotalAmount);
        }

        public int GetOrderCountByDateRange(DateTime startDate, DateTime endDate)
        {
            return _orderRepository.GetMulti(o => 
                o.OrderDate >= startDate && 
                o.OrderDate <= endDate && 
                o.Status != "Cancelled")
                .Count();
        }

        public int GetOrderCountByStatus(string status)
        {
            return _orderRepository.GetMulti(o => 
                o.Status != null && 
                o.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                .Count();
        }

        public List<OrderDto> GetOrdersByDateRange(DateTime startDate, DateTime endDate)
        {
            var orders = _orderRepository.GetMulti(o => 
                o.OrderDate >= startDate && 
                o.OrderDate <= endDate)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
            
            return orders.Select(o => _mapper.Map<Order, OrderDto>(o)).ToList();
        }

        public Dictionary<string, decimal> GetRevenueByMonth(int year)
        {
            var result = new Dictionary<string, decimal>();
            for (int month = 1; month <= 12; month++)
            {
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);
                var revenue = GetRevenueByDateRange(startDate, endDate);
                result.Add(month.ToString("00"), revenue);
            }
            return result;
        }

        public Dictionary<string, int> GetOrderCountByMonth(int year)
        {
            var result = new Dictionary<string, int>();
            for (int month = 1; month <= 12; month++)
            {
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);
                var count = GetOrderCountByDateRange(startDate, endDate);
                result.Add(month.ToString("00"), count);
            }
            return result;
        }

        #endregion Report Methods
    }
}