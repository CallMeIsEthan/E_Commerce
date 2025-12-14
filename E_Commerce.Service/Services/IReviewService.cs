using E_Commerce.Dto;
using System.Collections.Generic;

namespace E_Commerce.Service
{
    public interface IReviewService
    {
        /// <summary>
        /// Tạo đánh giá mới
        /// </summary>
        ReviewDto Create(ReviewCreateDto reviewCreateDto);

        /// <summary>
        /// Cập nhật đánh giá
        /// </summary>
        ReviewDto Update(int id, ReviewCreateDto reviewUpdateDto);

        /// <summary>
        /// Xóa đánh giá
        /// </summary>
        bool Delete(int id);

        /// <summary>
        /// Lấy đánh giá theo ID
        /// </summary>
        ReviewDto GetById(int id);

        /// <summary>
        /// Lấy tất cả đánh giá của một sản phẩm (chỉ lấy đã duyệt)
        /// </summary>
        List<ReviewDto> GetByProductId(int productId);

        /// <summary>
        /// Lấy tất cả đánh giá của một sản phẩm (bao gồm cả chưa duyệt - cho Admin)
        /// </summary>
        List<ReviewDto> GetAllByProductId(int productId);

        /// <summary>
        /// Lấy tất cả đánh giá của một người dùng
        /// </summary>
        List<ReviewDto> GetByUserId(int userId);

        /// <summary>
        /// Lấy tất cả đánh giá (cho Admin)
        /// </summary>
        List<ReviewDto> GetAll();

        /// <summary>
        /// Lấy đánh giá chờ duyệt (cho Admin)
        /// </summary>
        List<ReviewDto> GetPendingReviews();

        /// <summary>
        /// Duyệt đánh giá
        /// </summary>
        bool Approve(int id);

        /// <summary>
        /// Từ chối/Hủy duyệt đánh giá
        /// </summary>
        bool Reject(int id);

        /// <summary>
        /// Kiểm tra người dùng đã mua sản phẩm chưa (để được phép đánh giá)
        /// </summary>
        bool HasUserPurchasedProduct(int userId, int productId);

        /// <summary>
        /// Kiểm tra người dùng đã đánh giá sản phẩm chưa
        /// </summary>
        bool HasUserReviewedProduct(int userId, int productId);

        /// <summary>
        /// Lấy điểm đánh giá trung bình của sản phẩm
        /// </summary>
        double GetAverageRating(int productId);

        /// <summary>
        /// Lấy tổng số đánh giá của sản phẩm (đã duyệt)
        /// </summary>
        int GetReviewCount(int productId);

        /// <summary>
        /// Lấy phân bố số sao của sản phẩm (số lượng mỗi mức sao)
        /// </summary>
        Dictionary<int, int> GetRatingDistribution(int productId);

        /// <summary>
        /// Lấy thống kê đánh giá của sản phẩm
        /// </summary>
        ReviewStatisticsDto GetProductReviewStatistics(int productId);
    }

    /// <summary>
    /// DTO cho thống kê đánh giá sản phẩm
    /// </summary>
    public class ReviewStatisticsDto
    {
        public int ProductId { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } // Key: số sao (1-5), Value: số lượng
        public Dictionary<int, double> RatingPercentage { get; set; } // Key: số sao (1-5), Value: phần trăm
    }
}

