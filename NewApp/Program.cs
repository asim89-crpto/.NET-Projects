using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NewApp.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using NewApp.Services;
using NewApp.Models;
using Microsoft.ML; // ✅ Add this for ML.NET
using Microsoft.Extensions.ML;




var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database Configuration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DevConnection")));

// ✅ Machine Learning - PredictionEnginePool Configuration
builder.Services.AddPredictionEnginePool<SentimentData, SentimentPrediction>()
    .FromFile(modelName: "SentimentAnalysisModel", filePath: "SentimentModel.zip", watchForChanges: true);

// Logging Configuration (Optional)
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Kestrel Configuration
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000); // HTTP
    options.ListenLocalhost(5001, listenOptions => listenOptions.UseHttps()); // HTTPS
});

// SentimentModel Service
builder.Services.AddSingleton<SentimentModel>();

// ✅ CORS Configuration (including localhost variants for HTTP/HTTPS)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorWebAssembly", policy =>
        policy.WithOrigins(
            "https://localhost:7146",  // Blazor WebAssembly (HTTPS)
            "http://localhost:5293" // Optional fallback (HTTP)
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
    );
});

var app = builder.Build();

// Middleware configuration
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowBlazorWebAssembly");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();