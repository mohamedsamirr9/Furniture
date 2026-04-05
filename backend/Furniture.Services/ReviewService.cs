using AutoMapper;
using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Services.Specifications;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services
{
    public class ReviewService(IUnitOfWork _unitOfWork, IMapper _mapper) : IReviewService
    {
        public async Task<IEnumerable<ReviewDto>> GetProductReviewsAsync(int productId, int pageIndex, int pageSize)
        {
            var repo = _unitOfWork.GetRepository<Review, int>();
            var spec = new ReviewSpecifications(productId, pageIndex, pageSize);
            var reviews = await repo.GetAllAsync(spec);
            return _mapper.Map<IEnumerable<Review>, IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<ReviewDto> CreateReviewAsync(ReviewCreateDto dto)
        {
            var repo = _unitOfWork.GetRepository<Review, int>();
            var review = _mapper.Map<Review>(dto);
            await repo.AddAsync(review);
            await _unitOfWork.SaveChangesAsync();

            var spec = new ReviewWithDetailsSpecification(review.Id);
            var created = await repo.GetByIdAsync(spec);
            return _mapper.Map<Review, ReviewDto>(created!);
        }

        public async Task DeleteReviewAsync(int id)
        {
            var repo = _unitOfWork.GetRepository<Review, int>();
            var review = await repo.GetByIdAsync(id);
            if (review is null) return;
            repo.Remove(review);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}