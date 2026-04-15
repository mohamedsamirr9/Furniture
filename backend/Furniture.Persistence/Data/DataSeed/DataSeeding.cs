using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Domain.Models.Enum;
using Furniture.Persistence.Data.DbContexts;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;

        public DataSeeding(
            FurnitureDbContext dbContext,
            UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task InitializeAsync()
        {
            try
            {
                if (!await _userManager.Users.AnyAsync())
            {
                var admin = new ApplicationUser
            {
                Id = "admin-1",
                UserName = "admin@test.com",
                Email = "admin@test.com",
                Name = "Main Admin",
                Address = "Cairo",
                Role = Roles.admin,
                EmailConfirmed = true,
                IsConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("D")
            };

            var seller = new ApplicationUser
            {
                Id = "seller-1",
                UserName = "seller@test.com",
                Email = "seller@test.com",
                Name = "Main Seller",
                Address = "Cairo",
                Role = Roles.seller,
                EmailConfirmed = true,
                IsConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("D")
            };

            var adminResult = await _userManager.CreateAsync(admin, "Admin@123");
            var sellerResult = await _userManager.CreateAsync(seller, "Seller@123");

            if (!adminResult.Succeeded || !sellerResult.Succeeded)
            {
                throw new Exception(
                    string.Join(", ",
                        adminResult.Errors.Concat(sellerResult.Errors)
                        .Select(e => e.Description))
                );
            }
            }
                var HasProductsImages = await _dbContext.ProductImages.AnyAsync();
                                var HasProducts = await _dbContext.Products.AnyAsync();
                                                var HasCategories =await _dbContext.Categories.AnyAsync();


                if (!HasCategories)
                {
                    await SeedDataFromJsonAsync<Category>("Category.json", _dbContext.Categories);
                    await _dbContext.SaveChangesAsync();
                }

                if (!HasProducts)
                {
                    Console.WriteLine("Seeding Products...");
                    await SeedDataFromJsonAsync<Product>("Product.json", _dbContext.Products);
                    await _dbContext.SaveChangesAsync();
                }

                if (!HasProductsImages)
                {
                    Console.WriteLine("Seeding Product Images...");
                    await SeedDataFromJsonAsync<ProductImage>("ProductImages.json", _dbContext.ProductImages);
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
