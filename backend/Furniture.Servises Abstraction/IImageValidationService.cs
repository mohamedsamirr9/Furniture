using Furniture.shared.Dtos.ProductDtos;

namespace Furniture.Servises_Abstraction;


    public interface IImageValidationService
    {
        Task<ImageValidationResultDto> ValidateImageAsync(Stream imageStream, string fileName);
    }
