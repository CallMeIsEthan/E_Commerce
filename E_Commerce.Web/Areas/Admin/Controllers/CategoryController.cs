using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using E_Commerce.Common.Helpers;
using E_Commerce.Data.Repositories;
using E_Commerce.Service;
using E_Commerce.Web.Attributes;
using E_Commerce.Web.ViewModels;
using AutoMapper;

namespace E_Commerce.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryController(
            ICategoryService categoryService,
            ICategoryRepository categoryRepository,
            IMapper mapper)
        {
            _categoryService = categoryService;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        // GET: Admin/Category
        public ActionResult Index(CategoryFilterViewModel filter)
        {
            // TODO: Bật lại khi cần check phân quyền
            //if (Session["AdminUser"] == null)
            //{
            //    return RedirectToAction("Login", "Account", new { area = "Admin" });
            //}

            ViewBag.Title = "Quản lý danh mục";

            // Nếu filter null, khởi tạo mặc định
            if (filter == null)
            {
                filter = new CategoryFilterViewModel();
            }

            // Lấy danh sách categories đã lọc
            var categories = _categoryService.SearchCategories(
                searchTerm: filter.SearchTerm,
                isActive: filter.IsActive,
                level: filter.Level,
                sortBy: filter.SortBy ?? "name",
                sortOrder: filter.SortOrder ?? "asc"
            );

            // Truyền filter vào ViewBag để giữ lại giá trị trong form
            ViewBag.Filter = filter;

            return View(categories);
        }

        // GET: Admin/Category/Create
        public ActionResult Create()
        {
            // TODO: Bật lại khi cần check phân quyền
            //if (Session["AdminUser"] == null)
            //{
            //    return RedirectToAction("Login", "Account", new { area = "Admin" });
            //}

            var viewModel = new CategoryCreateViewModel
            {
                IsActive = true,
                DisplayOrder = 0,
                // Chỉ hiển thị danh mục bậc 1 (không có parent) vì chỉ hỗ trợ 2 bậc
                ParentCategories = new SelectList(_categoryRepository.GetAll().Where(c => c.IsActive && c.ParentCategoryId == null).OrderBy(c => c.Name), "Id", "Name", null)
            };

            ViewBag.Title = "Tạo danh mục mới";
            return View(viewModel);
        }

        // POST: Admin/Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CategoryCreateViewModel viewModel)
        {
            // TODO: Bật lại khi cần check phân quyền
            //if (Session["AdminUser"] == null)
            //{
            //    return Json(new { success = false, message = "Vui lòng đăng nhập" });
            //}

            if (!ModelState.IsValid)
            {
                // Lấy lại dropdown lists - chỉ hiển thị danh mục bậc 1
                viewModel.ParentCategories = new SelectList(_categoryRepository.GetAll().Where(c => c.IsActive && c.ParentCategoryId == null).OrderBy(c => c.Name), "Id", "Name", viewModel.ParentId);

                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join("<br>", errors) });
            }

            try
            {
                // Upload ảnh lên Cloudinary nếu có
                string imageUrl = null;
                if (Request.Files.Count > 0 && Request.Files["Image"] != null && Request.Files["Image"].ContentLength > 0)
                {
                    try
                    {
                        var file = Request.Files["Image"];
                        imageUrl = await CloudinaryHelper.UploadImageAsync(file, "categories"); // Upload vào folder "categories", Cloudinary tự generate public_id
                    }
                    catch (Exception uploadEx)
                    {
                        // Log lỗi upload nhưng vẫn tiếp tục tạo category (không bắt buộc phải có ảnh)
                        System.Diagnostics.Debug.WriteLine($"Lỗi upload ảnh: {uploadEx.Message}");
                        System.Diagnostics.Debug.WriteLine($"StackTrace: {uploadEx.StackTrace}");
                        // Có thể return lỗi hoặc tiếp tục không có ảnh
                        return Json(new { success = false, message = $"Lỗi upload ảnh: {uploadEx.Message}" });
                    }
                }

                // Map từ ViewModel sang DTO
                var categoryCreateDto = _mapper.Map<CategoryCreateViewModel, E_Commerce.Dto.CategoryCreateDto>(viewModel);

                // Set ảnh URL và DisplayOrder nếu đã upload
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    categoryCreateDto.ImageUrl = imageUrl;
                }

                // Generate alias nếu chưa có
                if (string.IsNullOrWhiteSpace(categoryCreateDto.Alias))
                {
                    categoryCreateDto.Alias = E_Commerce.Common.Helpers.AliasHelper.GenerateAlias(viewModel.Name);
                }

                // Set DisplayOrder
                categoryCreateDto.DisplayOrder = viewModel.DisplayOrder;

                // Tạo danh mục
                var categoryDto = _categoryService.Create(categoryCreateDto);

                return Json(new { success = true, message = "Tạo danh mục thành công", redirectUrl = Url.Action("Index", "Category") });
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết
                System.Diagnostics.Debug.WriteLine($"Error creating category: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    System.Diagnostics.Debug.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
                }
                
                // Trả về thông báo lỗi chi tiết hơn
                var errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += $" ({ex.InnerException.Message})";
                }
                
                return Json(new { success = false, message = errorMessage });
            }
        }

        // GET: Admin/Category/Edit/5
        public ActionResult Edit(int id)
        {
            // TODO: Bật lại khi cần check phân quyền
            //if (Session["AdminUser"] == null)
            //{
            //    return RedirectToAction("Login", "Account", new { area = "Admin" });
            //}

            var categoryDto = _categoryService.GetById(id);
            if (categoryDto == null)
            {
                return HttpNotFound();
            }

            var viewModel = new CategoryCreateViewModel
            {
                Name = categoryDto.Name,
                Description = categoryDto.Description,
                ParentId = categoryDto.ParentCategoryId,
                IsActive = categoryDto.IsActive,
                HomeFlag = categoryDto.HomeFlag,
                DisplayOrder = categoryDto.DisplayOrder,
                // Chỉ hiển thị danh mục bậc 1 (không có parent) vì chỉ hỗ trợ 2 bậc
                ParentCategories = new SelectList(_categoryRepository.GetAll().Where(c => c.IsActive && c.Id != id && c.ParentCategoryId == null).OrderBy(c => c.Name), "Id", "Name", categoryDto.ParentCategoryId)
            };

            ViewBag.ImageUrl = categoryDto.ImageUrl;
            ViewBag.CategoryId = id;
            ViewBag.Title = "Sửa danh mục";
            return View(viewModel);
        }

        // POST: Admin/Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, CategoryCreateViewModel viewModel)
        {
            // TODO: Bật lại khi cần check phân quyền
            //if (Session["AdminUser"] == null)
            //{
            //    return Json(new { success = false, message = "Vui lòng đăng nhập" });
            //}

            if (!ModelState.IsValid)
            {
                // Chỉ hiển thị danh mục bậc 1 (không có parent) vì chỉ hỗ trợ 2 bậc
                viewModel.ParentCategories = new SelectList(_categoryRepository.GetAll().Where(c => c.IsActive && c.Id != id && c.ParentCategoryId == null).OrderBy(c => c.Name), "Id", "Name", viewModel.ParentId);
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join("<br>", errors) });
            }

            try
            {
                var categoryDto = _categoryService.GetById(id);
                if (categoryDto == null)
                {
                    return Json(new { success = false, message = "Danh mục không tồn tại" });
                }

                // Upload ảnh mới nếu có
                string imageUrl = categoryDto.ImageUrl; // Giữ ảnh cũ
                if (Request.Files.Count > 0 && Request.Files["Image"] != null && Request.Files["Image"].ContentLength > 0)
                {
                    var file = Request.Files["Image"];
                    imageUrl = await CloudinaryHelper.UploadImageAsync(file, null); // Upload ở root, không vào folder
                }

                // Map từ ViewModel sang DTO
                var categoryUpdateDto = new E_Commerce.Dto.CategoryUpdateDto
                {
                    Id = id,
                    Name = viewModel.Name,
                    Description = viewModel.Description,
                    ParentCategoryId = viewModel.ParentId,
                    ImageUrl = imageUrl,
                    IsActive = viewModel.IsActive,
                    HomeFlag = viewModel.HomeFlag,
                    DisplayOrder = viewModel.DisplayOrder
                };

                // Cập nhật danh mục
                var updatedCategory = _categoryService.Update(id, categoryUpdateDto);

                return Json(new { success = true, message = "Cập nhật danh mục thành công", redirectUrl = Url.Action("Index", "Category") });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Admin/Category/Delete/5
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
                var result = _categoryService.Delete(id);
                return Json(new { success = true, message = "Xóa danh mục thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}