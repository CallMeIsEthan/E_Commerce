using System;
using System.Web.Mvc;
using E_Commerce.Service;
using E_Commerce.Dto;

namespace E_Commerce.Web.Areas.User.Controllers
{
    public class ReviewController : Controller
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// Lấy danh sách review của sản phẩm (chỉ lấy đã duyệt)
        /// </summary>
        [HttpGet]
        public ActionResult GetByProductId(int productId)
        {
            try
            {
                var reviews = _reviewService.GetByProductId(productId);
                return Json(new { success = true, reviews = reviews }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Lấy thống kê review của sản phẩm
        /// </summary>
        [HttpGet]
        public ActionResult GetStatistics(int productId)
        {
            try
            {
                var statistics = _reviewService.GetProductReviewStatistics(productId);
                return Json(new { success = true, statistics = statistics }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Kiểm tra user đã đánh giá sản phẩm chưa
        /// </summary>
        [HttpGet]
        public ActionResult CheckUserReview(int productId, int userId)
        {
            try
            {
                var hasReviewed = _reviewService.HasUserReviewedProduct(userId, productId);
                var hasPurchased = _reviewService.HasUserPurchasedProduct(userId, productId);
                return Json(new { success = true, hasReviewed = hasReviewed, hasPurchased = hasPurchased }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Tạo đánh giá mới
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ReviewCreateDto reviewCreateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
                }

                var review = _reviewService.Create(reviewCreateDto);
                return Json(new { success = true, message = "Đánh giá của bạn đã được gửi và đang chờ duyệt. Cảm ơn bạn đã đánh giá!", review = review });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy ID của user hiện tại (helper method)
        /// </summary>
        private int? GetCurrentUserId()
        {
            return Session["UserId"] as int?;
        }
    }
}

