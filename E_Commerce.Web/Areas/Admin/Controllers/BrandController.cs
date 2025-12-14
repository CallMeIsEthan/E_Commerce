using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using E_Commerce.Common.Helpers;
using E_Commerce.Service;
using E_Commerce.Web.Attributes;
using E_Commerce.Web.ViewModels;
using AutoMapper;

namespace E_Commerce.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class BrandController : Controller
    {
        private readonly IBrandService _brandService;
        private readonly IMapper _mapper;

        public BrandController(
            IBrandService brandService,
            IMapper mapper)
        {
            _brandService = brandService;
            _mapper = mapper;
        }

        // GET: Admin/Brand
        public ActionResult Index(BrandFilterViewModel filter)
        {
            // TODO: Bật lại khi cần check phân quyền
            //if (Session["AdminUser"] == null)
            //{
            //    return RedirectToAction("Login", "Account", new { area = "Admin" });
            //}

            ViewBag.Title = "Quản lý thương hiệu";

            // Nếu filter null, khởi tạo mặc định
            if (filter == null)
            {
                filter = new BrandFilterViewModel();
            }

            // Lấy danh sách brands đã lọc
            var brands = _brandService.SearchBrands(
                searchTerm: filter.SearchTerm,
                isActive: filter.IsActive,
                sortBy: filter.SortBy ?? "name",
                sortOrder: filter.SortOrder ?? "asc"
            );

            // Truyền filter vào ViewBag để giữ lại giá trị trong form
            ViewBag.Filter = filter;

            return View(brands);
        }

        // GET: Admin/Brand/Create
        public ActionResult Create()
        {
            // TODO: Bật lại khi cần check phân quyền
            //if (Session["AdminUser"] == null)
            //{
            //    return RedirectToAction("Login", "Account", new { area = "Admin" });
            //}

            var viewModel = new BrandCreateViewModel
            {
                IsActive = true,
                DisplayOrder = 0
            };

            ViewBag.Title = "Tạo thương hiệu mới";
            return View(viewModel);
        }

        // POST: Admin/Brand/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(BrandCreateViewModel viewModel)
        {
            // TODO: Bật lại khi cần check phân quyền
            //if (Session["AdminUser"] == null)
            //{
            //    return Json(new { success = false, message = "Vui lòng đăng nhập" });
            //}

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join("<br>", errors) });
            }

            try
            {
                // Upload logo lên Cloudinary nếu có
                string logoUrl = null;
                if (Request.Files.Count > 0 && Request.Files["Logo"] != null && Request.Files["Logo"].ContentLength > 0)
                {
                    try
                    {
                        var file = Request.Files["Logo"];
                        logoUrl = await CloudinaryHelper.UploadImageAsync(file, "brands");
                    }
                    catch (Exception uploadEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Lỗi upload logo: {uploadEx.Message}");
                        System.Diagnostics.Debug.WriteLine($"StackTrace: {uploadEx.StackTrace}");
                        return Json(new { success = false, message = $"Lỗi upload logo: {uploadEx.Message}" });
                    }
                }

                // Map từ ViewModel sang DTO
                var brandCreateDto = _mapper.Map<BrandCreateViewModel, E_Commerce.Dto.BrandCreateDto>(viewModel);

                // Set logo URL nếu đã upload
                if (!string.IsNullOrEmpty(logoUrl))
                {
                    brandCreateDto.LogoUrl = logoUrl;
                }

                // Tạo thương hiệu
                var brandDto = _brandService.Create(brandCreateDto);

                return Json(new { success = true, message = "Tạo thương hiệu thành công", redirectUrl = Url.Action("Index", "Brand") });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating brand: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                var errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += $" ({ex.InnerException.Message})";
                }

                return Json(new { success = false, message = errorMessage });
            }
        }

        // GET: Admin/Brand/Edit/5
        public ActionResult Edit(int id)
        {
            // TODO: Bật lại khi cần check phân quyền
            //if (Session["AdminUser"] == null)
            //{
            //    return RedirectToAction("Login", "Account", new { area = "Admin" });
            //}

            var brandDto = _brandService.GetById(id);
            if (brandDto == null)
            {
                return HttpNotFound();
            }

            var viewModel = new BrandCreateViewModel
            {
                Name = brandDto.Name,
                Description = brandDto.Description,
                IsActive = brandDto.IsActive,
                DisplayOrder = brandDto.DisplayOrder
            };

            ViewBag.LogoUrl = brandDto.LogoUrl;
            ViewBag.BrandId = id;
            ViewBag.Title = "Sửa thương hiệu";
            return View(viewModel);
        }

        // POST: Admin/Brand/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, BrandCreateViewModel viewModel)
        {
            // TODO: Bật lại khi cần check phân quyền
            //if (Session["AdminUser"] == null)
            //{
            //    return Json(new { success = false, message = "Vui lòng đăng nhập" });
            //}

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join("<br>", errors) });
            }

            try
            {
                var brandDto = _brandService.GetById(id);
                if (brandDto == null)
                {
                    return Json(new { success = false, message = "Thương hiệu không tồn tại" });
                }

                // Upload logo mới nếu có
                string logoUrl = brandDto.LogoUrl; // Giữ logo cũ
                if (Request.Files.Count > 0 && Request.Files["Logo"] != null && Request.Files["Logo"].ContentLength > 0)
                {
                    var file = Request.Files["Logo"];
                    logoUrl = await CloudinaryHelper.UploadImageAsync(file, "brands");
                }

                // Map từ ViewModel sang DTO
                var brandUpdateDto = new E_Commerce.Dto.BrandUpdateDto
                {
                    Id = id,
                    Name = viewModel.Name,
                    Description = viewModel.Description,
                    LogoUrl = logoUrl,
                    IsActive = viewModel.IsActive,
                    DisplayOrder = viewModel.DisplayOrder
                };

                // Cập nhật thương hiệu
                var updatedBrand = _brandService.Update(id, brandUpdateDto);

                return Json(new { success = true, message = "Cập nhật thương hiệu thành công", redirectUrl = Url.Action("Index", "Brand") });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Admin/Brand/Delete/5
        [HttpPost]
        public ActionResult Delete(int id)
        {
            // TODO: Bật lại khi cần check phân quyền
            //if (Session["AdminUser"] == null)
            //{
            //    return Json(new { success = false, message = "Vui lòng đăng nhập" });
            //}

            try
            {
                var result = _brandService.Delete(id);
                return Json(new { success = true, message = "Xóa thương hiệu thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

