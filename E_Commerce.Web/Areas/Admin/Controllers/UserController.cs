using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using E_Commerce.Data.Repositories;
using E_Commerce.Data.Infrastructure;
using AutoMapper;
using E_Commerce.Dto;
using UserModel = E_Commerce.Model.Models.User;
using OrderModel = E_Commerce.Model.Models.Order;
using E_Commerce.Web.Attributes;

namespace E_Commerce.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserController(
            IUserRepository userRepository,
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // GET: Admin/User
        public ActionResult Index(string searchTerm = null, string statusFilter = "all", int page = 1, int pageSize = 20)
        {
            ViewBag.Title = "Quản lý người dùng";

            var query = _userRepository.GetAll().AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(u =>
                    (u.Email != null && u.Email.Contains(searchTerm)) ||
                    (u.FullName != null && u.FullName.Contains(searchTerm)) ||
                    (u.Phone != null && u.Phone.Contains(searchTerm)));
            }

            // Lọc theo trạng thái
            if (statusFilter == "active")
            {
                query = query.Where(u => u.IsActive);
            }
            else if (statusFilter == "inactive")
            {
                query = query.Where(u => !u.IsActive);
            }

            // Sắp xếp
            query = query.OrderByDescending(u => u.CreatedDate);

            // Phân trang
            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var users = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Map sang DTO
            var userDtos = users.Select(u =>
            {
                var dto = _mapper.Map<UserModel, UserDto>(u);
                // Lấy số đơn hàng của user
                var orderCount = _orderRepository.GetMulti(o => o.UserId == u.Id && o.Status != "Cancelled").Count();
                ViewData[$"OrderCount_{u.Id}"] = orderCount;
                return dto;
            }).ToList();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;

            return View(userDtos);
        }

        // GET: Admin/User/Details/5
        public ActionResult Details(int id)
        {
            ViewBag.Title = "Chi tiết người dùng";

            var user = _userRepository.GetSingleById(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var userDto = _mapper.Map<UserModel, UserDto>(user);

            // Lấy đơn hàng của user
            var orders = _orderRepository.GetMulti(o => o.UserId == id)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
            var orderDtos = orders.Select(o => _mapper.Map<OrderModel, OrderDto>(o)).ToList();

            ViewBag.Orders = orderDtos;
            ViewBag.OrderCount = orderDtos.Count;
            ViewBag.TotalSpent = orderDtos.Where(o => o.PaymentStatus == "Paid" && o.Status != "Cancelled")
                .Sum(o => o.TotalAmount);

            return View(userDto);
        }

        // POST: Admin/User/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ToggleActive(int id)
        {
            try
            {
                var user = _userRepository.GetSingleById(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "Người dùng không tồn tại!" }, JsonRequestBehavior.AllowGet);
                }

                user.IsActive = !user.IsActive;
                user.UpdatedDate = DateTime.Now;
                _userRepository.Update(user);
                _unitOfWork.Commit();

                return Json(new
                {
                    success = true,
                    message = user.IsActive ? "Đã kích hoạt tài khoản!" : "Đã vô hiệu hóa tài khoản!",
                    isActive = user.IsActive
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Admin/User/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Delete(int id)
        {
            try
            {
                var user = _userRepository.GetSingleById(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "Người dùng không tồn tại!" }, JsonRequestBehavior.AllowGet);
                }

                // Kiểm tra xem user có đơn hàng không
                var hasOrders = _orderRepository.GetMulti(o => o.UserId == id).Any();
                if (hasOrders)
                {
                    // Không xóa, chỉ vô hiệu hóa
                    user.IsActive = false;
                    user.UpdatedDate = DateTime.Now;
                    _userRepository.Update(user);
                    _unitOfWork.Commit();
                    return Json(new { success = true, message = "Người dùng có đơn hàng, đã vô hiệu hóa tài khoản thay vì xóa!" }, JsonRequestBehavior.AllowGet);
                }

                _userRepository.Delete(user);
                _unitOfWork.Commit();

                return Json(new { success = true, message = "Đã xóa người dùng thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}