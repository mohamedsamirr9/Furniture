using System.Text;
using System.Security.Cryptography;
using Furniture.Services;
using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Persistence.Data.DataSeed;
using Furniture.Persistence.Data.DbContexts;
using Furniture.Persistence.Repositories;
using Furniture.Services.Implementations;
using Furniture.Services.Mapping;
using Furniture.Services.Mappings;
using Furniture.Servises_Abstraction;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Furniture.web.Hubs;

namespace Furniture.web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Basic
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Database
            builder.Services.AddDbContext<FurnitureDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<FurnitureDbContext>()
                .AddDefaultTokenProviders();

            // JWT
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
                        Encoding.UTF8.GetBytes(jwt["Key"] ?? string.Empty))
                };

                o.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments("/api/chatHub") ||
                             path.StartsWithSegments("/api/notificationHub")))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            // Authorization
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("VerifiedUser", policy =>
                    policy.RequireClaim("IsVerified", "True"));
                options.AddPolicy("SellerOnly", policy =>
                    policy.RequireRole("Seller"));
            });

            // Swagger
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
                        Array.Empty<string>()
                    }
                });
            });

            // CORS
            var allowedOrigins = builder.Configuration
                .GetSection("AllowedOrigins")
                .Get<string[]>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.WithOrigins(allowedOrigins!)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials()
                          .WithExposedHeaders("access_token");
                });
            });

            // SignalR
            builder.Services.AddSignalR();

            // AutoMapper 
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<MappingCategory>();
                cfg.AddProfile<MappingReview>();
                cfg.AddProfile<MappingOffer>();
                cfg.AddProfile<MappingCart>();
                cfg.AddProfile<MappingCustomRequest>();
                cfg.AddProfile<MappingOrder>();
                cfg.AddProfile<MappingProduct>();
                cfg.AddProfile<MappingComplaint>();
                cfg.AddProfile<MappingFavourite>();
                cfg.AddProfile<MappingUser>();
                cfg.AddProfile<ShippingMapping>();
                cfg.AddProfile<MappingChat>();
            });

            // Http Clients
            builder.Services.AddHttpClient("AIService", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["AIRecommendation:BaseUrl"]!);
                client.Timeout = TimeSpan.FromSeconds(60);
            });
            builder.Services.AddHttpClient("ImageValidationService", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["ImageValidation:BaseUrl"]!);
                client.Timeout = TimeSpan.FromSeconds(30);
            });
            builder.Services.AddHttpClient("VisualSearchService", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["VisualSearch:BaseUrl"]!);
                client.Timeout = TimeSpan.FromSeconds(30);
            });
            builder.Services.AddHttpClient("Paymob", client =>
            {
                client.BaseAddress = new Uri("https://accept.paymob.com/");
                client.Timeout = TimeSpan.FromSeconds(30);
            });
            builder.Services.AddHttpClient("PaymobPayouts", client => client.Timeout = TimeSpan.FromSeconds(30));
            builder.Services.AddHttpClient("PaymobPayoutsAuth", client => client.Timeout = TimeSpan.FromSeconds(30));
            builder.Services.AddHttpClient("PythonService");

            builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.SectionName));
            builder.Services.AddHttpClient("Resend", client =>
            {
                client.BaseAddress = new Uri("https://api.resend.com/");
                var timeoutSec = builder.Configuration.GetValue<int?>("Email:TimeoutSeconds") ?? 20;
                client.Timeout = TimeSpan.FromSeconds(timeoutSec + 5);
            });

            // Repositories & Services
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<JwtHelper>();
            builder.Services.AddScoped<IDataSeeding, DataSeeding>();
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<ISellerRequestService, SellerRequestService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IProductImageService, ProductImageService>();
            builder.Services.AddScoped<IFavouriteService, FavouriteService>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<IOfferService, OfferService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<IReviewService, ReviewService>();
            builder.Services.AddScoped<IComplaintService, ComplaintService>();
            builder.Services.AddScoped<ICustomRequestService, CustomRequestService>();
            builder.Services.AddScoped<IShippingService, ShippingService>();
            builder.Services.AddScoped<IShippingCalculatorService, ShippingCalculatorService>();
            builder.Services.AddScoped<ISellerService, SellerService>();
            builder.Services.AddScoped<ISellerPaymentService, SellerPaymentService>();
            builder.Services.AddScoped<IImageValidationService, ImageValidationService>();
            builder.Services.AddScoped<IChatService, ChatService>();
            builder.Services.AddScoped<IRecommendationService, RecommendationService>();
            builder.Services.AddScoped<ISearchService, SearchService>();
            builder.Services.AddHostedService<SearchIndexingBackgroundService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IHubNotificationClient, HubNotificationClient>();
            
            
            var app = builder.Build();

            // Data Seeding
            await using (var scope = app.Services.CreateAsyncScope())
            {
                var dataSeedingService = scope.ServiceProvider.GetRequiredService<IDataSeeding>();
                await dataSeedingService.InitializeAsync();
            }

            // Middleware
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Furniture API v1"));
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseStaticFiles();

            app.MapControllers();
            app.MapHub<ChatHub>("/api/chatHub");
            app.MapHub<NotificationHub>("/api/notificationHub");
            app.Run();
        }
    }
}