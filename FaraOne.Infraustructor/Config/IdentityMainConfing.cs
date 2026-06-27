using FaraOne.Application.Context;
using FaraOne.Domain.User;
using FaraOne.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;     
namespace FaraOne.Infraustructor;

public static class IdentityMainConfing
{
    public static IServiceCollection AddIdnetityMain(this IServiceCollection services, IConfiguration configuration)
    {
        var ConString = configuration.GetConnectionString("sqlServer");

        services.AddScoped<IDatabaseContext, DataContext>();
        // ثبت DbContext با تنظیمات کامل
        services.AddDbContext<DataContext>(options =>
        {
            options.UseSqlServer(ConString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly("FaraOne.Persistence"); // اسم اسمبلی Migration
                sqlOptions.EnableRetryOnFailure();
            });
            options.EnableSensitiveDataLogging(); // برای دیباگ (در توسعه)
        }, ServiceLifetime.Scoped); // مشخص کردن Lifetime


        services.Configure<IdentityOptions>(s =>
        {
            s.Lockout.MaxFailedAccessAttempts = 5;
            s.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
            s.Password.RequireDigit = true;
            s.Password.RequireNonAlphanumeric = false;
            s.Password.RequiredLength = 8;
        });
        return services;
    }
}