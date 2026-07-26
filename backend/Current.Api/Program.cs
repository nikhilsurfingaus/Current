using Current.Api.Configuration;
using Current.Api.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.AddSerilogLogging();

    // Services
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerDocumentation();
    builder.Services.AddFrontendCors(builder.Configuration, builder.Environment);
    builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);
builder.Services.AddEmailServices(builder.Configuration, builder.Environment);
    builder.Services.AddHealthMonitoring();
    builder.Services.AddProductionMiddleware();

    var app = builder.Build();

    if (!app.Environment.IsEnvironment("Testing"))
    {
        var emailOptions = app.Configuration.GetSection(EmailOptions.SectionName).Get<EmailOptions>();
        var smtpConfigured = emailOptions is not null &&
                             emailOptions.Enabled &&
                             !string.IsNullOrWhiteSpace(emailOptions.SmtpHost);

        if (smtpConfigured)
        {
            Log.Information(
                "Email verification: SMTP enabled (from {FromAddress} via {SmtpHost})",
                emailOptions!.FromAddress,
                emailOptions.SmtpHost);
        }
        else
        {
            Log.Warning(
                "Email verification: SMTP not configured — codes are logged only. Set Email__SmtpHost and Email__SmtpPassword on Render.");
        }
    }

    app.UseProductionMiddleware();
    app.UseSerilogRequestLogging();

    // Middleware
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    if (!app.Environment.IsEnvironment("Testing"))
    {
        app.UseHttpsRedirection();
    }

    app.UseCors(CorsExtensions.FrontendPolicy);
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthMonitoring();

    await app.ApplyMigrationsAsync();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;
