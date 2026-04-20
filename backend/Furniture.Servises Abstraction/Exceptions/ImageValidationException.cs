using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.ProductDtos;

namespace Furniture.Servises_Abstraction.Exceptions
{
    public class ImageValidationException : Exception
    {
        public ImageValidationSummary Summary { get; }

        public ImageValidationException(ImageValidationSummary summary)
            : base("One or more images were rejected")
        {
            Summary = summary;
        }
    }
}