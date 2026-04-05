using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Domain.Models.Enum;
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
                if (!await _dbContext.Users.AnyAsync())
                {
                    var seller = new ApplicationUser
                    {
                        Id = "seller-1", 
                        UserName = "seller@test.com",
                        NormalizedUserName = "SELLER@TEST.COM",
                        Email = "seller@test.com",
                        NormalizedEmail = "SELLER@TEST.COM",
                        EmailConfirmed = true,
                        Name = "Main Seller",
                        Role = Roles.seller,
                        Address = "Cairo",
                        IsConfirmed = true,

                        SecurityStamp = Guid.NewGuid().ToString("D"),
                        PasswordHash = "AQAAAAEAACcQAAAAE" 
                    };

                    await _dbContext.Users.AddAsync(seller);
                    await _dbContext.SaveChangesAsync();
                }
                var HasProductsImages = await _dbContext.ProductImages.AnyAsync();
                if (HasProductsImages) return;
                if (!HasProductsImages)
                {
                    Console.WriteLine("Seeding Products started...");
                    await SeedDataFromJsonAsync<ProductImage>("ProductImages.json", _dbContext.ProductImages);
                    await _dbContext.SaveChangesAsync();
                }
                var HasProducts = await _dbContext.Products.AnyAsync();
                if (HasProducts) return;
                if (!HasProducts)
                {
                    Console.WriteLine("Seeding Products started...");
                    await SeedDataFromJsonAsync<Product>("Product.json", _dbContext.Products);
                    await _dbContext.SaveChangesAsync();
                }
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
