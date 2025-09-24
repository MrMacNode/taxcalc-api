using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using System.Data;
using TaxcalcApi.Infrastructure.Database.Entities;
using Polly;
using Polly.Retry;

namespace TaxcalcApi.Infrastructure.Database.Repositories
{
    /// <summary>
    /// A repository for accessing tax band data from the database.
    /// </summary>
    /// <remarks>
    ///     - This implementation uses Dapper for data access and SQL Server as the database.
    ///     - Includes retry logic for transient faults using Polly.
    ///     - Caches results in memory to reduce database load.
    /// </remarks>
    public class TaxBandRepository(ILogger<TaxBandRepository> _logger, IMemoryCache _cache, IDbConnectionFactory _connectionFactory) : ITaxBandRepository
    {
        /// <summary>
        /// Retrieves all tax bands from the database, with caching and retry logic for transient faults.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>All tax bands</returns>
        /// <remarks>
        /// Considerations if the scope of the application expands:
        ///     - Caching strategy may need to be revisited for distributed scenarios.
        ///     - Retry policies may need to be adjusted based on observed failure patterns.
        ///     - Connection management may need to be enhanced for high-throughput scenarios.
        ///     - If tax calculations are needed for more countries, a more flexible data model may be required, including parameters for different tax years and jurisdictions.
        ///     - If the list of tax bands becomes large, it might be filtered based on income range.
        /// </remarks>
        public async Task<IEnumerable<TaxBand>> GetAllAsync(CancellationToken cancellationToken)
        {
            const string cacheKey = "TaxBandsCacheKey";

            //Try to get bands from cache first
            if (_cache.TryGetValue(cacheKey, out IEnumerable<TaxBand>? cachedTaxBands) && cachedTaxBands != null)
            {
                _logger.LogInformation("Fetching tax bands from cache.");
                return cachedTaxBands;
            }

            //If bands are not in the cache, pull from database.
            _logger.LogInformation("Fetching all tax bands from database.");

            // Use Polly to implement retry logic for transient faults.
            return await RetryPolicy.ExecuteAsync(async () =>
            {
                //Run the query
                using var connection = _connectionFactory.GetDbConnection();
                var taxBands = await connection.QueryAsync<TaxBand>(
                    new CommandDefinition(
                        "GetAllTaxBands",
                        parameters: null,
                        transaction: null,
                        commandTimeout: 10,
                        commandType: CommandType.StoredProcedure,
                        flags: CommandFlags.Buffered,
                        cancellationToken: cancellationToken
                    )
                );

                //Cache the result for 1 hour
                _cache.Set(cacheKey, taxBands, TimeSpan.FromHours(1));
                
                //Return result from the database.
                return taxBands;
            });
        }

        //List of SQL error numbers that we'll retry as transient errors.
        //Some of these errors are quite broad, but we'll retry a set number of times before failing.
        //TODO: Put these in config? Company standards may dictate which of these should be retried, and company standards change.
        // (We're also being a bit presumptive that we're using SQL Server; codes could change depending on the database engine.)
        public static readonly HashSet<int> TransientErrorNumbers =
        [
            4060, //Cannot open database
            10928, 10929, //Resource Limit Errors
            40197, //The service has encountered an error processing your request. Please try again.
            40501, //Currently busy.
            40613, //Database is not currently available.
            49918, 49919, 49920, //Too many operations
            1205, //Deadlock D:
            233, 10053, 10054, 10060, //Connection-related errors
            64, 20, //Network-related errors
            -2 //Timeout
        ];

        // Determines if a SqlException is transient based on its error numbers.
        private static bool IsTransient(SqlException ex) =>
            ex.Errors.Cast<SqlError>().Any(e => TransientErrorNumbers.Contains(e.Number));

        //TODO: Make retry waits configurable
        private static readonly AsyncRetryPolicy RetryPolicy = Policy
            .Handle<SqlException>(ex => IsTransient(ex))
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}