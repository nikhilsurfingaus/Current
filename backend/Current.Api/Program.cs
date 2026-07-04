using Current.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddFrontendCors(builder.Environment);
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(CorsExtensions.FrontendPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
