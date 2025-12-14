using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using E_Commerce.Common.Helpers;
using E_Commerce.Service;
using E_Commerce.Web.Attributes;
using E_Commerce.Web.ViewModels;
using AutoMapper;
using System.Collections.Generic;

namespace E_Commerce.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    public class ProductVariantController : Controller
    {
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public ProductVariantController(
            IProductService productService,
            IMapper mapper)
        {
            _productService = productService;
            _mapper = mapper;
        }

        // GET: Admin/ProductVariant/Index/5 (productId)
        public ActionResult Index(int productId)
        {
            // TODO: Bật lại khi cần check phân quyền
            //if (Session["AdminUser"] == null)
            //{
            //    return RedirectToAction("Login", "Account", new { area = "Admin" });
            //}

            var product = _productService.GetById(productId);
            if (product == null)
            {
                return HttpNotFound("Sản phẩm không tồn tại");
            }

            var variants = _productService.GetVariantsByProductId(productId);

            // Lấy ảnh chính cho mỗi variant
            var variantMainImages = new Dictionary<int, string>();
            foreach (var variant in variants)
            {
                var variantImages = _productService.GetVariantImages(variant.Id);
                var mainImage = variantImages.FirstOrDefault(vi => vi.IsMain);
                if (mainImage != null)
                {
                    variantMainImages[variant.Id] = mainImage.ImageUrl;
                }
            }
            ViewBag.VariantMainImages = variantMainImages;

            ViewBag.ProductId = productId;
            ViewBag.ProductName = product.Name;
            ViewBag.Title = $"Quản lý biến thể - {product.Name}";

            return View(variants);
        }

        // GET: Admin/ProductVariant/Create/5 (productId)
        public ActionResult Create(int productId)
        {
            // TODO: Bật lại khi cần check phân quyền
            //if (Session["AdminUser"] == null)
            //{
            //    return RedirectToAction("Login", "Account", new { area = "Admin" });
            //}

            var product = _productService.GetById(productId);
            if (product == null)
            {
                return HttpNotFound("Sản phẩm không tồn tại");
            }

            var viewModel = new ProductVariantViewModel
            {
                Price = product.Price, // Set giá từ product
                IsActive = true
            };

            ViewBag.ProductId = productId;
            ViewBag.ProductName = product.Name;
            ViewBag.ProductPrice = product.Price; // Truyền giá product vào ViewBag
            ViewBag.Title = $"Tạo biến thể mới - {product.Name}";

            return View(viewModel);
        }

        // POST: Admin/ProductVariant/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(int productId, ProductVariantViewModel viewModel)
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
                var product = _productService.GetById(productId);
                if (product == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại" });
                }

                // Map từ ViewModel sang DTO
                var variantCreateDto = _mapper.Map<ProductVariantViewModel, E_Commerce.Dto.ProductVariantCreateDto>(viewModel);
                variantCreateDto.ProductId = productId;
                variantCreateDto.Price = product.Price; // Lấy giá từ product, không phải từ viewModel

                // Tạo variant trước
                var variantDto = _productService.CreateVariant(variantCreateDto);

                // Kiểm tra xem có chọn "Dùng ảnh sản phẩm" không
                bool useProductImages = Request.Form["UseProductImages"] == "true";

                if (useProductImages)
                {
                    // Copy ảnh từ product sang variant
                    try
                    {
                        _productService.CopyProductImagesToVariant(productId, variantDto.Id);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Lỗi copy ảnh từ product sang variant: {ex.Message}");
                        // Không return error, chỉ log vì variant đã được tạo
                    }
                }
                else
                {
                    // Upload ảnh mới cho variant
                    var productImageUrls = new List<string>();
                    var uploadedFileNames = new HashSet<string>(); // Để tránh upload trùng lặp

                    // Lấy tất cả các file có name là "ProductImages"
                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        var file = Request.Files[i];
                        if (file != null && file.ContentLength > 0 && !string.IsNullOrEmpty(file.FileName))
                        {
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
                                        var imageUrl = await CloudinaryHelper.UploadImageAsync(file, "products/variants");
                                        if (!string.IsNullOrEmpty(imageUrl))
                                        {
                                            productImageUrls.Add(imageUrl);
                                            System.Diagnostics.Debug.WriteLine($"[Upload] Success: {imageUrl}");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"Lỗi upload ảnh variant {i}: {ex.Message}");
                                    }
                                }
                            }
                        }
                    }

                    // Thêm ảnh vào database: ảnh đầu tiên là ảnh chính (IsMain = true), các ảnh sau là ảnh phụ (IsMain = false)
                    if (productImageUrls.Any())
                    {
                        // Ảnh đầu tiên là ảnh chính
                        _productService.AddVariantMainImage(variantDto.Id, productImageUrls[0]);

                        // Các ảnh còn lại là ảnh phụ
                        if (productImageUrls.Count > 1)
                        {
                            var additionalImageUrls = productImageUrls.Skip(1).ToList();
                            _productService.AddVariantImages(variantDto.Id, additionalImageUrls);
                        }
                    }
                }

                return Json(new { success = true, message = "Tạo biến thể thành công", redirectUrl = Url.Action("Index", "ProductVariant", new { productId = productId }) });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating variant: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Admin/ProductVariant/Edit/5
        public ActionResult Edit(int id)
        {
            // TODO: Bật lại khi cần check phân quyền
            //if (Session["AdminUser"] == null)
            //{
            //    return RedirectToAction("Login", "Account", new { area = "Admin" });
            //}

            var variantDto = _productService.GetVariantById(id);
            if (variantDto == null)
            {
                return HttpNotFound("Biến thể không tồn tại");
            }

            // Lấy product để lấy giá
            var product = _productService.GetById(variantDto.ProductId);
            if (product == null)
            {
                return HttpNotFound("Sản phẩm không tồn tại");
            }

            // Lấy ảnh từ ProductVariantImages
            var variantImages = _productService.GetVariantImages(id);
            var mainImage = variantImages.FirstOrDefault(vi => vi.IsMain);

            var viewModel = new ProductVariantViewModel
            {
                Price = product.Price, // Lấy giá từ product, không phải từ variant
                Stock = variantDto.Stock,
                SKU = variantDto.SKU,
                Size = variantDto.Size,
                ColorName = variantDto.ColorName,
                ColorCode = variantDto.ColorCode,
                Pattern = variantDto.Pattern,
                // ImageUrl đã bỏ, ảnh lấy từ ProductVariantImages
                IsActive = variantDto.IsActive
            };

            ViewBag.VariantId = id;
            ViewBag.ProductId = variantDto.ProductId;
            ViewBag.ProductName = variantDto.ProductName;
            ViewBag.ProductPrice = product.Price; // Truyền giá product vào ViewBag
            ViewBag.MainImageUrl = mainImage?.ImageUrl; // Ảnh chính (IsMain = true)
            ViewBag.AllImages = variantImages; // Tất cả ảnh của variant
            ViewBag.Title = $"Sửa biến thể - {variantDto.ProductName}";

            // Đảm bảo không còn ModelState cũ ghi đè giá trị khi render form
            ModelState.Clear();

            return View(viewModel);
        }

        // POST: Admin/ProductVariant/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, ProductVariantViewModel viewModel)
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
                var variantDto = _productService.GetVariantById(id);
                if (variantDto == null)
                {
                    return Json(new { success = false, message = "Biến thể không tồn tại" });
                }

                // Lấy product để lấy giá
                var product = _productService.GetById(variantDto.ProductId);
                if (product == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại" });
                }

                // Map từ ViewModel sang DTO
                var variantUpdateDto = new E_Commerce.Dto.ProductVariantUpdateDto
                {
                    Id = id,
                    Price = product.Price, // Lấy giá từ product, không phải từ viewModel
                    Stock = viewModel.Stock,
                    SKU = viewModel.SKU,
                    Size = viewModel.Size,
                    ColorName = viewModel.ColorName,
                    ColorCode = viewModel.ColorCode,
                    Pattern = viewModel.Pattern,
                    // ImageUrl đã bỏ, ảnh lưu trong ProductVariantImages
                    IsActive = viewModel.IsActive
                };

                // Cập nhật variant
                var updatedVariant = _productService.UpdateVariant(id, variantUpdateDto);

                // Upload ảnh chính mới nếu có và lưu vào ProductVariantImages với IsMain = true
                if (Request.Files.Count > 0 && Request.Files["MainImage"] != null && Request.Files["MainImage"].ContentLength > 0)
                {
                    try
                    {
                        var file = Request.Files["MainImage"];
                        var mainImageUrl = await CloudinaryHelper.UploadImageAsync(file, "products/variants");
                        if (!string.IsNullOrEmpty(mainImageUrl))
                        {
                            _productService.AddVariantMainImage(id, mainImageUrl);
                        }
                    }
                    catch (Exception uploadEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Lỗi upload ảnh chính variant: {uploadEx.Message}");
                        // Không return error, chỉ log vì variant đã được cập nhật
                    }
                }

                // Upload các ảnh phụ cho variant (IsMain = false)
                var additionalImageUrls = new List<string>();
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];
                    if (file != null && file.ContentLength > 0 && file.FileName == "AdditionalImages")
                    {
                        try
                        {
                            var imageUrl = await CloudinaryHelper.UploadImageAsync(file, "products/variants");
                            if (!string.IsNullOrEmpty(imageUrl))
                            {
                                additionalImageUrls.Add(imageUrl);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Lỗi upload ảnh phụ variant: {ex.Message}");
                        }
                    }
                }

                // Thêm các ảnh phụ vào database
                if (additionalImageUrls.Any())
                {
                    _productService.AddVariantImages(id, additionalImageUrls);
                }

                return Json(new { success = true, message = "Cập nhật biến thể thành công", redirectUrl = Url.Action("Index", "ProductVariant", new { productId = variantDto.ProductId }) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Admin/ProductVariant/Delete/5
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
                var variant = _productService.GetVariantById(id);
                if (variant == null)
                {
                    return Json(new { success = false, message = "Biến thể không tồn tại" });
                }

                var productId = variant.ProductId;
                var result = _productService.DeleteVariant(id);
                return Json(new { success = true, message = "Xóa biến thể thành công", redirectUrl = Url.Action("Index", "ProductVariant", new { productId = productId }) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}