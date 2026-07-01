using Current.Api.Data;
using Current.Api.Interfaces;
using Current.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Current.Api.Extensions;

// Keeps Program.cs clean — all DI registrations live here
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // Scoped = one DbContext per HTTP request (like a FastAPI Depends session)
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Interface → implementation mapping for dependency injection
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAccountService, AccountService>();

        return services;
    }
}
