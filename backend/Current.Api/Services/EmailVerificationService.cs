using System.Security.Cryptography;
using Current.Api.Common.Constants;
using Current.Api.Common.Enums;
using Current.Api.Common.Exceptions;
using Current.Api.Data;
using Current.Api.DTOs.Auth;
using Current.Api.Entities;
using Current.Api.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Current.Api.Services;

public class EmailVerificationService : IEmailVerificationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmailVerificationService> _logger;

    public EmailVerificationService(
        ApplicationDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        IEmailSender emailSender,
        ILogger<EmailVerificationService> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<RegisterResponse> BeginRegistrationAsync(RegisterRequest request)
    {
        if (request.Password.Length < 8)
        {
            throw new InvalidOperationException("Password must be at least 8 characters.");
        }

        var userEmailNormalized = request.Email.Trim().ToLowerInvariant();
        var existingUser = await _dbContext.Users
            .FirstOrDefaultAsync(user => user.Email == userEmailNormalized);

        if (existingUser is not null && existingUser.IsEmailVerified)
        {
            throw new DuplicateEmailException();
        }

        var utcNow = DateTime.UtcNow;
        User userToVerify;

        if (existingUser is null)
        {
            userToVerify = new User
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = userEmailNormalized,
                Role = UserRole.User,
                IsEmailVerified = false,
                CreatedAt = utcNow,
                UpdatedAt = utcNow,
            };

            userToVerify.PasswordHash = _passwordHasher.HashPassword(userToVerify, request.Password);
            _dbContext.Users.Add(userToVerify);
        }
        else
        {
            userToVerify = existingUser;
            userToVerify.FirstName = request.FirstName.Trim();
            userToVerify.LastName = request.LastName.Trim();
            userToVerify.PasswordHash = _passwordHasher.HashPassword(userToVerify, request.Password);
            userToVerify.UpdatedAt = utcNow;
        }

        var verificationExpiresAt = await IssueVerificationCodeAsync(userToVerify, utcNow);

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Verification email requested for {Email}", userEmailNormalized);

        return new RegisterResponse
        {
            Email = userEmailNormalized,
            Message = "Check your email for a verification code.",
            VerificationExpiresAt = verificationExpiresAt,
        };
    }

    public async Task<User> VerifyEmailAsync(VerifyEmailRequest request)
    {
        var userEmailNormalized = request.Email.Trim().ToLowerInvariant();
        var normalizedCode = request.Code.Trim();

        if (normalizedCode.Length != EmailVerificationConstants.CodeLength ||
            !normalizedCode.All(char.IsDigit))
        {
            throw new InvalidVerificationCodeException();
        }

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(item => item.Email == userEmailNormalized);

        if (user is null || user.IsEmailVerified)
        {
            throw new InvalidVerificationCodeException();
        }

        if (user.EmailVerificationExpiresAt is null ||
            user.EmailVerificationExpiresAt <= DateTime.UtcNow ||
            string.IsNullOrWhiteSpace(user.EmailVerificationCodeHash))
        {
            throw new InvalidVerificationCodeException();
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.EmailVerificationCodeHash,
            normalizedCode);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            throw new InvalidVerificationCodeException();
        }

        user.IsEmailVerified = true;
        user.EmailVerificationCodeHash = null;
        user.EmailVerificationExpiresAt = null;
        user.EmailVerificationSentAt = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Email verified for user {UserId}", user.Id);

        return user;
    }

    public async Task<RegisterResponse> ResendVerificationAsync(ResendVerificationRequest request)
    {
        var userEmailNormalized = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(item => item.Email == userEmailNormalized);

        if (user is null || user.IsEmailVerified)
        {
            return new RegisterResponse
            {
                Email = userEmailNormalized,
                Message = "If an account exists, a verification code has been sent.",
                VerificationExpiresAt = DateTime.UtcNow.AddMinutes(EmailVerificationConstants.ExpiryMinutes),
            };
        }

        var utcNow = DateTime.UtcNow;
        if (user.EmailVerificationSentAt.HasValue &&
            utcNow < user.EmailVerificationSentAt.Value.AddSeconds(EmailVerificationConstants.ResendCooldownSeconds))
        {
            throw new InvalidOperationException("Please wait before requesting another verification code.");
        }

        var verificationExpiresAt = await IssueVerificationCodeAsync(user, utcNow);
        user.UpdatedAt = utcNow;

        await _dbContext.SaveChangesAsync();

        return new RegisterResponse
        {
            Email = userEmailNormalized,
            Message = "If an account exists, a verification code has been sent.",
            VerificationExpiresAt = verificationExpiresAt,
        };
    }

    private async Task<DateTime> IssueVerificationCodeAsync(User user, DateTime utcNow)
    {
        var verificationCode = GenerateVerificationCode();
        var verificationExpiresAt = utcNow.AddMinutes(EmailVerificationConstants.ExpiryMinutes);

        user.EmailVerificationCodeHash = _passwordHasher.HashPassword(user, verificationCode);
        user.EmailVerificationExpiresAt = verificationExpiresAt;
        user.EmailVerificationSentAt = utcNow;
        user.IsEmailVerified = false;

        await _emailSender.SendVerificationCodeAsync(user.Email, verificationCode, verificationExpiresAt);

        return verificationExpiresAt;
    }

    private static string GenerateVerificationCode()
    {
        var codeValue = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return codeValue.ToString($"D{EmailVerificationConstants.CodeLength}");
    }
}
