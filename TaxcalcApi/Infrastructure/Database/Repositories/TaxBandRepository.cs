using Dapper;
using Microsoft.Data.SqlClient;
using TaxcalcApi.Infrastructure.Database.Entities;

namespace TaxcalcApi.Infrastructure.Database.Repositories
{
    public class TaxBandRepository : ITaxBandRepository
    {
        private readonly string _connectionString;
        private ILogger<TaxBandRepository> _logger;

        //TODO: Create a builder extension method to validate configuration settings at startup.
        public TaxBandRepository(IConfiguration configuration, ILogger<TaxBandRepository> logger)
        {
            _connectionString = configuration["DATABASE__CONNECTION_STRING"];
            _logger = logger;
        }

        public async Task<IEnumerable<TaxBand>> GetAllAsync(CancellationToken cancellationToken)
        {
            //TODO: Implement caching here to reduce database load
            //TODO: Implement retry logic for transient faults

            _logger.LogInformation("Fetching all tax bands from database.");
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<TaxBand>(
                "GetAllTaxBands",
                cancellationToken,
                commandType: System.Data.CommandType.StoredProcedure 
            );
        }
    }
}