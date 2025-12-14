using System;
using System.Web.Mvc;
using E_Commerce.Service;
using E_Commerce.Dto;
using E_Commerce.Web.Attributes;

namespace E_Commerce.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class ReviewController : Controller
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // GET: Admin/Review
        public ActionResult Index()
        {
            var reviews = _reviewService.GetAll();
            var pendingCount = _reviewService.GetPendingReviews().Count;
            ViewBag.PendingCount = pendingCount;
            return View(reviews);
        }

        // GET: Admin/Review/Pending
        public ActionResult Pending()
        {
            var pendingReviews = _reviewService.GetPendingReviews();
            ViewBag.PendingCount = pendingReviews.Count;
            return View(pendingReviews);
        }

        // POST: Admin/Review/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Approve(int id)
        {
            try
            {
                _reviewService.Approve(id);
                TempData["SuccessMessage"] = "Đã duyệt đánh giá thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }
            return RedirectToAction("Pending");
        }

        // POST: Admin/Review/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reject(int id)
        {
            try
            {
                _reviewService.Reject(id);
                TempData["SuccessMessage"] = "Đã từ chối đánh giá thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }
            return RedirectToAction("Pending");
        }

        // POST: Admin/Review/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                _reviewService.Delete(id);
                TempData["SuccessMessage"] = "Đã xóa đánh giá thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}

