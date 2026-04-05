using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Persistence.Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Furniture.Persistence.Data.DataSeed
{
    public class DataSeeding : IDataSeeding
    {
        private readonly FurnitureDbContext _dbContext;

        public DataSeeding(FurnitureDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task InitializeAsync()
        {
            try
            {
                var HasCategories =await _dbContext.Categories.AnyAsync();
                if (HasCategories) return;
                if (!HasCategories)
                {
                    await SeedDataFromJsonAsync<Category>("Category.json", _dbContext.Categories);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex) { Console.WriteLine($"Data Seeding Failed: {ex}"); }

        }

        private async Task SeedDataFromJsonAsync<T>(string fileName, DbSet<T> dbset) where T : class
        {
            var filePath = @"..\Furniture.Persistence\Data\DataSeed\JsonFiles\"+fileName;
            if (!File.Exists(filePath)) throw new FileNotFoundException();
            try
            {
                using var dataStream = File.OpenRead(filePath);
                var data =await JsonSerializer.DeserializeAsync<List<T>>(dataStream, new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true,
                });
                if(data is not null)
                {
                   await dbset.AddRangeAsync(data);
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error while reading json: {ex}");
                return;
            }
        }
    }
}
