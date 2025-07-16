using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace user_panel.Helpers
{
    public static class LoggerHelper
    {
        private static Serilog.ILogger? _manualLogger;

        public static Serilog.ILogger GetManualLogger(IConfiguration configuration)
        {
            if (_manualLogger != null)
                return _manualLogger;

            string connectionString = configuration.GetConnectionString("DefaultConnection")
                                      ?? throw new InvalidOperationException("Connection string not found.");

            var sinkOptions = new MSSqlServerSinkOptions
            {
                TableName = "Logs",
                AutoCreateSqlTable = false
            };

            _manualLogger = new LoggerConfiguration()
                .WriteTo.MSSqlServer(
                    connectionString: connectionString,
                    sinkOptions: sinkOptions,
                    restrictedToMinimumLevel: LogEventLevel.Information
                )
                .CreateLogger();

            return _manualLogger;
        }
    }
}
