namespace TaxcalcApi.Infrastructure.Configuration
{
    public static partial class ConfigurationExtensions
    {
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
