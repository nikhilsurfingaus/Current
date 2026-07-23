using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Current.Api.Common.Enums;
using Current.Api.Common.Exceptions;
using Current.Api.Data;
using Current.Api.DTOs.Auth;
using Current.Api.Entities;
using Current.Api.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Current.Api.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly INotificationService _notificationService;

    public AuthService(
        ApplicationDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        IConfiguration configuration,
        INotificationService notificationService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _notificationService = notificationService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (request.Password.Length < 8)
        {
            throw new InvalidOperationException("Password must be at least 8 characters.");
        }

        var userEmailNormalized = request.Email.Trim().ToLowerInvariant();

        var userEmailExists = await _dbContext.Users
            .AnyAsync(user => user.Email == userEmailNormalized);

        if (userEmailExists)
        {
            throw new DuplicateEmailException();
        }

        var utcNow = DateTime.UtcNow;
        var userToCreate = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = userEmailNormalized,
            Role = UserRole.User,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        userToCreate.PasswordHash = _passwordHasher.HashPassword(userToCreate, request.Password);

        _dbContext.Users.Add(userToCreate);
        await _dbContext.SaveChangesAsync();

        await _notificationService.TryCreateNotificationAsync(
            userToCreate.Id,
            NotificationType.Security,
            "Welcome to Current",
            "Your account is ready. Create your first account to get started.");

        return BuildAuthResponse(userToCreate);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var userEmailNormalized = request.Email.Trim().ToLowerInvariant();

        var userByEmail = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Email == userEmailNormalized);

        if (userByEmail is null)
        {
            throw new InvalidCredentialsException();
        }

        var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(
            userByEmail,
            userByEmail.PasswordHash,
            request.Password);

        if (passwordVerificationResult == PasswordVerificationResult.Failed)
        {
            throw new InvalidCredentialsException();
        }

        return BuildAuthResponse(userByEmail);
    }

    private AuthResponse BuildAuthResponse(User authUser)
    {
        var jwtIssuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
        var jwtAudience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience is not configured.");
        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured.");

        var jwtExpiryMinutesConfigured = _configuration["Jwt:ExpiryMinutes"];
        var jwtExpiryMinutes = int.TryParse(jwtExpiryMinutesConfigured, out var parsedMinutes)
            ? parsedMinutes
            : 60;

        var tokenExpiresAt = DateTime.UtcNow.AddMinutes(jwtExpiryMinutes);
        var tokenSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var tokenCredentials = new SigningCredentials(tokenSecurityKey, SecurityAlgorithms.HmacSha256);

        var tokenClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, authUser.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, authUser.Email),
            new(ClaimTypes.Role, authUser.Role.ToString()),
            new(ClaimTypes.NameIdentifier, authUser.Id.ToString())
        };

        var jwtToken = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: tokenClaims,
            expires: tokenExpiresAt,
            signingCredentials: tokenCredentials);

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenValue = tokenHandler.WriteToken(jwtToken);

        return new AuthResponse
        {
            UserId = authUser.Id,
            Email = authUser.Email,
            Role = authUser.Role,
            Token = tokenValue,
            ExpiresAt = tokenExpiresAt
        };
    }
}
