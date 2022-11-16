using WF.LoggingLib;
using System.Runtime.CompilerServices;
using Serilog;
using Serilog.Events;
using Serilog.Core;

namespace Opus.Common.Logging
{
    /// <summary>
    /// Base class for serilogging classes.
    /// </summary>
    public class SeriLogbook : Logbook
    {
        private readonly LoggingLevelSwitch levelSwitch;

        /// <summary>
        /// Template for log entries.
        /// </summary>
        const string template =
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}]{NewLine}\tMSG: {Message:lj}{NewLine}\tCLASS: {CallerName}{NewLine}\tMETHOD: {CallerMemberName}{NewLine}{Exception}{NewLine}";

        /// <summary>
        /// Create a new serilogbook.
        /// </summary>
        public SeriLogbook()
        {
            levelSwitch = new LoggingLevelSwitch(LogEventLevel.Information);

            Log.Logger = new LoggerConfiguration().MinimumLevel
                .ControlledBy(levelSwitch)
                .Enrich.FromLogContext()
                .WriteTo.Async(
                    a =>
                        a.File(
                            "logs/opus.log",
                            rollingInterval: RollingInterval.Day,
                            outputTemplate: template,
                            retainedFileCountLimit: 5
                        )
                )
                .CreateLogger();
        }

        /// <summary>
        /// Close and flush the logbook (when closing app).
        /// </summary>
        public void CloseAndFlush()
        {
            Write("Closing and flushing logger.", LogLevel.Information);
            Log.CloseAndFlush();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void ChangeLevel(LogLevel level)
        {
            levelSwitch.MinimumLevel = LogLevelToSerilogLevel(level);
        }

        private LogEventLevel LogLevelToSerilogLevel(LogLevel level)
        {
            return level switch
            {
                LogLevel.Debug => LogEventLevel.Debug,
                LogLevel.Information => LogEventLevel.Information,
                LogLevel.Warning => LogEventLevel.Warning,
                LogLevel.Error => LogEventLevel.Error,
                LogLevel.Fatal => LogEventLevel.Fatal,
                _ => LogEventLevel.Verbose
            };
        }

        /// <summary>
        /// Write into the log.
        /// </summary>
        /// <param name="message">Message to record.</param>
        /// <param name="level">Loglevel.</param>
        /// <param name="exception">Exception that was thrown.</param>
        /// <param name="callerName">Name of the calling class.</param>
        /// <param name="callerMemberName">Name of the calling member of the class.</param>
        /// <param name="customContent">Extra content for log entry.</param>
        public override void Write(
            string message,
            LogLevel level,
            Exception? exception = null,
            string? callerName = "",
            [CallerMemberName] string callerMemberName = "",
            params object[]? customContent
        )
        {
            string? caller = callerName == "" ? "Unknown" : callerName;
            string member = callerMemberName == "" ? "Unkown" : callerMemberName;
            LogEventLevel logLevel = (LogEventLevel)(level + 1);

            Context(caller, member).Write(logLevel, exception, message, customContent);
        }

        private ILogger Context(string? callerName, string callerMemberName)
        {
            return Log.Logger
                .ForContext("CallerName", callerName)
                .ForContext("CallerMemberName", callerMemberName);
        }
    }
}
