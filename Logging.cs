using Serilog;
using Serilog.Context;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace GetEPGs
{
    static internal class Logging
    {
        static internal void Init()
        {
           Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(LogEventLevel.Debug, theme: AnsiConsoleTheme.Literate)
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(evt =>
                        evt.Properties.TryGetValue("LogToFile", out var logToFile) &&
                        logToFile is ScalarValue sv &&
                        sv.Value is bool b &&
                        b)
                    .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", $"log-.txt"), LogEventLevel.Information, rollingInterval: RollingInterval.Day))
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        public static void Info(this string message, bool logToFile = false)
        {
            using (LogContext.PushProperty("LogToFile", logToFile))
                Log.Logger.Information(message);
        }

        public static void Warn(this string message, bool logToFile = false)
        {
            using (LogContext.PushProperty("LogToFile", logToFile))
                Log.Logger.Warning(message);
        }

        public static void Error(this string message, bool logToFile = false)
        {
            using (LogContext.PushProperty("LogToFile", logToFile))
                Log.Logger.Error(message);
        }
    }
}
