using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using E_Commerce.Data.Infrastructure;
using E_Commerce.Data.Repositories;
using E_Commerce.Dto;
using E_Commerce.Model.Models;

namespace E_Commerce.Service
{
    public class DiscountCodeService : IDiscountCodeService
    {
        private readonly IDiscountCodeRepository _discountCodeRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DiscountCodeService(
            IDiscountCodeRepository discountCodeRepository,
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _discountCodeRepository = discountCodeRepository;
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public DiscountCodeDto GetById(int id)
        {
            var discountCode = _discountCodeRepository.GetSingleById(id);
            if (discountCode == null || discountCode.IsDeleted) return null;

            return _mapper.Map<DiscountCode, DiscountCodeDto>(discountCode);
        }

        public List<DiscountCodeDto> GetAll()
        {
            var discountCodes = _discountCodeRepository.GetMulti(dc => !dc.IsDeleted).ToList();
            return _mapper.Map<List<DiscountCode>, List<DiscountCodeDto>>(discountCodes);
        }

        public List<DiscountCodeDto> SearchDiscountCodes(string searchTerm = null, bool? isActive = null, string sortBy = "createdDate", string sortOrder = "desc")
        {
            var query = _discountCodeRepository.GetMulti(dc => !dc.IsDeleted);

            // Filter by search term
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(dc => 
                    dc.Code.Contains(searchTerm) || 
                    dc.Name.Contains(searchTerm));
            }

            // Filter by active status
            if (isActive.HasValue)
            {
                query = query.Where(dc => dc.IsActive == isActive.Value);
            }

            // Sort
            switch (sortBy.ToLower())
            {
                case "code":
                    query = sortOrder.ToLower() == "asc" 
                        ? query.OrderBy(dc => dc.Code)
                        : query.OrderByDescending(dc => dc.Code);
                    break;
                case "name":
                    query = sortOrder.ToLower() == "asc"
                        ? query.OrderBy(dc => dc.Name)
                        : query.OrderByDescending(dc => dc.Name);
                    break;
                case "startdate":
                    query = sortOrder.ToLower() == "asc"
                        ? query.OrderBy(dc => dc.StartDate)
                        : query.OrderByDescending(dc => dc.StartDate);
                    break;
                case "enddate":
                    query = sortOrder.ToLower() == "asc"
                        ? query.OrderBy(dc => dc.EndDate)
                        : query.OrderByDescending(dc => dc.EndDate);
                    break;
                default: // createdDate
                    query = sortOrder.ToLower() == "asc"
                        ? query.OrderBy(dc => dc.CreatedDate)
                        : query.OrderByDescending(dc => dc.CreatedDate);
                    break;
            }

            var discountCodes = query.ToList();
            return _mapper.Map<List<DiscountCode>, List<DiscountCodeDto>>(discountCodes);
        }

        public DiscountCodeDto Create(DiscountCodeCreateDto createDto)
        {
            // Check if code already exists (chỉ kiểm tra những mã chưa bị xóa)
            var existing = _discountCodeRepository.GetSingleByCondition(
                dc => dc.Code.ToUpper() == createDto.Code.ToUpper() && !dc.IsDeleted);
            if (existing != null)
            {
                throw new Exception("Mã giảm giá đã tồn tại.");
            }

            // Validate dates
            var now = DateTime.Now.Date;
            if (createDto.StartDate.Date < now)
            {
                throw new Exception("Ngày bắt đầu không được nhỏ hơn ngày hiện tại.");
            }
            
            if (createDto.EndDate <= createDto.StartDate)
            {
                throw new Exception("Ngày kết thúc phải sau ngày bắt đầu.");
            }

            var discountCode = _mapper.Map<DiscountCodeCreateDto, DiscountCode>(createDto);
            discountCode.CreatedDate = DateTime.Now;
            discountCode.UsedCount = 0;
            discountCode.IsDeleted = false;

            _discountCodeRepository.Add(discountCode);
            _unitOfWork.Commit();

            return _mapper.Map<DiscountCode, DiscountCodeDto>(discountCode);
        }

