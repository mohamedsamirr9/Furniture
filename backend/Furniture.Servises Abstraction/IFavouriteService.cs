using Furniture.shared.Dtos.FavouriteProductDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Servises_Abstraction
{
    public interface IFavouriteService
    {
        Task<IEnumerable<FavouriteDto>> GetFavouritesAsync(string userId);
        Task<FavouriteDto> AddToFavouritesAsync(string userId, int productId);
        Task RemoveFromFavouritesAsync(string userId, int productId);
    }
}
