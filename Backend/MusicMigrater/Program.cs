using FluentValidation;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using MusicMigrater.BLL.DI;
using MusicMigrater.BLL.Handlers;
using MusicMigrater.BLL.Options;
using MusicMigrater.DAL.Context;
using MusicMigrater.DAL.DI;
using MusicMigrater.Domain.Logging;
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

var hangfireConnectionString = builder.Configuration.GetConnectionString("AzureDefaultConnection");

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(hangfireConnectionString, new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

builder.Services.AddHangfireServer(opt =>
{
    opt.WorkerCount = 1;
});

var app = builder.Build();

// auto-apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogStartApplyingMigrations();

        var context = services.GetRequiredService<AppDbContext>();

        context.Database.Migrate();

        logger.LogFinishApplyingMigrations();
    }
    catch (Exception ex)
    {
        logger.LogFailedToApplyMigrationsOnStartup(ex);
        throw;
    }
}

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard();
}

app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();
app.UseCors();

app.MapMusicEndpoints();
app.MapUserSettingsEndpoints();
app.MapSyncEndpoints();

app.Run();
