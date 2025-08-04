using App.BLL;
using App.Contracts.BLL;
using App.Contracts.DAL;
using App.DAL.EF;
using App.Domain.Identity;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using WebApp.Infrastructure.Data;
using WebApp.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Connection String
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// 2. EF Core + UnitOfWork + BLL/DAL
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(connectionString));
builder.Services.AddScoped<IAppUnitOfWork, AppUOW>();
builder.Services.AddScoped<IAppBLL, AppBLL>();

// 3. Identity
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services
    .AddIdentity<AppUser, AppRole>(opts => opts.SignIn.RequireConfirmedAccount = false)
    .AddDefaultUI()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// 4. Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opts =>
{
    opts.IdleTimeout   = TimeSpan.FromMinutes(20);
    opts.Cookie.HttpOnly = true;
    opts.Cookie.IsEssential = true;
});

// 5. CORS
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() 
    ?? Array.Empty<string>();

builder.Services.AddCors(opts =>
    opts.AddPolicy("CorsPolicy", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    })
);

// 6. JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// 7. AutoMapper + API Versioning + Swagger
builder.Services.AddMappingProfiles();
builder.Services.AddVersioningAndSwagger();

// 8. MVC / Controllers / RazorPages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddHealthChecks();

var app = builder.Build();

// 9. Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();

app.UseHealthChecks("/health");

app.UseRouting();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(opts =>
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    foreach (var desc in provider.ApiVersionDescriptions)
    {
        opts.SwaggerEndpoint(
            $"/swagger/{desc.GroupName}/swagger.json",
            desc.GroupName.ToUpperInvariant()
        );
    }
});

app.MapControllerRoute(
    name: "area",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);
app.MapRazorPages();

// 10. Seeding
app.Seed();

app.Run();

public partial class Program { }
