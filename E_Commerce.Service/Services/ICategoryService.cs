using E_Commerce.Dto;
using System.Collections.Generic;

namespace E_Commerce.Service
{
    public interface ICategoryService
    {
        /// <summary>
        /// Tạo mới danh mục
        /// </summary>
        CategoryDto Create(CategoryCreateDto categoryCreateDto);

        /// <summary>
        /// Cập nhật danh mục
        /// </summary>
        CategoryDto Update(int id, CategoryUpdateDto categoryUpdateDto);

        /// <summary>
        /// Xóa danh mục (soft delete)
        /// </summary>
        bool Delete(int id);

        /// <summary>
        /// Lấy danh mục theo ID
        /// </summary>
        CategoryDto GetById(int id);

        /// <summary>
        /// Lấy tất cả danh mục
        /// </summary>
        List<CategoryDto> GetAll();

        /// <summary>
        /// Lấy danh mục đang kích hoạt
        /// </summary>
        List<CategoryDto> GetActiveCategories();

        /// <summary>
        /// Lấy danh mục con theo danh mục cha
        /// </summary>
        List<CategoryDto> GetSubCategories(int parentCategoryId);

        /// <summary>
        /// Lấy danh mục gốc (không có parent)
        /// </summary>
        List<CategoryDto> GetRootCategories();

        /// <summary>
        /// Tìm kiếm và lọc danh mục
        /// </summary>
        /// <param name="searchTerm">Từ khóa tìm kiếm (tên danh mục)</param>
        /// <param name="isActive">Lọc theo trạng thái (null = tất cả, true = kích hoạt, false = tắt)</param>
        /// <param name="level">Lọc theo cấp độ (null = tất cả, 1 = chỉ bậc 1, 2 = chỉ bậc 2)</param>
        /// <param name="sortBy">Sắp xếp theo (name, createdDate, displayOrder)</param>
        /// <param name="sortOrder">Thứ tự sắp xếp (asc, desc)</param>
        /// <returns>Danh sách danh mục đã lọc</returns>
        List<CategoryDto> SearchCategories(string searchTerm = null, bool? isActive = null, int? level = null, string sortBy = "name", string sortOrder = "asc");

        /// <summary>
        /// Lấy danh mục bậc 1 có HomeFlag = true (hiển thị trên trang chủ)
        /// </summary>
        List<CategoryDto> GetHomeCategories();

        /// <summary>
        /// Lấy tất cả danh mục con (đệ quy) của một danh mục
        /// </summary>
        List<int> GetAllChildCategoryIds(int parentCategoryId);
    }
}

