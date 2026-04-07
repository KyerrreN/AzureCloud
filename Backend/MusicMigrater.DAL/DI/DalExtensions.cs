using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MusicMigrater.DAL.Context;

namespace MusicMigrater.DAL.DI;

public static class DalExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection RegisterDalLayer(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(connectionString));

            return services;
        }
    }
}
