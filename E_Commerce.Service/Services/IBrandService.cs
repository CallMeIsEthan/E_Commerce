using E_Commerce.Dto;
using System.Collections.Generic;

namespace E_Commerce.Service
{
    public interface IBrandService
    {
        /// <summary>
        /// Tạo mới thương hiệu
        /// </summary>
        BrandDto Create(BrandCreateDto brandCreateDto);

        /// <summary>
        /// Cập nhật thương hiệu
        /// </summary>
        BrandDto Update(int id, BrandUpdateDto brandUpdateDto);

        /// <summary>
        /// Xóa thương hiệu (soft delete)
        /// </summary>
        bool Delete(int id);

        /// <summary>
        /// Lấy thương hiệu theo ID
        /// </summary>
        BrandDto GetById(int id);

        /// <summary>
        /// Lấy tất cả thương hiệu
        /// </summary>
        List<BrandDto> GetAll();

        /// <summary>
        /// Lấy thương hiệu đang kích hoạt
        /// </summary>
        List<BrandDto> GetActiveBrands();

        /// <summary>
        /// Tìm kiếm và lọc thương hiệu
        /// </summary>
        /// <param name="searchTerm">Từ khóa tìm kiếm (tên thương hiệu)</param>
        /// <param name="isActive">Lọc theo trạng thái (null = tất cả, true = kích hoạt, false = tắt)</param>
        /// <param name="sortBy">Sắp xếp theo (name, createdDate, displayOrder)</param>
        /// <param name="sortOrder">Thứ tự sắp xếp (asc, desc)</param>
        /// <returns>Danh sách thương hiệu đã lọc</returns>
        List<BrandDto> SearchBrands(string searchTerm = null, bool? isActive = null, string sortBy = "name", string sortOrder = "asc");
    }
}

