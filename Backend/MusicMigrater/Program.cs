using FluentValidation;
using Hangfire;
using Hangfire.Storage.SQLite;
using MusicMigrater.BLL.DI;
using MusicMigrater.BLL.Handlers;
using MusicMigrater.BLL.Options;
using MusicMigrater.DAL.DI;
using MusicMigrater.Endpoints;
using MusicMigrater.Maping;
using MusicMigrater.Middlewares;
using MusicMigrater.Options;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

MapsterConfigApi.Configure();

var corsSettings = builder.Configuration.GetRequiredSection(CorsOptions.SectionName);
builder.Services.Configure<CorsOptions>(corsSettings);
var corsOptions = corsSettings.Get<CorsOptions>()
    ?? throw new InvalidOperationException("CORS options are missing in appSettings");

var spotifyOptions = builder.Configuration.GetRequiredSection(SpotifyOptions.SectionName);
builder.Services.Configure<SpotifyOptions>(spotifyOptions);

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddTransient<SpotifyAuthHandler>();

builder.Services.RegisterBllLayer(builder.Configuration);
builder.Services.RegisterDalLayer(builder.Configuration);

builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsOptions.Origin)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddOpenApi();

var hangfireConnectionString = builder.Configuration.GetConnectionString("HangfireConnection");

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSQLiteStorage(hangfireConnectionString, new SQLiteStorageOptions
    {
        InvisibilityTimeout = TimeSpan.FromDays(5),
    }));

builder.Services.AddHangfireServer(opt =>
{
    // SQLite is not optimized for multiple
    // writes at the same time. Increase/delete if migrating
    // to another DB
    opt.WorkerCount = 1;
});

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseHangfireDashboard();
}

app.UseHttpsRedirection();
app.UseCors();

app.MapMusicEndpoints();
app.MapUserSettingsEndpoints();
app.MapSyncEndpoints();

app.Run();
