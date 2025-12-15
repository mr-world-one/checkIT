using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Check_IT.Interfaces;

namespace Check_IT.Services
{
    public static class LoggingExtensions
    {
        public static IServiceCollection AddAppLogging(this IServiceCollection services)
        {
            services.AddSingleton<Serilog.ILogger>(Log.Logger);
            services.AddSingleton<IAppLogger, SerilogAppLogger>();
            return services;
        }
    }

    public class SerilogAppLogger : IAppLogger
    {
        private readonly Serilog.ILogger _logger;
        public SerilogAppLogger(Serilog.ILogger logger)
        {
            _logger = logger;
        }

        public void Information(string message) => _logger.Information(message);
        public void Debug(string message) => _logger.Debug(message);
        public void Warning(string message) => _logger.Warning(message);
        public void Error(Exception? ex, string message) => _logger.Error(ex, message);
    }
}