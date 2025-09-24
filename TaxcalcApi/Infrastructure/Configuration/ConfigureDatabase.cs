using TaxcalcApi.Infrastructure.Database.Repositories;

namespace TaxcalcApi.Infrastructure.Configuration
{
    public static partial class ConfigurationExtensions
    {
        public static IHostApplicationBuilder ConfigureDatabase(this IHostApplicationBuilder builder)
        {
            // Get the database connection string from configuration. Fail fast if not present.
            string connectionString = builder.TryGetConfigurationString("dbConnectionString")!;

            // Register the SQL connection factory
            builder.Services.AddSingleton<IDbConnectionFactory>(sp => new DbConnectionFactory(connectionString));

            // Register a cache for the repository to use.
            builder.Services.AddMemoryCache();

            // Register the TaxBandRepository
            builder.Services.AddScoped<ITaxBandRepository, TaxBandRepository>();
            return builder;
        }
    }
}
