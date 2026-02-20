using Application.Contracts.Repositories;
using Application.Contracts.Services;
using Application.Services;
using DinkToPdf;
using DinkToPdf.Contracts;
using Domain.Entities;
using Infrastructure.Hubs;
using Infrastructure.Persistance;
using Infrastructure.Persistance.Repository;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Persistence.Repository;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Project.WebAPI;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// -------------------- Swagger --------------------
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Atleticx-Api",
        Version = "v1"
    });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT Bearer token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
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

// -------------------- JWT --------------------
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new Exception("JWT:Key is missing in configuration");

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        RoleClaimType = ClaimTypes.Role
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/orderHub"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// -------------------- CORS --------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://wolfathleticx.duckdns.org")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
builder.Services.AddHealthChecks();
// -------------------- Controllers --------------------
builder.Services.AddControllers()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(
        new JsonStringEnumConverter()
    );
});



// -------------------- Http Client --------------------
builder.Services.AddHttpClient();

// -------------------- Repositories --------------------
builder.Services.AddScoped(typeof(IGenericeRepository<>), typeof(GenericeRepository<>));
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IWishlistRepository, WishlistRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();

// -------------------- Services --------------------
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IFileService, CloudinaryService>();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<IEmailSender, EmailService>();

// -------------------- SignalR --------------------
builder.Services.AddSignalR();

// -------------------- PDF Converter --------------------
builder.Services.AddSingleton(typeof(IConverter),
    new SynchronizedConverter(new PdfTools()));

// -------------------- Database --------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null
        )
    )
);

// -------------------- SAFE NATIVE DLL LOADING --------------------
// IMPORTANT FIX: Load wkhtmltopdf ONLY on Windows
if (OperatingSystem.IsWindows())
{
    var context = new CustomAssemblyLoadContext();
    string architecture = IntPtr.Size == 8 ? "x64" : "x86";

    string dllPath = Path.Combine(
        Directory.GetCurrentDirectory(),
        "runtimes",
        $"win-{architecture}",
        "native",
        "libwkhtmltox.dll"
    );

    if (File.Exists(dllPath))
    {
        context.LoadUnmanagedLibrary(dllPath);
    }
}

// -------------------- Build App --------------------
var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//{
//    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//    await DbInitializer.SeedAsync(context);
//}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (exception != null)
        {
            // This prints the FULL stack trace to console
            Console.WriteLine("=== FULL ERROR ===");
            Console.WriteLine(exception.Error.Message);
            Console.WriteLine(exception.Error.StackTrace);
            Console.WriteLine(exception.Error.InnerException?.Message);
            Console.WriteLine(exception.Error.InnerException?.StackTrace);
            Console.WriteLine("==================");
        }
    });
});





// -------------------- Swagger Middleware --------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Atleticx-Api v1");
        c.RoutePrefix = "swagger";
    });
}



// -------------------- Middleware Pipeline --------------------

var forwardHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};

forwardHeadersOptions.KnownNetworks.Clear();
forwardHeadersOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardHeadersOptions);



app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseMiddleware<Middlewares>();
app.UseAuthentication();
app.UseMiddleware<UserBlockMiddleware>();
app.UseAuthorization();

// -------------------- Endpoints --------------------
app.MapHub<NotificationHub>("/orderHub");
app.MapControllers();
app.MapGet("/api/health", () => Results.Ok("Healthy"));




// -------------------- Run --------------------
app.Run();
