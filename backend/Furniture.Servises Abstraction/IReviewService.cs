using Furniture.shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Furniture.Servises_Abstraction
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewDto>> GetProductReviewsAsync(int productId, int pageIndex, int pageSize);
        Task<ReviewDto> CreateReviewAsync(ReviewCreateDto dto);
        Task<IEnumerable<int>> GetUserReviewedProductIdsAsync(string userId);
        Task DeleteReviewAsync(int id);
    }
}