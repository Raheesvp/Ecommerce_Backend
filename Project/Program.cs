// --- FIX: This is the critical namespace you were missing ---
using Application.Contracts.Repositories;
using Application.Contracts.Services;
using Application.DTOs;
using Application.DTOs.Product;
using Application.Services;
// ------------------------------------------------------------
using CloudinaryDotNet.Actions;
using Infrastructure.Persistance;
using Infrastructure.Persistance.Repository;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Persistence.Repository;

using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 2. Configure Swagger (ONLY ONCE)
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // JWT Configuration for Swagger UI
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Put **ONLY** your JWT Bearer token below",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

// 3. Configure Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });

builder.Services.AddAuthorization();

// 4. Configure Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("Infrastructure")
    )
);

// 5. Register Repositories (The "Contracts" namespace fix applies here)


builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IFileService, FileService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IWishlistRepository, WishlistRepository>();
builder.Services.AddScoped<IWishlistService, WishlistService>();

builder.Services.AddScoped<ICartRepository, CartRepository>();

builder.Services.AddScoped<ICartService, CartService>();

builder.Services.AddScoped<IUserService, UserService>();


//builder.Services.AddScoped<IWishlistRepository, WishlistRepository>();
// builder.Services.AddScoped<ICartRepository, CartRepository>(); // Uncomment when needed

// 6. Register Services
//builder.Services.AddScoped<ImageStorageService, CloudinaryService>();

// 7. Register UseCases (Uncomment these ONLY if your Controllers use them)
// builder.Services.AddScoped<LoginUseCase>();
// builder.Services.AddScoped<SignupUseCase>();
// builder.Services.AddScoped<GetAllProductsUseCase>();

// ----------------------------------------------------------------------

var app = builder.Build();



using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        // This line runs the logic we just wrote
        await DbInitializer.SeedAsync(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error seeding DB: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();