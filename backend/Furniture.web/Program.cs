
using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Persistence.Data.DataSeed;
using Furniture.Persistence.Data.DbContexts;
using Furniture.Persistence.Repositories;
using Furniture.Services;
using Furniture.Services.Mapping;
using Furniture.Servises_Abstraction;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Furniture.Services.Implementations;
using Furniture.Services.Mappings;

namespace Furniture.web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            // builder.Services.AddOpenApi();
            builder.Services.AddDbContext<FurnitureDbContext>(Options =>
            {
                Options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
             .AddEntityFrameworkStores<FurnitureDbContext>()
             .AddDefaultTokenProviders();


            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    });
            });
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddAutoMapper(x =>
            {
                x.AddProfile<MappingCategory>();
                x.AddProfile<MappingReview>();
            });

            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IDataSeeding, DataSeeding>();
            builder.Services.AddScoped<IReviewService, ReviewService>();
            builder.Services.AddAutoMapper(x => x.AddProfile<MappingCategory>());
            builder.Services.AddAutoMapper(x => x.AddProfile<MappingOffer>());
            builder.Services.AddAutoMapper(x => x.AddProfile<MappingCart>());
            builder.Services.AddAutoMapper(x => x.AddProfile<MappingCustomRequest>());

            builder.Services.AddAutoMapper(x => x.AddProfile<MappingOrder>());
            
  
            builder.Services.AddAutoMapper(x => x.AddProfile<MappingProduct>());
            builder.Services.AddAutoMapper(x => x.AddProfile<MappingFavourite>());
            builder.Services.AddAutoMapper(x => x.AddProfile<ShippingMapping>());

            builder.Services.AddAutoMapper(x => x.AddProfile<MappingProduct>());
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IProductImageService, ProductImageService>();
            builder.Services.AddScoped<IFavouriteService, FavouriteService>();
            builder.Services.AddScoped<IOfferService, OfferService>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<ICustomRequestService, CustomRequestService>();
            builder.Services.AddScoped<IShippingService, ShippingService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            
            var app = builder.Build();

            // Data Seeding
            await using var scope= app.Services.CreateAsyncScope();
            var DataSeedingService = scope.ServiceProvider.GetRequiredService<IDataSeeding>();
           await DataSeedingService.InitializeAsync();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Furniture API v1");
                });
                //  app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");
            app.UseAuthorization();


            app.MapControllers();

            app.Run();

        }
    }
}
