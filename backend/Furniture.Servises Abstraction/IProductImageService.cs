using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Servises_Abstraction
{
    public interface IProductImageService 
    {
        Task AddImageAsync(int productId, Stream imageStream, string fileName, string contentType);
        Task DeleteImageAsync(int imageId);
    }
}
