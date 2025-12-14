using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using E_Commerce.Common.Helpers;
using E_Commerce.Data.Repositories;
using E_Commerce.Service;
using E_Commerce.Web.Attributes;
using E_Commerce.Web.ViewModels;
using AutoMapper;
using E_Commerce.Dto;

namespace E_Commerce.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly IMapper _mapper;

        public ProductController(
            IProductService productService,
            ICategoryRepository categoryRepository,
            IBrandRepository brandRepository,
            IMapper mapper)
        {
            _productService = productService;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _mapper = mapper;
        }

        // GET: Admin/Product/Create
        public ActionResult Create()
        {
            // TODO: Bật lại khi cần check phân quyền
            //if (Session["AdminUser"] == null)
            //{
            //    return RedirectToAction("Login", "Account", new { area = "Admin" });
            //}

            var viewModel = new ProductCreateViewModel
            {
                IsActive = true,
                // Chỉ hiển thị danh mục bậc 2 (có parent) vì sản phẩm nên gán vào danh mục cụ thể
                Categories = new SelectList(_categoryRepository.GetAll().Where(c => c.IsActive && c.ParentCategoryId != null), "Id", "Name"),
                Brands = new SelectList(_brandRepository.GetAll().Where(b => b.IsActive), "Id", "Name"),
                Genders = new SelectList(new[]
                {
                    new { Value = "Nam", Text = "Nam" },
                    new { Value = "Nữ", Text = "Nữ" },
                    new { Value = "Unisex", Text = "Unisex" }
                }, "Value", "Text"),
                Seasons = new SelectList(new[]
                {
                    new { Value = "Xuân", Text = "Xuân" },
                    new { Value = "Hè", Text = "Hè" },
                    new { Value = "Thu", Text = "Thu" },
                    new { Value = "Đông", Text = "Đông" },
                    new { Value = "Quanh năm", Text = "Quanh năm" }
                }, "Value", "Text")
            };

            ViewBag.Title = "Tạo sản phẩm mới";
            return View(viewModel);
        }

        // POST: Admin/Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ProductCreateViewModel viewModel)
        {
            // Kiểm tra đăng nhập
            //if (Session["AdminUser"] == null)
            //{
            //    return Json(new { success = false, message = "Vui lòng đăng nhập" });
            //}

            if (!ModelState.IsValid)
            {
                // Lấy lại dropdown lists
                // Chỉ hiển thị danh mục bậc 2 (có parent) vì sản phẩm nên gán vào danh mục cụ thể
                viewModel.Categories = new SelectList(_categoryRepository.GetAll().Where(c => c.IsActive && c.ParentCategoryId != null), "Id", "Name", viewModel.CategoryId);
                viewModel.Brands = new SelectList(_brandRepository.GetAll().Where(b => b.IsActive), "Id", "Name", viewModel.BrandId);
                viewModel.Genders = new SelectList(new[]
                {
                    new { Value = "Nam", Text = "Nam" },
                    new { Value = "Nữ", Text = "Nữ" },
                    new { Value = "Unisex", Text = "Unisex" }
                }, "Value", "Text", viewModel.Gender);
                viewModel.Seasons = new SelectList(new[]
                {
                    new { Value = "Xuân", Text = "Xuân" },
                    new { Value = "Hè", Text = "Hè" },
                    new { Value = "Thu", Text = "Thu" },
                    new { Value = "Đông", Text = "Đông" },
                    new { Value = "Quanh năm", Text = "Quanh năm" }
                }, "Value", "Text", viewModel.Season);

                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = string.Join("<br>", errors) });
                }

                TempData["ErrorMessage"] = string.Join("<br>", errors);
                return View(viewModel);
            }

            try
            {
                // Map từ ViewModel sang DTO
                var productCreateDto = _mapper.Map<ProductCreateViewModel, E_Commerce.Dto.ProductCreateDto>(viewModel);

                // Generate alias nếu chưa có
                if (string.IsNullOrWhiteSpace(productCreateDto.Alias))
                {
                    productCreateDto.Alias = E_Commerce.Common.Helpers.AliasHelper.GenerateAlias(viewModel.Name);
                }

                // Tạo sản phẩm trước
                var productDto = _productService.Create(productCreateDto);

                // Upload ảnh sản phẩm (ảnh đầu tiên sẽ là ảnh chính với IsMain = true)
                var productImageUrls = new List<string>();
                var uploadedFileNames = new HashSet<string>(); // Để tránh upload trùng lặp

                // Lấy tất cả các file có name là "ProductImages"
                // Khi input có multiple, cần lặp qua tất cả files và kiểm tra name
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];
                    // Kiểm tra name của input (không phải FileName - tên file thực tế)
                    if (file != null && file.ContentLength > 0 && !string.IsNullOrEmpty(file.FileName))
                    {
                        // Kiểm tra xem file này có phải từ input "ProductImages" không
                        string inputName = Request.Files.AllKeys[i];
                        if (inputName == "ProductImages" || (inputName != null && inputName.StartsWith("ProductImages")))
                        {
                            // Kiểm tra trùng lặp dựa trên tên file và kích thước
                            string fileKey = $"{file.FileName}_{file.ContentLength}";
                            if (!uploadedFileNames.Contains(fileKey))
                            {
                                uploadedFileNames.Add(fileKey);
                                try
                                {
                                    System.Diagnostics.Debug.WriteLine($"[Upload] File {i}: {file.FileName}, Size: {file.ContentLength}, Key: {inputName}");
                                    var imageUrl = await CloudinaryHelper.UploadImageAsync(file, "products");
                                    if (!string.IsNullOrEmpty(imageUrl))
                                    {
                                        productImageUrls.Add(imageUrl);
                                        System.Diagnostics.Debug.WriteLine($"[Upload] Success: {imageUrl}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Log lỗi nhưng không dừng quá trình tạo sản phẩm
                                    System.Diagnostics.Debug.WriteLine($"Lỗi upload ảnh {i}: {ex.Message}");
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"[Upload] Skip duplicate: {file.FileName}");
                            }
                        }
                    }
                }

                // Thêm ảnh vào database: ảnh đầu tiên là ảnh chính (IsMain = true), các ảnh sau là ảnh phụ (IsMain = false)
                if (productImageUrls.Any())
                {
                    // Ảnh đầu tiên là ảnh chính
                    _productService.AddProductMainImage(productDto.Id, productImageUrls[0]);

                    // Các ảnh còn lại là ảnh phụ
                    if (productImageUrls.Count > 1)
                    {
                        var additionalImageUrls = productImageUrls.Skip(1).ToList();
                        _productService.AddProductImages(productDto.Id, additionalImageUrls);
                    }
                }

                // Tạo các biến thể sản phẩm (màu, size, giá, stock) nếu có
                if (viewModel.Variants != null && viewModel.Variants.Any())
                {
                    var variantCreateDtos = viewModel.Variants
                        .Where(v => v.Price > 0 || v.Stock > 0 || !string.IsNullOrEmpty(v.ColorName) || !string.IsNullOrEmpty(v.Size))
                        .Select(v => _mapper.Map<E_Commerce.Web.ViewModels.ProductVariantViewModel, ProductVariantCreateDto>(v))
                        .ToList();

                    if (variantCreateDtos.Any())
                    {
                        try
                        {
                            _productService.CreateVariants(productDto.Id, variantCreateDtos);
                        }
                        catch (Exception variantEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"Lỗi tạo variants: {variantEx.Message}");
                            // Không throw exception, chỉ log vì sản phẩm đã được tạo thành công
                        }
                    }
                }

                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = true, message = "Tạo sản phẩm thành công", redirectUrl = Url.Action("Index", "Product") });
                }

                TempData["SuccessMessage"] = "Tạo sản phẩm thành công";
                return RedirectToAction("Index", "Product");
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết
                System.Diagnostics.Debug.WriteLine($"Error creating product: {ex.Message}");
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

                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = errorMessage });
                }

                // Khi là request bình thường, trả về lại view với message lỗi
                viewModel.Categories = new SelectList(_categoryRepository.GetAll().Where(c => c.IsActive && c.ParentCategoryId != null), "Id", "Name", viewModel.CategoryId);
                viewModel.Brands = new SelectList(_brandRepository.GetAll().Where(b => b.IsActive), "Id", "Name", viewModel.BrandId);
                viewModel.Genders = new SelectList(new[]
                {
                    new { Value = "Nam", Text = "Nam" },
                    new { Value = "Nữ", Text = "Nữ" },
                    new { Value = "Unisex", Text = "Unisex" }
                }, "Value", "Text", viewModel.Gender);
                viewModel.Seasons = new SelectList(new[]
                {
                    new { Value = "Xuân", Text = "Xuân" },
                    new { Value = "Hè", Text = "Hè" },
                    new { Value = "Thu", Text = "Thu" },
                    new { Value = "Đông", Text = "Đông" },
                    new { Value = "Quanh năm", Text = "Quanh năm" }
                }, "Value", "Text", viewModel.Season);

                TempData["ErrorMessage"] = errorMessage;
                return View(viewModel);
            }
        }

        // GET: Admin/Product/Edit/5
        public ActionResult Edit(int id)
        {
            // TODO: Bật lại khi cần check phân quyền
            //if (Session["AdminUser"] == null)
            //{
            //    return RedirectToAction("Login", "Account", new { area = "Admin" });
            //}

            var productDto = _productService.GetById(id);
            if (productDto == null)
            {
                return HttpNotFound("Sản phẩm không tồn tại");
            }

            var viewModel = new ProductCreateViewModel
            {
                Name = productDto.Name,
                Alias = productDto.Alias,
                Description = productDto.Description,
                Content = productDto.Content, // Mô tả chi tiết
                Price = productDto.Price,
                CompareAtPrice = productDto.CompareAtPrice,
                // MainImageUrl đã bỏ, ảnh chính lấy từ ProductImages với IsMain = true
                CategoryId = productDto.CategoryId,
                BrandId = productDto.BrandId,
                Material = productDto.Material,
                Origin = productDto.Origin,
                Style = productDto.Style,
                Season = productDto.Season,
                Gender = productDto.Gender,
                StockQuantity = productDto.StockQuantity,
                SKU = productDto.SKU,
                IsActive = productDto.IsActive,
                IsFeatured = productDto.IsFeatured,
                IsOnSale = productDto.IsOnSale,
                Categories = new SelectList(_categoryRepository.GetAll().Where(c => c.IsActive && c.ParentCategoryId != null), "Id", "Name", productDto.CategoryId), // Chỉ hiển thị danh mục bậc 2
                Brands = new SelectList(_brandRepository.GetAll().Where(b => b.IsActive), "Id", "Name", productDto.BrandId),
                Genders = new SelectList(new[]
                {
                    new { Value = "Nam", Text = "Nam" },
                    new { Value = "Nữ", Text = "Nữ" },
                    new { Value = "Unisex", Text = "Unisex" }
                }, "Value", "Text", productDto.Gender),
                Seasons = new SelectList(new[]
                {
                    new { Value = "Xuân", Text = "Xuân" },
                    new { Value = "Hạ", Text = "Hạ" },
                    new { Value = "Thu", Text = "Thu" },
                    new { Value = "Đông", Text = "Đông" },
                    new { Value = "Quanh năm", Text = "Quanh năm" }
                }, "Value", "Text", productDto.Season)
            };

            // Lấy ảnh chính từ ProductImages
            var mainImage = _productService.GetProductMainImage(id);
            var allImages = _productService.GetProductImages(id);

            ViewBag.ProductId = id;
            ViewBag.MainImageUrl = mainImage?.ImageUrl; // Ảnh chính (IsMain = true)
            ViewBag.AllImages = allImages; // Tất cả ảnh (để hiển thị gallery)
            ViewBag.Title = "Sửa sản phẩm";
            return View(viewModel);
        }

        // POST: Admin/Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, ProductCreateViewModel viewModel)
        {
            // TODO: Bật lại khi cần check phân quyền
            //if (Session["AdminUser"] == null)
            //{
            //    return Json(new { success = false, message = "Vui lòng đăng nhập" });
            //}

            if (!ModelState.IsValid)
            {
                // Chỉ hiển thị danh mục bậc 2 (có parent) vì sản phẩm nên gán vào danh mục cụ thể
                viewModel.Categories = new SelectList(_categoryRepository.GetAll().Where(c => c.IsActive && c.ParentCategoryId != null), "Id", "Name", viewModel.CategoryId);
                viewModel.Brands = new SelectList(_brandRepository.GetAll().Where(b => b.IsActive), "Id", "Name", viewModel.BrandId);
                viewModel.Genders = new SelectList(new[]
                {
                    new { Value = "Nam", Text = "Nam" },
                    new { Value = "Nữ", Text = "Nữ" },
                    new { Value = "Unisex", Text = "Unisex" }
                }, "Value", "Text", viewModel.Gender);
                viewModel.Seasons = new SelectList(new[]
                {
                    new { Value = "Xuân", Text = "Xuân" },
                    new { Value = "Hạ", Text = "Hạ" },
                    new { Value = "Thu", Text = "Thu" },
                    new { Value = "Đông", Text = "Đông" },
                    new { Value = "Quanh năm", Text = "Quanh năm" }
                }, "Value", "Text", viewModel.Season);

                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join("<br>", errors) });
            }

            try
            {
                var productDto = _productService.GetById(id);
                if (productDto == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại" });
                }

                // Map từ ViewModel sang DTO
                var productUpdateDto = _mapper.Map<ProductCreateViewModel, ProductUpdateDto>(viewModel);
                productUpdateDto.Id = id;

                // Generate alias nếu chưa có
                if (string.IsNullOrWhiteSpace(productUpdateDto.Alias))
                {
                    productUpdateDto.Alias = E_Commerce.Common.Helpers.AliasHelper.GenerateAlias(viewModel.Name);
                }

                // Cập nhật sản phẩm
                var updatedProduct = _productService.Update(id, productUpdateDto);

                // Upload ảnh sản phẩm mới (ảnh đầu tiên sẽ là ảnh chính với IsMain = true)
                var productImageUrls = new List<string>();
                var uploadedFileNames = new HashSet<string>(); // Để tránh upload trùng lặp

                // Lấy tất cả các file có name là "ProductImages"
                // Khi input có multiple, cần lặp qua tất cả files và kiểm tra name
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];
                    // Kiểm tra name của input (không phải FileName - tên file thực tế)
                    if (file != null && file.ContentLength > 0 && !string.IsNullOrEmpty(file.FileName))
                    {
                        // Kiểm tra xem file này có phải từ input "ProductImages" không
                        string inputName = Request.Files.AllKeys[i];
                        if (inputName == "ProductImages" || (inputName != null && inputName.StartsWith("ProductImages")))
                        {
                            // Kiểm tra trùng lặp dựa trên tên file và kích thước
                            string fileKey = $"{file.FileName}_{file.ContentLength}";
                            if (!uploadedFileNames.Contains(fileKey))
                            {
                                uploadedFileNames.Add(fileKey);
                                try
                                {
                                    System.Diagnostics.Debug.WriteLine($"[Upload] File {i}: {file.FileName}, Size: {file.ContentLength}, Key: {inputName}");
                                    var imageUrl = await CloudinaryHelper.UploadImageAsync(file, "products");
                                    if (!string.IsNullOrEmpty(imageUrl))
                                    {
                                        productImageUrls.Add(imageUrl);
                                        System.Diagnostics.Debug.WriteLine($"[Upload] Success: {imageUrl}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Log lỗi nhưng không dừng quá trình cập nhật
                                    System.Diagnostics.Debug.WriteLine($"Lỗi upload ảnh {i}: {ex.Message}");
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"[Upload] Skip duplicate: {file.FileName}");
                            }
                        }
                    }
                }

                // Thêm ảnh vào database: ảnh đầu tiên là ảnh chính (IsMain = true), các ảnh sau là ảnh phụ (IsMain = false)
                if (productImageUrls.Any())
                {
                    // Ảnh đầu tiên là ảnh chính
                    _productService.AddProductMainImage(id, productImageUrls[0]);

                    // Các ảnh còn lại là ảnh phụ
                    if (productImageUrls.Count > 1)
                    {
                        var additionalImageUrls = productImageUrls.Skip(1).ToList();
                        _productService.AddProductImages(id, additionalImageUrls);
                    }
                }

                return Json(new { success = true, message = "Cập nhật sản phẩm thành công", redirectUrl = Url.Action("Index", "Product") });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating product: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Admin/Product
        public ActionResult Index(ProductFilterViewModel filter)
        {
            // TODO: Bật lại khi cần check phân quyền
            //if (Session["AdminUser"] == null)
            //{
            //    return RedirectToAction("Login", "Account", new { area = "Admin" });
            //}

            ViewBag.Title = "Quản lý sản phẩm";

            // Nếu filter null, khởi tạo mặc định
            if (filter == null)
            {
                filter = new ProductFilterViewModel();
            }

            // Lấy danh sách products đã lọc
            var products = _productService.SearchProducts(
                searchTerm: filter.SearchTerm,
                isActive: filter.IsActive,
                categoryId: filter.CategoryId,
                brandId: filter.BrandId,
                isFeatured: filter.IsFeatured,
                isOnSale: filter.IsOnSale,
                sortBy: filter.SortBy ?? "name",
                sortOrder: filter.SortOrder ?? "asc"
            );

            // Lấy ảnh chính cho mỗi product
            var productMainImages = new Dictionary<int, string>();
            foreach (var product in products)
            {
                var mainImage = _productService.GetProductMainImage(product.Id);
                if (mainImage != null)
                {
                    productMainImages[product.Id] = mainImage.ImageUrl;
                }
            }
            ViewBag.ProductMainImages = productMainImages;

            // Truyền filter vào ViewBag để giữ lại giá trị trong form
            ViewBag.Filter = filter;

            // Truyền danh sách categories và brands cho dropdown filter
            ViewBag.Categories = new SelectList(_categoryRepository.GetAll().Where(c => c.IsActive && c.ParentCategoryId != null).OrderBy(c => c.Name), "Id", "Name", filter.CategoryId);
            ViewBag.Brands = new SelectList(_brandRepository.GetAll().Where(b => b.IsActive).OrderBy(b => b.Name), "Id", "Name", filter.BrandId);

            return View(products);
        }

        // POST: Admin/Product/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                var product = _productService.GetById(id);
                if (product == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại" });
                }

                _productService.Delete(id); // xóa mềm (IsDeleted = true)

                return Json(new { success = true, message = "Đã xóa sản phẩm thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}