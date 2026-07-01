using Current.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// --- Register services (DI container) ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplicationServices(builder.Configuration); // DbContext, UserService, AccountService

var app = builder.Build();

// --- HTTP pipeline (order matters) ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers(); // wires up UsersController, AccountsController, etc.

app.Run();
