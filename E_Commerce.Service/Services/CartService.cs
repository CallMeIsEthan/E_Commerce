using AutoMapper;
using E_Commerce.Data.Infrastructure;
using E_Commerce.Data.Repositories;
using E_Commerce.Dto;
using E_Commerce.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace E_Commerce.Service
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IProductRepository _productRepository;
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IProductImageRepository _productImageRepository;
        private readonly IProductVariantImageRepository _productVariantImageRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CartService(
            ICartRepository cartRepository,
            ICartItemRepository cartItemRepository,
            IProductRepository productRepository,
            IProductVariantRepository productVariantRepository,
            IProductImageRepository productImageRepository,
            IProductVariantImageRepository productVariantImageRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _productRepository = productRepository;
            _productVariantRepository = productVariantRepository;
            _productImageRepository = productImageRepository;
            _productVariantImageRepository = productVariantImageRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public CartDto GetCartByUserId(int userId)
        {
            var cart = _cartRepository.GetSingleByCondition(c => c.UserId == userId);
            
            if (cart == null)
            {
                // Tạo giỏ hàng mới nếu chưa có
                cart = new Cart
                {
                    UserId = userId,
                    CreatedDate = DateTime.Now
                };
                _cartRepository.Add(cart);
                _unitOfWork.Commit();
            }

            var cartDto = _mapper.Map<Cart, CartDto>(cart);
            
            // Lấy tất cả items trong giỏ hàng
            var cartItems = _cartItemRepository.GetMulti(ci => ci.CartId == cart.Id)
                .ToList();

            var cartItemDtos = new List<CartItemDto>();
            decimal totalAmount = 0;

            foreach (var item in cartItems)
            {
                var product = _productRepository.GetSingleById(item.ProductId);
                if (product == null || !product.IsActive) continue;

                var cartItemDto = _mapper.Map<CartItem, CartItemDto>(item);
                cartItemDto.ProductName = product.Name;

                // Lấy ảnh sản phẩm ưu tiên theo variant
                cartItemDto.ProductImage = GetProductImage(product.Id, item.ProductVariantId);

                // Lấy thông tin variant và số lượng tồn kho
                int availableStock;
                if (item.ProductVariantId.HasValue)
                {
                    var variant = _productVariantRepository.GetSingleById(item.ProductVariantId.Value);
                    if (variant != null)
                    {
                        cartItemDto.Size = variant.Size;
                        cartItemDto.Color = !string.IsNullOrWhiteSpace(variant.ColorName)
                            ? variant.ColorName
                            : variant.ColorCode;
                        availableStock = variant.Stock;
                    }
                    else
                    {
                        availableStock = product.StockQuantity;
                    }
                }
                else
                {
                    availableStock = product.StockQuantity;
                }
                
                cartItemDto.AvailableStock = availableStock;
                cartItemDto.TotalPrice = item.UnitPrice * item.Quantity;
                totalAmount += cartItemDto.TotalPrice;

                cartItemDtos.Add(cartItemDto);
            }

            cartDto.CartItems = cartItemDtos;
            cartDto.TotalAmount = totalAmount;
            cartDto.TotalItems = cartItemDtos.Sum(ci => ci.Quantity);
            
            // Tính phí vận chuyển: Miễn phí nếu >= 1.000.000đ, nếu không thì 50.000đ
            const decimal FREE_SHIPPING_THRESHOLD = 1000000m;
            const decimal SHIPPING_FEE = 50000m;
            
            if (totalAmount >= FREE_SHIPPING_THRESHOLD)
            {
                cartDto.ShippingFee = 0;
            }
            else
            {
                cartDto.ShippingFee = SHIPPING_FEE;
            }
            
            // Tính thuế VAT 10% trên tổng (tạm tính + phí vận chuyển)
            const decimal VAT_RATE = 0.1m; // 10%
            cartDto.TaxAmount = (cartDto.TotalAmount + cartDto.ShippingFee) * VAT_RATE;
            
            // Tổng cộng = Tạm tính + Phí vận chuyển + Thuế VAT
            cartDto.FinalTotal = cartDto.TotalAmount + cartDto.ShippingFee + cartDto.TaxAmount;

            return cartDto;
        }

        public CartItemDto AddToCart(int userId, CartItemCreateDto cartItemCreateDto)
        {
            // Validate sản phẩm
            var product = _productRepository.GetSingleById(cartItemCreateDto.ProductId);
            if (product == null || product.IsDeleted || !product.IsActive)
            {
                throw new Exception("Sản phẩm không tồn tại hoặc đã ngừng bán");
            }

            // Nếu không có ProductVariantId, tự động lấy variant mặc định (variant đầu tiên còn hàng)
            int? variantId = cartItemCreateDto.ProductVariantId;
            if (!variantId.HasValue)
            {
                var variants = _productVariantRepository.GetMulti(v => 
                    v.ProductId == product.Id && 
                    v.IsActive && 
                    !v.IsDeleted)
                    .OrderBy(v => v.Stock > 0 ? 0 : 1) // Ưu tiên variant còn hàng
                    .ThenBy(v => v.Id)
                    .ToList();
                
                if (variants.Any())
                {
                    // Lấy variant đầu tiên còn hàng, nếu không có thì lấy variant đầu tiên
                    var defaultVariant = variants.FirstOrDefault(v => v.Stock > 0) ?? variants.First();
                    variantId = defaultVariant.Id;
                }
            }

            // Validate variant nếu có
            decimal unitPrice = product.Price;
            int availableStock;
            if (variantId.HasValue)
            {
                var variant = _productVariantRepository.GetSingleById(variantId.Value);
                if (variant == null || variant.IsDeleted || !variant.IsActive)
                {
                    throw new Exception("Biến thể sản phẩm không hợp lệ");
                }
                availableStock = variant.Stock;
                // Nếu variant có giá, dùng giá variant; nếu không, dùng giá sản phẩm
                unitPrice = variant.Price > 0 ? variant.Price : product.Price;
            }
            else
            {
                availableStock = product.StockQuantity;
            }

            // Lấy hoặc tạo giỏ hàng
            var cart = _cartRepository.GetSingleByCondition(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedDate = DateTime.Now
                };
                _cartRepository.Add(cart);
                _unitOfWork.Commit();
            }

            // Kiểm tra xem sản phẩm đã có trong giỏ hàng chưa
            var existingItem = _cartItemRepository.GetSingleByCondition(
                ci => ci.CartId == cart.Id 
                && ci.ProductId == cartItemCreateDto.ProductId
                && ci.ProductVariantId == variantId);

            // Kiểm tra tồn kho (bao gồm cả trường hợp đã có sẵn trong giỏ)
            var newQuantity = (existingItem?.Quantity ?? 0) + cartItemCreateDto.Quantity;
            if (availableStock <= 0)
            {
                throw new Exception("Sản phẩm đã hết hàng");
            }
            if (newQuantity > availableStock)
            {
                throw new Exception($"Chỉ còn {availableStock} sản phẩm trong kho");
            }

            if (existingItem != null)
            {
                // Cập nhật số lượng
                existingItem.Quantity = newQuantity;
                existingItem.UpdatedDate = DateTime.Now;
                _cartItemRepository.Update(existingItem);
            }
            else
            {
                // Thêm mới
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = cartItemCreateDto.ProductId,
                    ProductVariantId = variantId,
                    Quantity = cartItemCreateDto.Quantity,
                    UnitPrice = unitPrice,
                    CreatedDate = DateTime.Now
                };
                _cartItemRepository.Add(cartItem);
                existingItem = cartItem;
            }

            _unitOfWork.Commit();

            // Map và trả về
            var cartItemDto = _mapper.Map<CartItem, CartItemDto>(existingItem);
            cartItemDto.ProductName = product.Name;
            cartItemDto.TotalPrice = existingItem.UnitPrice * existingItem.Quantity;
            cartItemDto.ProductImage = GetProductImage(product.Id, existingItem.ProductVariantId);
            cartItemDto.AvailableStock = availableStock; // Set available stock

            // Lấy variant info
            if (existingItem.ProductVariantId.HasValue)
            {
                var variant = _productVariantRepository.GetSingleById(existingItem.ProductVariantId.Value);
                if (variant != null)
                {
                    cartItemDto.Size = variant.Size;
                    cartItemDto.Color = !string.IsNullOrWhiteSpace(variant.ColorName)
                        ? variant.ColorName
                        : variant.ColorCode;
                }
            }

            return cartItemDto;
        }

        public CartItemDto UpdateCartItem(int cartItemId, int quantity)
        {
            if (quantity <= 0)
            {
                throw new Exception("Số lượng phải lớn hơn 0");
            }

            var cartItem = _cartItemRepository.GetSingleById(cartItemId);
            if (cartItem == null)
            {
                throw new Exception("Không tìm thấy sản phẩm trong giỏ hàng");
            }

            // Kiểm tra tồn kho trước khi cập nhật
            var product = _productRepository.GetSingleById(cartItem.ProductId);
            if (product == null || product.IsDeleted || !product.IsActive)
            {
                throw new Exception("Sản phẩm không tồn tại hoặc đã ngừng bán");
            }

            int availableStock;
            if (cartItem.ProductVariantId.HasValue)
            {
                var variant = _productVariantRepository.GetSingleById(cartItem.ProductVariantId.Value);
                if (variant == null || variant.IsDeleted || !variant.IsActive)
                {
                    throw new Exception("Biến thể sản phẩm không hợp lệ");
                }
                availableStock = variant.Stock;
            }
            else
            {
                availableStock = product.StockQuantity;
            }

            if (availableStock <= 0)
            {
                throw new Exception("Sản phẩm đã hết hàng");
            }
            if (quantity > availableStock)
            {
                throw new Exception($"Chỉ còn {availableStock} sản phẩm trong kho");
            }

            cartItem.Quantity = quantity;
            cartItem.UpdatedDate = DateTime.Now;
            _cartItemRepository.Update(cartItem);
            _unitOfWork.Commit();

            var cartItemDto = _mapper.Map<CartItem, CartItemDto>(cartItem);
            cartItemDto.ProductName = product.Name;
            cartItemDto.TotalPrice = cartItem.UnitPrice * cartItem.Quantity;
            cartItemDto.ProductImage = GetProductImage(cartItem.ProductId, cartItem.ProductVariantId);
            cartItemDto.AvailableStock = availableStock; // Set available stock
            if (cartItem.ProductVariantId.HasValue)
            {
                var variant = _productVariantRepository.GetSingleById(cartItem.ProductVariantId.Value);
                if (variant != null)
                {
                    cartItemDto.Size = variant.Size;
                    cartItemDto.Color = !string.IsNullOrWhiteSpace(variant.ColorName)
                        ? variant.ColorName
                        : variant.ColorCode;
                }
            }

            return cartItemDto;
        }

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

        public bool RemoveFromCart(int cartItemId)
        {
            var cartItem = _cartItemRepository.GetSingleById(cartItemId);
            if (cartItem == null)
            {
                return false;
            }

            _cartItemRepository.Delete(cartItem);
            _unitOfWork.Commit();
            return true;
        }

        public bool ClearCart(int userId)
        {
            var cart = _cartRepository.GetSingleByCondition(c => c.UserId == userId);
            if (cart == null)
            {
                return false;
            }

            var cartItems = _cartItemRepository.GetMulti(ci => ci.CartId == cart.Id).ToList();
            foreach (var item in cartItems)
            {
                _cartItemRepository.Delete(item);
            }

            _unitOfWork.Commit();
            return true;
        }

        public int GetCartItemCount(int userId)
        {
            var cart = _cartRepository.GetSingleByCondition(c => c.UserId == userId);
            if (cart == null)
            {
                return 0;
            }

            return _cartItemRepository.GetMulti(ci => ci.CartId == cart.Id)
                .Sum(ci => ci.Quantity);
        }
    }
}

