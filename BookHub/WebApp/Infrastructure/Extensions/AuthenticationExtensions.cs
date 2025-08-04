using System.Text;
using App.Domain.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using AppDbContext = App.DAL.EF.AppDbContext;

namespace WebApp.Infrastructure.Extensions
{
    /// <summary>
    /// Extensions to configure authentication and authorization
    /// </summary>
    public static class AuthenticationExtensions
    {
        /// <summary>
        /// Adds ASP.net Core Identity with cookie and JWT BEARER in one place
        /// </summary>
        /// <param name="services"></param>f
        /// <param name="config"></param>
        /// <returns></returns>
        public static IServiceCollection AddAppAuthentication(this IServiceCollection services, IConfiguration config)
        {
            // 1. Identity settings (cookies)
            services.AddIdentity<AppUser, AppRole>(opts =>
                {
                    opts.SignIn.RequireConfirmedAccount = false;
                })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultUI()
                .AddDefaultTokenProviders();

            // 2. We set up exactly the same Cook created by AddIdentity
            services.ConfigureApplicationCookie(cfg =>
            {
                cfg.Cookie.Name         = ".AspNetCore.Identity.Application";
                cfg.Cookie.HttpOnly     = true;
                cfg.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                cfg.Cookie.SameSite     = SameSiteMode.Lax;
            });
            
            // 3. Reading JWT settings from configuration
            var key      = config["JWT:key"]!;
            var issuer   = config["JWT:issuer"];
            var audience = config["JWT:audience"];
            
            // 4. Configuration of JWT-Bearer
            services.AddAuthentication()  
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.RequireHttpsMetadata = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer           = true,
                        ValidateAudience         = true,
                        ValidateLifetime         = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer              = issuer,
                        ValidAudience            = audience,
                        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                        ClockSkew                = TimeSpan.Zero
                    };
                });

            return services;
        }
    }
}