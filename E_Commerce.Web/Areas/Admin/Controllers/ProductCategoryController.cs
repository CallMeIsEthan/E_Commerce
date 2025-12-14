using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using E_Commerce.Service;
using E_Commerce.Dto;
using AutoMapper;

namespace E_Commerce.Web.Areas.Admin.Controllers
{
    [RoutePrefix("api/admin/productcategory")]
    public class ProductCategoryController : ApiController
    {
        private readonly ICategoryService _categoryService;
        private readonly IMapper _mapper;

        public ProductCategoryController(
            ICategoryService categoryService,
            IMapper mapper)
        {
            _categoryService = categoryService;
            _mapper = mapper;
        }

        // GET: api/admin/productcategory
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll(string searchTerm = null, bool? isActive = null, int? level = null, string sortBy = "name", string sortOrder = "asc")
        {
            try
            {
                var categories = _categoryService.SearchCategories(
                    searchTerm: searchTerm,
                    isActive: isActive,
                    level: level,
                    sortBy: sortBy,
                    sortOrder: sortOrder
                );

                return Ok(new { success = true, data = categories, total = categories.Count });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/admin/productcategory/{id}
        [HttpGet]
        [Route("{id}")]
        public IHttpActionResult GetById(int id)
        {
            try
            {
                var category = _categoryService.GetById(id);
                if (category == null)
                {
                    return NotFound();
                }

                return Ok(new { success = true, data = category });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/admin/productcategory
        [HttpPost]
        [Route("")]
        public IHttpActionResult Create([FromBody] CategoryCreateDto categoryDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(string.Join(", ", errors));
                }

                var createdCategory = _categoryService.Create(categoryDto);
                return Created(Request.RequestUri + "/" + createdCategory.Id, new { success = true, data = createdCategory, message = "Tạo danh mục thành công" });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT: api/admin/productcategory/{id}
        [HttpPut]
        [Route("{id}")]
        public IHttpActionResult Update(int id, [FromBody] CategoryUpdateDto categoryDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(string.Join(", ", errors));
                }

                var existingCategory = _categoryService.GetById(id);
                if (existingCategory == null)
                {
                    return NotFound();
                }

                categoryDto.Id = id;
                var updatedCategory = _categoryService.Update(id, categoryDto);
                return Ok(new { success = true, data = updatedCategory, message = "Cập nhật danh mục thành công" });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // DELETE: api/admin/productcategory/{id}
        [HttpDelete]
        [Route("{id}")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                var category = _categoryService.GetById(id);
                if (category == null)
                {
                    return NotFound();
                }

                _categoryService.Delete(id);
                return Ok(new { success = true, message = "Xóa danh mục thành công" });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/admin/productcategory/active
        [HttpGet]
        [Route("active")]
        public IHttpActionResult GetActiveCategories()
        {
            try
            {
                var categories = _categoryService.GetActiveCategories();
                return Ok(new { success = true, data = categories, total = categories.Count });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/admin/productcategory/root
        [HttpGet]
        [Route("root")]
        public IHttpActionResult GetRootCategories()
        {
            try
            {
                var categories = _categoryService.GetRootCategories();
                return Ok(new { success = true, data = categories, total = categories.Count });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
