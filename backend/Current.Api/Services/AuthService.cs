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
    private readonly IEmailVerificationService _emailVerificationService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ApplicationDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        IConfiguration configuration,
        INotificationService notificationService,
        IEmailVerificationService emailVerificationService,
        ILogger<AuthService> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _notificationService = notificationService;
        _emailVerificationService = emailVerificationService;
        _logger = logger;
    }

    public Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        return _emailVerificationService.BeginRegistrationAsync(request);
    }

    public async Task<AuthResponse> VerifyEmailAsync(VerifyEmailRequest request)
    {
        var verifiedUser = await _emailVerificationService.VerifyEmailAsync(request);

        await _notificationService.TryCreateNotificationAsync(
            verifiedUser.Id,
            NotificationType.Security,
            "Welcome to Current",
            "Your account is ready. Create your first account to get started.");

        _logger.LogInformation(
            "User registered {UserId} with email {Email}",
            verifiedUser.Id,
            verifiedUser.Email);

        return BuildAuthResponse(verifiedUser);
    }

    public Task<RegisterResponse> ResendVerificationAsync(ResendVerificationRequest request)
    {
        return _emailVerificationService.ResendVerificationAsync(request);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var userEmailNormalized = request.Email.Trim().ToLowerInvariant();

        var userByEmail = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Email == userEmailNormalized);

        if (userByEmail is null)
        {
            _logger.LogWarning("Authentication failed for {Email}: user not found", userEmailNormalized);
            throw new InvalidCredentialsException();
        }

        var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(
            userByEmail,
            userByEmail.PasswordHash,
            request.Password);

        if (passwordVerificationResult == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning("Authentication failed for {Email}: invalid password", userEmailNormalized);
            throw new InvalidCredentialsException();
        }

        if (!userByEmail.IsEmailVerified)
        {
            _logger.LogWarning("Authentication failed for {Email}: email not verified", userEmailNormalized);
            throw new EmailNotVerifiedException();
        }

        _logger.LogInformation(
            "User {UserId} authenticated as {Email}",
            userByEmail.Id,
            userByEmail.Email);

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
