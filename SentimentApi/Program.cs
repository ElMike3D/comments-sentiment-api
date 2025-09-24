using SentimentApi.Data;
using Microsoft.EntityFrameworkCore;
using SentimentApi.Services;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

var geminiApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                   ?? builder.Configuration["Gemini:ApiKey"];

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
