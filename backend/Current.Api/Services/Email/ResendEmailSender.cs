using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Current.Api.Configuration;
using Current.Api.Interfaces;
using Microsoft.Extensions.Options;

namespace Current.Api.Services.Email;

public class ResendEmailSender : IEmailSender
{
    private readonly HttpClient _httpClient;
    private readonly EmailOptions _emailOptions;
    private readonly ILogger<ResendEmailSender> _logger;

    public ResendEmailSender(
        HttpClient httpClient,
        IOptions<EmailOptions> emailOptions,
        ILogger<ResendEmailSender> logger)
    {
        _httpClient = httpClient;
        _emailOptions = emailOptions.Value;
        _logger = logger;
    }

    public async Task SendVerificationCodeAsync(string recipientEmail, string verificationCode, DateTime expiresAtUtc)
    {
        var apiKey = _emailOptions.ResolveResendApiKey();

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("Resend API key is not configured.");
        }

        var fromAddress = BuildFromAddress();
        var emailBody = $"""
            Your Current verification code is: {verificationCode}

            This code expires at {expiresAtUtc:u}.

            If you did not create an account, you can ignore this email.
            """;

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(new ResendSendEmailRequest
        {
            From = fromAddress,
            To = [recipientEmail],
            Subject = "Verify your Current account",
            Text = emailBody,
        });

        HttpResponseMessage response;

        try
        {
            response = await _httpClient.SendAsync(request);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to reach Resend API for {Email}", recipientEmail);
            throw new InvalidOperationException("Unable to send verification email. Please try again later.");
        }

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        _logger.LogError(
            "Resend API returned {StatusCode} for {Email}: {ResponseBody}",
            (int)response.StatusCode,
            recipientEmail,
            responseBody);

        throw new InvalidOperationException("Unable to send verification email. Please try again later.");
    }

    private string BuildFromAddress()
    {
        if (string.IsNullOrWhiteSpace(_emailOptions.FromName))
        {
            return _emailOptions.FromAddress;
        }

        return $"{_emailOptions.FromName} <{_emailOptions.FromAddress}>";
    }

    private sealed class ResendSendEmailRequest
    {
        [JsonPropertyName("from")]
        public required string From { get; init; }

        [JsonPropertyName("to")]
        public required string[] To { get; init; }

        [JsonPropertyName("subject")]
        public required string Subject { get; init; }

        [JsonPropertyName("text")]
        public required string Text { get; init; }
    }
}
