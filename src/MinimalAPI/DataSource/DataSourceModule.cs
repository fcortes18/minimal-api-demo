using Microsoft.EntityFrameworkCore;
using MinimalAPI.DataSource.Tables;

namespace MinimalAPI.DataSource
{
    public static class DataSourceModule
    {
        public static void AddDataSourceModule(this IServiceCollection services)
        {
            services.AddDbContext<StoreDbContext>(opt => opt.UseSqlServer("CONNECTION_STRING"));
        }
    }
}
