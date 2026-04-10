
using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Persistence.Data.DataSeed;
using Furniture.Persistence.Data.DbContexts;
using Furniture.Persistence.Repositories;
using Furniture.Services;
using Furniture.Services.Implementations;
using Furniture.Services.Mapping;
using Furniture.Services.Mappings;
using Furniture.Servises_Abstraction;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;

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

            //JWT
            var jwt = builder.Configuration.GetSection("Jwt");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwt["Issuer"],
                    ValidAudience = jwt["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwt["Key"]))
                };
            });
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("VerifiedUser", policy =>
                    policy.RequireClaim("IsVerified", "True"));

                options.AddPolicy("SellerOnly", policy =>
                    policy.RequireRole("Seller"));
            });
            // swagger authorization
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Enter: Bearer YOUR_TOKEN"
                });

                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
            });


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
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<JwtHelper>();
            builder.Services.AddAutoMapper(x => x.AddProfile<MappingOrder>());
            
  
  builder.Services.AddAutoMapper(x => x.AddProfile<MappingProduct>());
            builder.Services.AddAutoMapper(x => x.AddProfile<MappingFavourite>());
            builder.Services.AddAutoMapper(x => x.AddProfile<MappingUser>());


            builder.Services.AddAutoMapper(x => x.AddProfile<MappingProduct>());
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IProductImageService, ProductImageService>();
            builder.Services.AddScoped<IFavouriteService, FavouriteService>();
            builder.Services.AddScoped<IOfferService, OfferService>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<ICustomRequestService, CustomRequestService>();
            
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
            app.UseAuthentication();
            app.UseStaticFiles();

            app.MapControllers();

            app.Run();

        }
    }
}
