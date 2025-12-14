using AutoMapper;
using E_Commerce.Data.Infrastructure;
using E_Commerce.Data.Repositories;
using E_Commerce.Dto;
using E_Commerce.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace E_Commerce.Service
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(
            ICategoryRepository categoryRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public CategoryDto Create(CategoryCreateDto categoryCreateDto)
        {
            // Validate parent category exists if provided
            if (categoryCreateDto.ParentCategoryId.HasValue)
            {
            var parentCategory = _categoryRepository.GetSingleById(categoryCreateDto.ParentCategoryId.Value);
            if (parentCategory == null || parentCategory.IsDeleted)
                {
                    throw new Exception("Danh mục cha không tồn tại");
                }
            }

            // Check if category name already exists
            var existingCategory = _categoryRepository.GetSingleByCondition(c => c.Name == categoryCreateDto.Name && c.IsActive && !c.IsDeleted);
            if (existingCategory != null)
            {
                throw new Exception("Tên danh mục đã tồn tại");
            }

            // Map từ DTO sang Model
            var category = _mapper.Map<CategoryCreateDto, Category>(categoryCreateDto);

            // Set các giá trị mặc định
            category.CreatedDate = DateTime.Now;
            category.IsActive = categoryCreateDto.IsActive;
            category.IsDeleted = false;

            // Thêm vào database
            _categoryRepository.Add(category);
            _unitOfWork.Commit();

            // Map lại sang DTO để trả về
            return _mapper.Map<Category, CategoryDto>(category);
        }

        public CategoryDto Update(int id, CategoryUpdateDto categoryUpdateDto)
        {
            var category = _categoryRepository.GetSingleById(id);
            if (category == null || category.IsDeleted)
            {
                throw new Exception("Danh mục không tồn tại");
            }

            // Validate parent category exists if provided
            if (categoryUpdateDto.ParentCategoryId.HasValue)
            {
                // Không cho phép set chính nó làm parent
                if (categoryUpdateDto.ParentCategoryId.Value == id)
                {
                    throw new Exception("Không thể chọn chính danh mục này làm danh mục cha");
                }

            var parentCategory = _categoryRepository.GetSingleById(categoryUpdateDto.ParentCategoryId.Value);
            if (parentCategory == null || parentCategory.IsDeleted)
                {
                    throw new Exception("Danh mục cha không tồn tại");
                }
            }

            // Check if category name already exists (excluding current category)
            var existingCategory = _categoryRepository.GetSingleByCondition(c => c.Name == categoryUpdateDto.Name && c.Id != id && c.IsActive && !c.IsDeleted);
            if (existingCategory != null)
            {
                throw new Exception("Tên danh mục đã tồn tại");
            }

            // Map từ DTO sang Model (giữ nguyên CreatedDate)
            _mapper.Map(categoryUpdateDto, category);
            category.UpdatedDate = DateTime.Now;

            // Cập nhật vào database
            _categoryRepository.Update(category);
            _unitOfWork.Commit();

            // Map lại sang DTO để trả về
            return _mapper.Map<Category, CategoryDto>(category);
        }

        public bool Delete(int id)
        {
            var category = _categoryRepository.GetSingleById(id);
            if (category == null || category.IsDeleted)
            {
                throw new Exception("Danh mục không tồn tại");
            }

            // Kiểm tra xem có danh mục con không
            var subCategories = _categoryRepository.GetMulti(c => c.ParentCategoryId == id && c.IsActive && !c.IsDeleted).ToList();
            if (subCategories.Any())
            {
                throw new Exception("Không thể xóa danh mục này vì còn danh mục con");
            }

            // Soft delete: chỉ set IsDeleted = true (không động vào IsActive)
            category.IsDeleted = true;
            category.UpdatedDate = DateTime.Now;

            _categoryRepository.Update(category);
            _unitOfWork.Commit();

            return true;
        }

        public CategoryDto GetById(int id)
        {
            var category = _categoryRepository.GetSingleById(id);
            if (category == null)
            {
                return null;
            }

            return _mapper.Map<Category, CategoryDto>(category);
        }

        public List<CategoryDto> GetAll()
        {
            var categories = _categoryRepository.GetMulti(c => !c.IsDeleted).OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name).ToList();
            return _mapper.Map<List<Category>, List<CategoryDto>>(categories);
        }

        public List<CategoryDto> GetActiveCategories()
        {
            var categories = _categoryRepository.GetMulti(c => c.IsActive && !c.IsDeleted)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToList();
            return _mapper.Map<List<Category>, List<CategoryDto>>(categories);
        }

        public List<CategoryDto> GetSubCategories(int parentCategoryId)
        {
            var categories = _categoryRepository.GetMulti(c => c.ParentCategoryId == parentCategoryId && c.IsActive && !c.IsDeleted).ToList();
            return _mapper.Map<List<Category>, List<CategoryDto>>(categories);
        }

        public List<CategoryDto> GetRootCategories()
        {
            var categories = _categoryRepository.GetMulti(c => c.ParentCategoryId == null && c.IsActive && !c.IsDeleted).ToList();
            return _mapper.Map<List<Category>, List<CategoryDto>>(categories);
        }

        public List<CategoryDto> SearchCategories(string searchTerm = null, bool? isActive = null, int? level = null, string sortBy = "name", string sortOrder = "asc")
        {
            // Bắt đầu với tất cả categories
            var query = _categoryRepository.GetAll().AsQueryable();

            // Ẩn danh mục đã xóa mềm
            query = query.Where(c => !c.IsDeleted);

            // Lọc theo từ khóa tìm kiếm (tên danh mục)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c => c.Name.Contains(searchTerm) || 
                                         (c.Description != null && c.Description.Contains(searchTerm)));
            }

            // Lọc theo trạng thái
            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            // Lọc theo cấp độ
            if (level.HasValue)
            {
                if (level.Value == 1)
                {
                    // Chỉ danh mục bậc 1 (không có parent)
                    query = query.Where(c => c.ParentCategoryId == null);
                }
                else if (level.Value == 2)
                {
                    // Chỉ danh mục bậc 2 (có parent)
                    query = query.Where(c => c.ParentCategoryId != null);
                }
            }

            // Sắp xếp
            switch (sortBy?.ToLower())
            {
                case "name":
                    query = sortOrder?.ToLower() == "desc" 
                        ? query.OrderByDescending(c => c.Name) 
                        : query.OrderBy(c => c.Name);
                    break;
                case "createddate":
                    query = sortOrder?.ToLower() == "desc" 
                        ? query.OrderByDescending(c => c.CreatedDate) 
                        : query.OrderBy(c => c.CreatedDate);
                    break;
                case "displayorder":
                    query = sortOrder?.ToLower() == "desc" 
                        ? query.OrderByDescending(c => c.DisplayOrder) 
                        : query.OrderBy(c => c.DisplayOrder);
                    break;
                default:
                    query = query.OrderBy(c => c.Name);
                    break;
            }

            var categories = query.ToList();
            return _mapper.Map<List<Category>, List<CategoryDto>>(categories);
        }

        public List<CategoryDto> GetHomeCategories()
        {
            // Lấy danh mục bậc 1 (không có parent) có HomeFlag = true và IsActive = true
            var categories = _categoryRepository.GetMulti(c => 
                c.ParentCategoryId == null && 
                c.HomeFlag == true && 
                c.IsActive == true &&
                c.IsDeleted == false)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToList();
            
            return _mapper.Map<List<Category>, List<CategoryDto>>(categories);
        }

        public List<int> GetAllChildCategoryIds(int parentCategoryId)
        {
            var result = new List<int> { parentCategoryId };
            var directChildren = _categoryRepository.GetMulti(c => c.ParentCategoryId == parentCategoryId && c.IsActive && !c.IsDeleted).ToList();
            
            foreach (var child in directChildren)
            {
                result.Add(child.Id);
                // Đệ quy lấy tất cả category con của child
                var grandChildren = GetAllChildCategoryIds(child.Id);
                result.AddRange(grandChildren);
            }
            
            return result.Distinct().ToList();
        }
    }
}

