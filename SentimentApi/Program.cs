using SentimentApi.Data;
using Microsoft.EntityFrameworkCore;
using SentimentApi.Services;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Configurar Kestrel para escuchar en todas las interfaces y puerto configurable
var port = Environment.GetEnvironmentVariable("PORT") ?? "5019";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Connection string
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION")
                       ?? builder.Configuration.GetConnectionString("Default")
                       ?? throw new InvalidOperationException("Database connection string not configured.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Gemini API key
var geminiApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                   ?? builder.Configuration["Gemini:ApiKey"]
                   ?? throw new InvalidOperationException("Gemini API key is not configured.");

builder.Services.AddScoped<AiService>(sp => new AiService(geminiApiKey, sp.GetRequiredService<AppDbContext>()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
