using System.Collections.Generic;
using E_Commerce.Dto;

namespace E_Commerce.Service
{
    public interface IDiscountCodeService
    {
        DiscountCodeDto GetById(int id);
        List<DiscountCodeDto> GetAll();
        List<DiscountCodeDto> SearchDiscountCodes(string searchTerm = null, bool? isActive = null, string sortBy = "createdDate", string sortOrder = "desc");
        DiscountCodeDto Create(DiscountCodeCreateDto createDto);
        DiscountCodeDto Update(int id, DiscountCodeUpdateDto updateDto);
        bool Delete(int id);
        DiscountCodeDto ValidateDiscountCode(string code, decimal totalAmount, int? userId = null);
    }
}