        public DiscountCodeDto Update(int id, DiscountCodeUpdateDto updateDto)
        {
            var discountCode = _discountCodeRepository.GetSingleById(id);
            if (discountCode == null || discountCode.IsDeleted)
            {
                throw new Exception("Mã giảm giá không tồn tại.");
            }

            // Check if code already exists (excluding current, chỉ kiểm tra những mã chưa bị xóa)
            var existing = _discountCodeRepository.GetSingleByCondition(
                dc => dc.Code.ToUpper() == updateDto.Code.ToUpper() && dc.Id != id && !dc.IsDeleted);
            if (existing != null)
            {
                throw new Exception("Mã giảm giá đã tồn tại.");
            }

            // Validate dates
            var now = DateTime.Now.Date;
            // Chỉ validate nếu ngày bắt đầu mới khác với ngày cũ và nhỏ hơn hôm nay
            if (updateDto.StartDate.Date != discountCode.StartDate.Date && updateDto.StartDate.Date < now)
            {
                throw new Exception("Ngày bắt đầu không được nhỏ hơn ngày hiện tại.");
            }
            
            if (updateDto.EndDate <= updateDto.StartDate)
            {
                throw new Exception("Ngày kết thúc phải sau ngày bắt đầu.");
            }

            // Map properties
            discountCode.Code = updateDto.Code;
            discountCode.Name = updateDto.Name;
            discountCode.DiscountType = updateDto.DiscountType;
            discountCode.DiscountValue = updateDto.DiscountValue;
            discountCode.MinOrderAmount = updateDto.MinOrderAmount;
            discountCode.UsageLimit = updateDto.UsageLimit;
            discountCode.PerUserLimit = updateDto.PerUserLimit;
            discountCode.StartDate = updateDto.StartDate;
            discountCode.EndDate = updateDto.EndDate;
            discountCode.IsActive = updateDto.IsActive;
            discountCode.UpdatedDate = DateTime.Now;

            _discountCodeRepository.Update(discountCode);
            _unitOfWork.Commit();

            return _mapper.Map<DiscountCode, DiscountCodeDto>(discountCode);
        }

        public bool Delete(int id)
        {
            var discountCode = _discountCodeRepository.GetSingleById(id);
            if (discountCode == null || discountCode.IsDeleted)
            {
                throw new Exception("Mã giảm giá không tồn tại.");
            }

            // Soft delete: chỉ set IsDeleted = true (không động IsActive)
            // Giữ lại để đảm bảo tính toàn vẹn dữ liệu với Orders
            discountCode.IsDeleted = true;
            discountCode.UpdatedDate = DateTime.Now;

            _discountCodeRepository.Update(discountCode);
            _unitOfWork.Commit();

            return true;
        }

        public DiscountCodeDto ValidateDiscountCode(string code, decimal totalAmount, int? userId = null)
        {
            var discountCode = _discountCodeRepository.GetSingleByCondition(
                dc => dc.Code.ToUpper() == code.ToUpper().Trim() && dc.IsActive && !dc.IsDeleted);

            if (discountCode == null)
            {
                return null;
            }

            // Check validity
            var now = DateTime.Now;
            if (discountCode.StartDate > now || discountCode.EndDate < now)
            {
                return null;
            }

            // Check total usage limit
            if (discountCode.UsageLimit.HasValue && discountCode.UsedCount >= discountCode.UsageLimit.Value)
            {
                return null;
            }

            // Check per-user usage limit
            if (userId.HasValue && discountCode.PerUserLimit.HasValue)
            {
                var userUsageCount = _orderRepository.GetAll()
                    .Count(o => o.UserId == userId.Value && 
                                o.DiscountCodeId == discountCode.Id);
                
                if (userUsageCount >= discountCode.PerUserLimit.Value)
                {
                    return null;
                }
            }

            if (discountCode.MinOrderAmount.HasValue && totalAmount < discountCode.MinOrderAmount.Value)
            {
                return null;
            }

            return _mapper.Map<DiscountCode, DiscountCodeDto>(discountCode);
        }
    }
}

