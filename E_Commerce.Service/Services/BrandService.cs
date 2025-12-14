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
    public class BrandService : IBrandService
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BrandService(
            IBrandRepository brandRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _brandRepository = brandRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public BrandDto Create(BrandCreateDto brandCreateDto)
        {
            // Check if brand name already exists
            var existingBrand = _brandRepository.GetSingleByCondition(b => b.Name == brandCreateDto.Name && b.IsActive && !b.IsDeleted);
            if (existingBrand != null)
            {
                throw new Exception("Tên thương hiệu đã tồn tại");
            }

            // Map từ DTO sang Model
            var brand = _mapper.Map<BrandCreateDto, Brand>(brandCreateDto);

            // Set các giá trị mặc định
            brand.CreatedDate = DateTime.Now;
            brand.IsActive = brandCreateDto.IsActive;
            brand.IsDeleted = false;

            // Thêm vào database
            _brandRepository.Add(brand);
            _unitOfWork.Commit();

            // Map lại sang DTO để trả về
            return _mapper.Map<Brand, BrandDto>(brand);
        }

        public BrandDto Update(int id, BrandUpdateDto brandUpdateDto)
        {
            var brand = _brandRepository.GetSingleById(id);
            if (brand == null || brand.IsDeleted)
            {
                throw new Exception("Thương hiệu không tồn tại");
            }

            // Check if brand name already exists (excluding current brand)
            var existingBrand = _brandRepository.GetSingleByCondition(b => b.Name == brandUpdateDto.Name && b.Id != id && b.IsActive && !b.IsDeleted);
            if (existingBrand != null)
            {
                throw new Exception("Tên thương hiệu đã tồn tại");
            }

            // Map từ DTO sang Model (giữ nguyên CreatedDate)
            _mapper.Map(brandUpdateDto, brand);
            brand.UpdatedDate = DateTime.Now;

            // Cập nhật vào database
            _brandRepository.Update(brand);
            _unitOfWork.Commit();

            // Map lại sang DTO để trả về
            return _mapper.Map<Brand, BrandDto>(brand);
        }

        public bool Delete(int id)
        {
            var brand = _brandRepository.GetSingleById(id);
            if (brand == null || brand.IsDeleted)
            {
                throw new Exception("Thương hiệu không tồn tại");
            }

            // Kiểm tra xem có sản phẩm nào đang sử dụng thương hiệu này không
            // Note: Products có thể null nếu chưa được load, nên chỉ kiểm tra nếu đã được load
            if (brand.Products != null && brand.Products.Any(p => p.IsActive))
            {
                throw new Exception("Không thể xóa thương hiệu này vì còn sản phẩm đang sử dụng");
            }

            // Soft delete: chỉ set IsDeleted = true (không động IsActive)
            brand.IsDeleted = true;
            brand.UpdatedDate = DateTime.Now;

            _brandRepository.Update(brand);
            _unitOfWork.Commit();

            return true;
        }

        public BrandDto GetById(int id)
        {
            var brand = _brandRepository.GetSingleById(id);
            if (brand == null)
            {
                return null;
            }

            return _mapper.Map<Brand, BrandDto>(brand);
        }

        public List<BrandDto> GetAll()
        {
            var brands = _brandRepository.GetMulti(b => !b.IsDeleted).ToList();
            return _mapper.Map<List<Brand>, List<BrandDto>>(brands);
        }

        public List<BrandDto> GetActiveBrands()
        {
            var brands = _brandRepository.GetMulti(b => b.IsActive && !b.IsDeleted).ToList();
            return _mapper.Map<List<Brand>, List<BrandDto>>(brands);
        }

        public List<BrandDto> SearchBrands(string searchTerm = null, bool? isActive = null, string sortBy = "name", string sortOrder = "asc")
        {
            // Bắt đầu với tất cả brands
            var query = _brandRepository.GetAll().AsQueryable();

            // Ẩn brand đã xóa mềm
            query = query.Where(b => !b.IsDeleted);

            // Lọc theo từ khóa tìm kiếm (tên thương hiệu)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(b => b.Name.Contains(searchTerm) || 
                                         (b.Description != null && b.Description.Contains(searchTerm)));
            }

            // Lọc theo trạng thái
            if (isActive.HasValue)
            {
                query = query.Where(b => b.IsActive == isActive.Value);
            }

            // Sắp xếp
            switch (sortBy?.ToLower())
            {
                case "name":
                    query = sortOrder?.ToLower() == "desc" 
                        ? query.OrderByDescending(b => b.Name) 
                        : query.OrderBy(b => b.Name);
                    break;
                case "createddate":
                    query = sortOrder?.ToLower() == "desc" 
                        ? query.OrderByDescending(b => b.CreatedDate) 
                        : query.OrderBy(b => b.CreatedDate);
                    break;
                case "displayorder":
                    query = sortOrder?.ToLower() == "desc" 
                        ? query.OrderByDescending(b => b.DisplayOrder) 
                        : query.OrderBy(b => b.DisplayOrder);
                    break;
                default:
                    query = query.OrderBy(b => b.Name);
                    break;
            }

            var brands = query.ToList();
            return _mapper.Map<List<Brand>, List<BrandDto>>(brands);
        }
    }
}

