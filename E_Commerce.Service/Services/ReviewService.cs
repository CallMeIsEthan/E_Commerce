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
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ReviewService(
            IReviewRepository reviewRepository,
            IProductRepository productRepository,
            IUserRepository userRepository,
            IOrderRepository orderRepository,
            IOrderDetailRepository orderDetailRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _reviewRepository = reviewRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Tạo đánh giá mới
        /// </summary>
        public ReviewDto Create(ReviewCreateDto reviewCreateDto)
        {
            // Validate product exists
            var product = _productRepository.GetSingleById(reviewCreateDto.ProductId);
            if (product == null)
            {
                throw new Exception("Sản phẩm không tồn tại");
            }

            // Validate user exists
            var user = _userRepository.GetSingleById(reviewCreateDto.UserId);
            if (user == null)
            {
                throw new Exception("Người dùng không tồn tại");
            }

            // Validate rating range
            if (reviewCreateDto.Rating < 1 || reviewCreateDto.Rating > 5)
            {
                throw new Exception("Đánh giá phải từ 1 đến 5 sao");
            }

            // Check if user has already reviewed this product (exclude deleted)
            if (HasUserReviewedProduct(reviewCreateDto.UserId, reviewCreateDto.ProductId))
            {
                throw new Exception("Bạn đã đánh giá sản phẩm này rồi");
            }

            // Check if user has purchased this product - chỉ cho phép người đã mua hàng mới được đánh giá
            if (!HasUserPurchasedProduct(reviewCreateDto.UserId, reviewCreateDto.ProductId))
            {
                throw new Exception("Bạn cần mua sản phẩm trước khi đánh giá");
            }

            // Map từ DTO sang Model
            var review = _mapper.Map<ReviewCreateDto, Review>(reviewCreateDto);
            review.CreatedDate = DateTime.Now;
            review.IsDeleted = false;
            review.IsApproved = false; // Chờ duyệt

            // Thêm vào database
            _reviewRepository.Add(review);
            _unitOfWork.Commit();

            // Map lại sang DTO để trả về
            var reviewDto = _mapper.Map<Review, ReviewDto>(review);
            reviewDto.ProductName = product.Name;
            reviewDto.UserName = !string.IsNullOrEmpty(user.FullName) ? user.FullName : user.Email;
            return reviewDto;
        }

        /// <summary>
        /// Cập nhật đánh giá
        /// </summary>
        public ReviewDto Update(int id, ReviewCreateDto reviewUpdateDto)
        {
            var review = _reviewRepository.GetSingleById(id);
            if (review == null || review.IsDeleted)
            {
                throw new Exception("Đánh giá không tồn tại");
            }

            // Chỉ cho phép cập nhật nếu là chủ sở hữu
            if (review.UserId != reviewUpdateDto.UserId)
            {
                throw new Exception("Bạn không có quyền cập nhật đánh giá này");
            }

            // Validate rating range
            if (reviewUpdateDto.Rating < 1 || reviewUpdateDto.Rating > 5)
            {
                throw new Exception("Đánh giá phải từ 1 đến 5 sao");
            }

            // Cập nhật thông tin
            review.Rating = reviewUpdateDto.Rating;
            review.Comment = reviewUpdateDto.Comment;
            review.UpdatedDate = DateTime.Now;
            review.IsApproved = false; // Reset về chờ duyệt sau khi cập nhật

            _reviewRepository.Update(review);
            _unitOfWork.Commit();

            return _mapper.Map<Review, ReviewDto>(review);
        }

        /// <summary>
        /// Xóa đánh giá
        /// </summary>
        public bool Delete(int id)
        {
            var review = _reviewRepository.GetSingleById(id);
            if (review == null)
            {
                throw new Exception("Đánh giá không tồn tại");
            }

            // Soft delete: set IsDeleted = true
            review.IsDeleted = true;
            review.UpdatedDate = DateTime.Now;
            _reviewRepository.Update(review);
            _unitOfWork.Commit();

            return true;
        }

        /// <summary>
        /// Lấy đánh giá theo ID
        /// </summary>
        public ReviewDto GetById(int id)
        {
            var review = _reviewRepository.GetSingleByCondition(r => r.Id == id && !r.IsDeleted, new[] { "Product", "User" });
            if (review == null)
            {
                return null;
            }

            var reviewDto = _mapper.Map<Review, ReviewDto>(review);
            if (review.Product != null)
            {
                reviewDto.ProductName = review.Product.Name;
            }
            if (review.User != null)
            {
                reviewDto.UserName = !string.IsNullOrEmpty(review.User.FullName) ? review.User.FullName : review.User.Email;
            }
            return reviewDto;
        }

        /// <summary>
        /// Lấy tất cả đánh giá của một sản phẩm (chỉ lấy đã duyệt)
        /// </summary>
        public List<ReviewDto> GetByProductId(int productId)
        {
            var reviews = _reviewRepository.GetMulti(
                r => r.ProductId == productId && r.IsApproved && !r.IsDeleted,
                new[] { "User" }
            ).OrderByDescending(r => r.CreatedDate).ToList();

            return MapReviewsToDto(reviews);
        }

        /// <summary>
        /// Lấy tất cả đánh giá của một sản phẩm (bao gồm cả chưa duyệt - cho Admin)
        /// </summary>
        public List<ReviewDto> GetAllByProductId(int productId)
        {
            var reviews = _reviewRepository.GetMulti(
                r => r.ProductId == productId && !r.IsDeleted,
                new[] { "Product", "User" }
            ).OrderByDescending(r => r.CreatedDate).ToList();

            return MapReviewsToDto(reviews);
        }

        /// <summary>
        /// Lấy tất cả đánh giá của một người dùng
        /// </summary>
        public List<ReviewDto> GetByUserId(int userId)
        {
            var reviews = _reviewRepository.GetMulti(
                r => r.UserId == userId && !r.IsDeleted,
                new[] { "Product" }
            ).OrderByDescending(r => r.CreatedDate).ToList();

            return MapReviewsToDto(reviews);
        }

        /// <summary>
        /// Lấy tất cả đánh giá (cho Admin)
        /// </summary>
        public List<ReviewDto> GetAll()
        {
            var reviews = _reviewRepository.GetMulti(
                r => !r.IsDeleted,
                new[] { "Product", "User" }
            ).OrderByDescending(r => r.CreatedDate).ToList();

            return MapReviewsToDto(reviews);
        }

        /// <summary>
        /// Lấy đánh giá chờ duyệt (cho Admin)
        /// </summary>
        public List<ReviewDto> GetPendingReviews()
        {
            var reviews = _reviewRepository.GetMulti(
                r => !r.IsApproved && !r.IsDeleted,
                new[] { "Product", "User" }
            ).OrderByDescending(r => r.CreatedDate).ToList();

            return MapReviewsToDto(reviews);
        }

        /// <summary>
        /// Duyệt đánh giá
        /// </summary>
        public bool Approve(int id)
        {
            var review = _reviewRepository.GetSingleById(id);
            if (review == null)
            {
                throw new Exception("Đánh giá không tồn tại");
            }

            review.IsApproved = true;
            review.UpdatedDate = DateTime.Now;

            _reviewRepository.Update(review);
            _unitOfWork.Commit();

            return true;
        }

        /// <summary>
        /// Từ chối/Hủy duyệt đánh giá
        /// </summary>
        public bool Reject(int id)
        {
            var review = _reviewRepository.GetSingleById(id);
            if (review == null)
            {
                throw new Exception("Đánh giá không tồn tại");
            }

            review.IsApproved = false;
            review.UpdatedDate = DateTime.Now;

            _reviewRepository.Update(review);
            _unitOfWork.Commit();

            return true;
        }

        /// <summary>
        /// Kiểm tra người dùng đã mua sản phẩm chưa (để được phép đánh giá)
        /// Chỉ cần đơn hàng có status Completed/Delivered hoặc đã thanh toán và đã giao (Shipped trở lên)
        /// </summary>
        public bool HasUserPurchasedProduct(int userId, int productId)
        {
            // Lấy tất cả đơn hàng của user (không filter status để debug dễ hơn)
            var allOrders = _orderRepository.GetMulti(
                o => o.UserId == userId
            ).ToList();

            if (!allOrders.Any())
            {
                return false;
            }

            // Lọc các đơn hàng đã hoàn thành hoặc đã thanh toán và đã giao
            // Cho phép: Completed, Delivered, hoặc (Paid + Shipped/Processing trở lên)
            var validOrders = allOrders.Where(o =>
            {
                var status = (o.Status ?? "").Trim();
                var paymentStatus = (o.PaymentStatus ?? "").Trim();
                
                // Đơn hàng đã hoàn thành
                if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                    status.Equals("Delivered", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                
                // Đơn hàng đã thanh toán và đã giao (Shipped/Processing)
                if (paymentStatus.Equals("Paid", StringComparison.OrdinalIgnoreCase) &&
                    (status.Equals("Shipped", StringComparison.OrdinalIgnoreCase) ||
                     status.Equals("Processing", StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
                
                return false;
            }).ToList();

            if (!validOrders.Any())
            {
                return false;
            }

            // Kiểm tra xem có đơn hàng nào chứa sản phẩm này không
            var orderIds = validOrders.Select(o => o.Id).ToList();
            var hasPurchased = _orderDetailRepository.GetMulti(
                od => orderIds.Contains(od.OrderId) && od.ProductId == productId
            ).Any();

            return hasPurchased;
        }

        /// <summary>
        /// Kiểm tra người dùng đã đánh giá sản phẩm chưa
        /// </summary>
        public bool HasUserReviewedProduct(int userId, int productId)
        {
            return _reviewRepository.GetMulti(
                r => r.UserId == userId && r.ProductId == productId && !r.IsDeleted
            ).Any();
        }

        /// <summary>
        /// Lấy điểm đánh giá trung bình của sản phẩm
        /// </summary>
        public double GetAverageRating(int productId)
        {
            var reviews = _reviewRepository.GetMulti(
                r => r.ProductId == productId && r.IsApproved && !r.IsDeleted
            ).ToList();

            if (!reviews.Any())
            {
                return 0;
            }

            return Math.Round(reviews.Average(r => r.Rating), 1);
        }

        /// <summary>
        /// Lấy tổng số đánh giá của sản phẩm (đã duyệt)
        /// </summary>
        public int GetReviewCount(int productId)
        {
            return _reviewRepository.GetMulti(
                r => r.ProductId == productId && r.IsApproved && !r.IsDeleted
            ).Count();
        }

        /// <summary>
        /// Lấy phân bố số sao của sản phẩm (số lượng mỗi mức sao)
        /// </summary>
        public Dictionary<int, int> GetRatingDistribution(int productId)
        {
            var reviews = _reviewRepository.GetMulti(
                r => r.ProductId == productId && r.IsApproved
            ).ToList();

            var distribution = new Dictionary<int, int>
            {
                { 1, 0 },
                { 2, 0 },
                { 3, 0 },
                { 4, 0 },
                { 5, 0 }
            };

            foreach (var review in reviews)
            {
                if (distribution.ContainsKey(review.Rating))
                {
                    distribution[review.Rating]++;
                }
            }

            return distribution;
        }

        /// <summary>
        /// Lấy thống kê đánh giá của sản phẩm
        /// </summary>
        public ReviewStatisticsDto GetProductReviewStatistics(int productId)
        {
            var distribution = GetRatingDistribution(productId);
            var totalReviews = distribution.Values.Sum();
            var averageRating = GetAverageRating(productId);

            // Tính phần trăm
            var percentage = new Dictionary<int, double>();
            foreach (var kvp in distribution)
            {
                percentage[kvp.Key] = totalReviews > 0
                    ? Math.Round((double)kvp.Value / totalReviews * 100, 1)
                    : 0;
            }

            return new ReviewStatisticsDto
            {
                ProductId = productId,
                AverageRating = averageRating,
                TotalReviews = totalReviews,
                RatingDistribution = distribution,
                RatingPercentage = percentage
            };
        }

        /// <summary>
        /// Helper method để map danh sách Review sang ReviewDto
        /// </summary>
        private List<ReviewDto> MapReviewsToDto(List<Review> reviews)
        {
            var reviewDtos = new List<ReviewDto>();
            foreach (var review in reviews)
            {
                var dto = _mapper.Map<Review, ReviewDto>(review);
                if (review.Product != null)
                {
                    dto.ProductName = review.Product.Name;
                }
                if (review.User != null)
                {
                    dto.UserName = !string.IsNullOrEmpty(review.User.FullName) ? review.User.FullName : review.User.Email;
                }
                reviewDtos.Add(dto);
            }
            return reviewDtos;
        }
    }
}