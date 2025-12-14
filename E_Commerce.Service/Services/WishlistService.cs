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
    public class WishlistService : IWishlistService
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public WishlistService(
            IWishlistRepository wishlistRepository,
            IProductRepository productRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _wishlistRepository = wishlistRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public IEnumerable<WishlistDto> GetByUser(int userId)
        {
            var items = _wishlistRepository.GetMulti(
                w => w.UserId == userId,
                new[] { "Product", "Product.ProductImages", "User" })
                .OrderByDescending(w => w.CreatedDate)
                .ToList();

            return _mapper.Map<IEnumerable<Wishlist>, IEnumerable<WishlistDto>>(items);
        }

        public WishlistDto Add(int userId, int productId)
        {
            var product = _productRepository.GetSingleById(productId);
            if (product == null || product.IsDeleted || !product.IsActive)
            {
                throw new Exception("Sản phẩm không tồn tại hoặc đã ngừng bán.");
            }

            var existing = _wishlistRepository.GetSingleByCondition(
                w => w.UserId == userId && w.ProductId == productId);
            if (existing != null)
            {
                return GetWishlistDto(existing.Id);
            }

            var wishlist = new Wishlist
            {
                UserId = userId,
                ProductId = productId,
                CreatedDate = DateTime.Now
            };

            _wishlistRepository.Add(wishlist);
            _unitOfWork.Commit();

            return GetWishlistDto(wishlist.Id);
        }

        public bool Remove(int userId, int productId)
        {
            var item = _wishlistRepository.GetSingleByCondition(
                w => w.UserId == userId && w.ProductId == productId);

            if (item == null) return false;

            _wishlistRepository.Delete(item);
            _unitOfWork.Commit();
            return true;
        }

        public bool Clear(int userId)
        {
            _wishlistRepository.DeleteMulti(w => w.UserId == userId);
            _unitOfWork.Commit();
            return true;
        }

        public int Count(int userId)
        {
            return _wishlistRepository.Count(w => w.UserId == userId);
        }

        public bool Exists(int userId, int productId)
        {
            return _wishlistRepository.CheckContains(w => w.UserId == userId && w.ProductId == productId);
        }

        public List<int> GetWishlistProductIds(int userId)
        {
            return _wishlistRepository.GetMulti(w => w.UserId == userId)
                .Select(w => w.ProductId)
                .ToList();
        }

        private WishlistDto GetWishlistDto(int id)
        {
            var item = _wishlistRepository.GetSingleByCondition(
                w => w.Id == id,
                new[] { "Product", "Product.ProductImages", "User" });

            if (item == null) return null;

            return _mapper.Map<Wishlist, WishlistDto>(item);
        }
    }
}

