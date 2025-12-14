using System;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using E_Commerce.Dto;
using E_Commerce.Service;
using E_Commerce.Web.ViewModels;

namespace E_Commerce.Web.Areas.User.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;

        public AccountController(IAccountService accountService, IMapper mapper)
        {
            _accountService = accountService;
            _mapper = mapper;
        }

        // GET: User/Account/Login
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            // Nếu đã đăng nhập, redirect về trang chủ
            if (Session["User"] != null)
            {
                return RedirectToAction("Index", "Home", new { area = "User" });
            }

            var viewModel = new LoginViewModel { ReturnUrl = returnUrl };
            ViewBag.ReturnUrl = returnUrl;
            return View(viewModel);
        }

        // POST: User/Account/Login (AJAX)
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

                // Kiểm tra tài khoản có active không
                if (!user.IsActive)
                {
                    return Json(new { success = false, message = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên." }, JsonRequestBehavior.AllowGet);
                }

                // Lưu thông tin user vào session
                Session["User"] = user;
                Session["UserId"] = user.Id;
                Session["UserEmail"] = user.Email;

                // Xác định redirect URL
                if (string.IsNullOrEmpty(returnUrl) || returnUrl.Contains("/Account/Register"))
                {
                    returnUrl = Url.Action("Index", "Home", new { area = "User" });
                }
                else
                {
                    if (!Url.IsLocalUrl(returnUrl))
                    {
                        returnUrl = Url.Action("Index", "Home", new { area = "User" });
                    }
                }

                return Json(new { success = true, message = "Đăng nhập thành công.", redirectUrl = returnUrl }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: User/Account/Register
        [HttpGet]
        public ActionResult Register()
        {
            // Nếu đã đăng nhập, redirect về trang chủ
            if (Session["User"] != null)
            {
                return RedirectToAction("Index", "Home", new { area = "User" });
            }

            return View(new UserCreateViewModel());
        }

        // POST: User/Account/Register (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Register(UserCreateViewModel viewModel, string returnUrl)
        {
            // Validate ModelState
            if (!ModelState.IsValid)
            {
                var allErrors = string.Join("<br>", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Where(e => !string.IsNullOrEmpty(e.ErrorMessage))
                    .Select(e => e.ErrorMessage));

                if (string.IsNullOrEmpty(allErrors))
                {
                    allErrors = "Vui lòng kiểm tra lại thông tin đã nhập.";
                }

                return Json(new { success = false, message = allErrors }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                // Map ViewModel -> DTO bằng AutoMapper
                var userCreateDto = _mapper.Map<UserCreateViewModel, UserCreateDto>(viewModel);
                userCreateDto.RoleIds = null; // Mặc định sẽ gán role Customer

                var user = _accountService.Register(userCreateDto);

                if (user == null)
                {
                    return Json(new { success = false, message = "Đăng ký thất bại. Vui lòng thử lại." }, JsonRequestBehavior.AllowGet);
                }

                // Sau khi đăng ký thành công, redirect về trang Login để user đăng nhập
                var loginUrl = Url.Action("Login", "Account", new { area = "User" });
                if (!string.IsNullOrEmpty(returnUrl) && !returnUrl.Contains("/Account/"))
                {
                    loginUrl += "?returnUrl=" + Uri.EscapeDataString(returnUrl);
                }

                return Json(new { success = true, message = "Đăng ký thành công! Vui lòng đăng nhập để tiếp tục.", redirectUrl = loginUrl }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log lỗi để debug
                System.Diagnostics.Debug.WriteLine("Register Error: " + ex.ToString());
                
                // Trả về message lỗi chi tiết hơn
                var errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += " " + ex.InnerException.Message;
                }
                
                return Json(new { success = false, message = errorMessage }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: User/Account/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "Home", new { area = "User" });
        }

        // GET: User/Account/Profile
        [HttpGet]
        public ActionResult Profile()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", new { returnUrl = Url.Action("Profile", "Account", new { area = "User" }) });
            }

            var user = _accountService.GetUserById(userId.Value);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài khoản.";
                return RedirectToAction("Login");
            }

            var vm = new UserProfileViewModel
            {
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                Address = user.Address
            };

            return View(vm);
        }

        // POST: User/Account/Profile (update info)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Profile(UserProfileViewModel model)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." }, JsonRequestBehavior.AllowGet);
            }

            if (!ModelState.IsValid)
            {
                var errors = string.Join("<br/>", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Json(new { success = false, message = errors }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var dto = new UserCreateDto
                {
                    Email = model.Email, // giữ email hiện tại
                    FullName = model.FullName,
                    Phone = model.Phone,
                    Address = model.Address,
                    // Không đổi mật khẩu ở đây
                };

                var updated = _accountService.UpdateUser(userId.Value, dto);

                // Cập nhật session
                Session["User"] = updated;
                Session["UserId"] = updated.Id;
                Session["UserEmail"] = updated.Email;
                Session["Name"] = updated.FullName;
                Session["Email"] = updated.Email;
                Session["Phone"] = updated.Phone;

                return Json(new { success = true, message = "Cập nhật thông tin thành công." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: User/Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." }, JsonRequestBehavior.AllowGet);
            }

            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ mật khẩu cũ, mới và xác nhận." }, JsonRequestBehavior.AllowGet);
            }

            if (newPassword.Length < 6)
            {
                return Json(new { success = false, message = "Mật khẩu mới phải tối thiểu 6 ký tự." }, JsonRequestBehavior.AllowGet);
            }

            if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
            {
                return Json(new { success = false, message = "Mật khẩu xác nhận không khớp." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var ok = _accountService.ChangePassword(userId.Value, oldPassword, newPassword);
                if (ok)
                {
                    return Json(new { success = true, message = "Đổi mật khẩu thành công." }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { success = false, message = "Đổi mật khẩu thất bại." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private int? GetCurrentUserId()
        {
            return Session["UserId"] as int?;
        }
    }
}

