using Serilog;
using Serilog.Sinks.Grafana.Loki;

namespace TaxcalcApi.Infrastructure.Configuration
{
    public static partial class ConfigurationExtensions
    {

        /// <summary>
        /// Configures logging to Loki/Grafana.
        /// </summary>
        /// <param name="builder">WebApplicationBuilder at startup.</param>
        /// <remarks>
        /// - Ensure the following configuration settings are provided:
        ///     - logging__uri: The URI of the Loki/Grafana instance.
        ///     - logging__login: The service account or username for authentication.
        ///     - logging__token: The authentication token or password.
        /// - Lack of logging configuration will cause the application to throw an exception at startup. (Fail fast)
        /// - In the event of issues with logging, console debug logs are enabled as a fallback to troubleshoot logging problems.
        /// </remarks>
        /// 
        public static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder)
        {
            // As a fallback for grafana logs, enable console logs for internal debugging.
            //Serilog.Debugging.SelfLog.Enable(Console.WriteLine);

            //Get logging configuration. 
            string loggingUri = builder.TryGetConfigurationString("logging__uri")!;
            string loggingLogin = builder.TryGetConfigurationString("logging__login")!;
            string loggingToken = builder.TryGetConfigurationString("logging__token")!;

            // Configure Serilog to use Grafana Loki as a logging sink.
            builder.Host.UseSerilog((context, services, configuration) =>
            {
                configuration
                    .MinimumLevel.Debug()
                    .WriteTo.Console()
                    .WriteTo.Debug()
                    .WriteTo.GrafanaLoki(
                        loggingUri,
                        labels:
                        [
                            new LokiLabel { Key = "environment", Value = builder.Environment.EnvironmentName },
                            new LokiLabel { Key = "application", Value = builder.Environment.ApplicationName }
                        ],
                        credentials: new()
                        {
                            Login = loggingLogin,
                            Password = loggingToken
                        }
                    );
            });
            return builder;
        }
    }
}
