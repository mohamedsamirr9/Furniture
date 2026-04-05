using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Servises_Abstraction
{
    public interface IProductImageService 
    {
        Task AddImageAsync(int productId, string imageUrl);
        Task DeleteImageAsync(int imageId);
    }
}
