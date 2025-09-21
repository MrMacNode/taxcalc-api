using Dapper;
using Microsoft.Data.SqlClient;
using TaxcalcApi.Infrastructure.Database.Entities;

namespace TaxcalcApi.Infrastructure.Database.Repositories
{
    public class TaxBandRepository : ITaxBandRepository
    {
        private readonly string _connectionString;

        public TaxBandRepository(IConfiguration configuration)
        {
            _connectionString = configuration["DATABASE__CONNECTION_STRING"];
        }

        public async Task<IEnumerable<TaxBand>> GetAllAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<TaxBand>(
                "GetAllTaxBands",
                commandType: System.Data.CommandType.StoredProcedure
            );
        }
    }
}