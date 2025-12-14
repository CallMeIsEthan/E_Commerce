using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using E_Commerce.Dto;
using E_Commerce.Service;
using E_Commerce.Web.Attributes;
using E_Commerce.Web.ViewModels;

namespace E_Commerce.Web.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        // GET: Admin/Account/Login
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            // Nếu đã đăng nhập, redirect về dashboard
            if (Session["AdminUser"] != null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var viewModel = new LoginViewModel { ReturnUrl = returnUrl };
            ViewBag.ReturnUrl = returnUrl;
            return View(viewModel);
        }

        // POST: Admin/Account/Login (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Login(LoginViewModel viewModel, string returnUrl)
        {
            // Validate ModelState
            if (!ModelState.IsValid)
            {
                var allErrors = string.Join("<br>", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                return Json(new { success = false, message = allErrors }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                // Map ViewModel -> DTO
                var loginDto = new UserLoginDto
                {
                    Username = viewModel.Username,
                    Password = viewModel.Password
                };

                var user = _accountService.Login(loginDto);

                // Kiểm tra user có role Admin không (roles được load từ bảng UserRole)
                if (user.Roles == null || user.Roles.Count == 0 || !user.Roles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase)))
                {
                    return Json(new { success = false, message = "Bạn không có quyền truy cập vào khu vực quản trị" }, JsonRequestBehavior.AllowGet);
                }

                // Lưu thông tin user vào session
                Session["AdminUser"] = user;
                Session["AdminUserId"] = user.Id;
                Session["AdminFullName"] = user.FullName;
                Session["AdminUsername"] = user.FullName ?? user.Email;
                Session["AdminUserEmail"] = user.Email;

                // Xác định redirect URL
                if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                {
                    returnUrl = Url.Action("Index", "Dashboard", new { area = "Admin" });
                }

                return Json(new { success = true, message = "Đăng nhập thành công!", redirectUrl = returnUrl }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Admin/Account/ChangePassword
        [AuthorizeAdmin]
        public ActionResult ChangePassword()
        {
            ViewBag.Title = "Đổi mật khẩu";
            return View();
        }

        // POST: Admin/Account/ChangePassword
        [HttpPost]
        [AuthorizeAdmin]
        [ValidateAntiForgeryToken]
        public JsonResult ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            try
            {
                var userId = Session["AdminUserId"] as int?;
                if (!userId.HasValue)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập lại" }, JsonRequestBehavior.AllowGet);
                }

                // Validate
                if (string.IsNullOrWhiteSpace(oldPassword))
                {
                    return Json(new { success = false, message = "Vui lòng nhập mật khẩu cũ" }, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrWhiteSpace(newPassword))
                {
                    return Json(new { success = false, message = "Vui lòng nhập mật khẩu mới" }, JsonRequestBehavior.AllowGet);
                }

                if (newPassword.Length < 6)
                {
                    return Json(new { success = false, message = "Mật khẩu mới phải có ít nhất 6 ký tự" }, JsonRequestBehavior.AllowGet);
                }

                if (newPassword != confirmPassword)
                {
                    return Json(new { success = false, message = "Mật khẩu mới và xác nhận mật khẩu không khớp" }, JsonRequestBehavior.AllowGet);
                }

                // Đổi mật khẩu
                var result = _accountService.ChangePassword(userId.Value, oldPassword, newPassword);

                if (result)
                {
                    return Json(new { success = true, message = "Đổi mật khẩu thành công!" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Đổi mật khẩu thất bại" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Admin/Account/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login");
        }
    }
}