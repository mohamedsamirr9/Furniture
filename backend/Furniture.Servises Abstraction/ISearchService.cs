using Furniture.shared.Dtos.SearchWithImage;
using Microsoft.AspNetCore.Http;

namespace Furniture.Servises_Abstraction;

public interface ISearchService
{
    Task<IEnumerable<ProductSearchResultDto>> SearchByImageAsync(IFormFile image, int topK = 5);
}