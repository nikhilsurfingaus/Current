using Current.Api.Data;
using Current.Api.Entities;
using Current.Api.Interfaces;
using Current.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Current.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        return services;
    }
}
