using Asp.Versioning;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApp.Infrastructure.Extensions
{
    /// <summary>
    /// Extensions for API
    /// </summary>
    public static class ApiExtensions
    {
        /// <summary>
        /// API Versioning and Swagger setup
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddVersioningAndSwagger(this IServiceCollection services)
        {
            // API Versioning
            var apiVersionBuilder = services.AddApiVersioning(options =>
            {
                options.ReportApiVersions      = true;
                options.DefaultApiVersion      = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
            });
            apiVersionBuilder.AddApiExplorer(opts =>
            {
                opts.GroupNameFormat            = "'v'VVV";
                opts.SubstituteApiVersionInUrl  = true;
            });

            // Swagger
            services.AddEndpointsApiExplorer();
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen();

            return services;
        }

        /// <summary>
        /// Map profiles through App BLL -> EF
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddMappingProfiles(this IServiceCollection services)
        {
            services.AddAutoMapper(
                typeof(App.DAL.EF.AutoMapperProfile),
                typeof(App.BLL.AutoMapperProfile),
                typeof(Helpers.AutoMapperProfile)
            );
            return services;
        }
    }
}