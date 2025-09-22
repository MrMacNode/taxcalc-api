namespace TaxcalcApi.Infrastructure.Configuration
{
    public static partial class ConfigurationExtensions
    {
        /// <summary>
        /// Packages a common pattern of retrieving configuration values.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="key">The key for the configuration setting.</param>
        /// <param name="required">If true (default), an exception will be thrown if the configuration item is not found.</param>
        /// <param name="defaultValue">A default alternative for optional configuration items.</param>
        /// <returns>The string value of the configuration setting.</returns>
        /// <exception cref="ArgumentException">Thrown if the key is invalid (empty or null)</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the configuration item is required and not present.</exception>
        public static string? TryGetConfigurationString(this IHostApplicationBuilder builder, string key, bool required = true, string? defaultValue = null)
        {
            if(string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Invalid configuration key.");
            }

            var value = builder.Configuration[key];

            if(!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            if(required)
            {
                throw new KeyNotFoundException($"Required configuration item '{key}' not found.");
            }

            return defaultValue;
        }
    }
}
