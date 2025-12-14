using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using E_Commerce.Service;
using E_Commerce.Web.Attributes;
using E_Commerce.Web.ViewModels;
using AutoMapper;

namespace E_Commerce.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class DiscountCodeController : Controller
    {
        private readonly IDiscountCodeService _discountCodeService;
        private readonly IMapper _mapper;

        public DiscountCodeController(
            IDiscountCodeService discountCodeService,
            IMapper mapper)
        {
            _discountCodeService = discountCodeService;
            _mapper = mapper;
        }

        // GET: Admin/DiscountCode
        public ActionResult Index(DiscountCodeFilterViewModel filter)
        {
            ViewBag.Title = "Quản lý mã giảm giá";

            // Nếu filter null, khởi tạo mặc định
            if (filter == null)
            {
                filter = new DiscountCodeFilterViewModel();
            }

            // Lấy danh sách discount codes đã lọc
            var discountCodes = _discountCodeService.SearchDiscountCodes(
                searchTerm: filter.SearchTerm,
                isActive: filter.IsActive,
                sortBy: filter.SortBy ?? "createdDate",
                sortOrder: filter.SortOrder ?? "desc"
            );

            // Truyền filter vào ViewBag để giữ lại giá trị trong form
            ViewBag.Filter = filter;

            return View(discountCodes);
        }

        // GET: Admin/DiscountCode/Create
        public ActionResult Create()
        {
            var viewModel = new DiscountCodeCreateViewModel
            {
                IsActive = true,
                DiscountType = "Percentage",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(1)
            };

            ViewBag.Title = "Tạo mã giảm giá mới";
            return View(viewModel);
        }

        // POST: Admin/DiscountCode/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DiscountCodeCreateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join("<br>", errors) });
            }

            try
            {
                // Map từ ViewModel sang DTO
                var discountCodeCreateDto = _mapper.Map<DiscountCodeCreateViewModel, E_Commerce.Dto.DiscountCodeCreateDto>(viewModel);

                // Tạo mã giảm giá
                var discountCodeDto = _discountCodeService.Create(discountCodeCreateDto);

                return Json(new { success = true, message = "Tạo mã giảm giá thành công", redirectUrl = Url.Action("Index", "DiscountCode") });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Admin/DiscountCode/Edit/5
        public ActionResult Edit(int id)
        {
            var discountCodeDto = _discountCodeService.GetById(id);
            if (discountCodeDto == null)
            {
                return HttpNotFound();
            }

            var viewModel = new DiscountCodeCreateViewModel
            {
                Code = discountCodeDto.Code,
                Name = discountCodeDto.Name,
                DiscountType = discountCodeDto.DiscountType,
                DiscountValue = discountCodeDto.DiscountValue,
                MinOrderAmount = discountCodeDto.MinOrderAmount,
                UsageLimit = discountCodeDto.UsageLimit,
                PerUserLimit = discountCodeDto.PerUserLimit,
                StartDate = discountCodeDto.StartDate,
                EndDate = discountCodeDto.EndDate,
                IsActive = discountCodeDto.IsActive
            };

            ViewBag.DiscountCodeId = id;
            ViewBag.UsedCount = discountCodeDto.UsedCount;
            ViewBag.Title = "Sửa mã giảm giá";
            return View(viewModel);
        }

        // POST: Admin/DiscountCode/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, DiscountCodeCreateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join("<br>", errors) });
            }

            try
            {
                var discountCodeDto = _discountCodeService.GetById(id);
                if (discountCodeDto == null)
                {
                    return Json(new { success = false, message = "Mã giảm giá không tồn tại" });
                }

                // Map từ ViewModel sang DTO
                var discountCodeUpdateDto = new E_Commerce.Dto.DiscountCodeUpdateDto
                {
                    Id = id,
                    Code = viewModel.Code,
                    Name = viewModel.Name,
                    DiscountType = viewModel.DiscountType,
                    DiscountValue = viewModel.DiscountValue,
                    MinOrderAmount = viewModel.MinOrderAmount,
                    UsageLimit = viewModel.UsageLimit,
                    PerUserLimit = viewModel.PerUserLimit,
                    StartDate = viewModel.StartDate,
                    EndDate = viewModel.EndDate,
                    IsActive = viewModel.IsActive
                };

                // Cập nhật mã giảm giá
                var updatedDiscountCode = _discountCodeService.Update(id, discountCodeUpdateDto);

                return Json(new { success = true, message = "Cập nhật mã giảm giá thành công", redirectUrl = Url.Action("Index", "DiscountCode") });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Admin/DiscountCode/Delete/5
        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                var result = _discountCodeService.Delete(id);
                return Json(new { success = true, message = "Xóa mã giảm giá thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

