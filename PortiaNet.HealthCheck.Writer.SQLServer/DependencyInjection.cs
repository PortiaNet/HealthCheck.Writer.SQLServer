using Microsoft.Extensions.DependencyInjection;
using PortiaNet.HealthCheck.Reporter;
using PortiaNet.HealthCheck.Writer.SQLServer;

namespace PortiaNet.HealthCheck.Writer
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSQLServerWriter(this IServiceCollection services, Action<SQLServerConfigurationWriterConfiguration> configuration)
        {
            var config = new SQLServerConfigurationWriterConfiguration();
            configuration(config);
            var reportServiceImplementation = new HealthCheckReportService(config);
            services.AddSingleton<IHealthCheckReportService>(reportServiceImplementation);
            return services;
        }
    }
}
