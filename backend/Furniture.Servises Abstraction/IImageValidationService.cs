using Furniture.shared.Dtos.ProductDtos;

namespace Furniture.Servises_Abstraction
{
    public interface IImageValidationService
    {
        Task<ImageValidationResult> ValidateAsync(Stream imageStream, string fileName, string contentType);
        Task<ImageValidationSummary> ValidateUrlsAsync(IEnumerable<string> imageUrls); 
    }

   
}