using CX.LoggingLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Serilog.Events;

namespace Opus.Services.Implementation.Logging
{
    public class SeriLogbook : Logbook
    {
        const string template = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}]{NewLine}\tMSG: {Message:lj}{NewLine}\tCLASS: {CallerName}{NewLine}\tMETHOD: {CallerMemberName}{NewLine}{Exception}{NewLine}";

        public SeriLogbook()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Async(a => a.File("logs/opus.log",
                    rollingInterval: RollingInterval.Day, 
                    outputTemplate: template,
                    retainedFileCountLimit: 5))
                .CreateLogger();
        }

        public void CloseAndFlush()
        {
            Write("Closing and flushing logger.", LogLevel.Information);
            Log.CloseAndFlush();
        }

        public override void Write(string message, LogLevel level, Exception? exception = null, string? callerName = "",
            [CallerMemberName] string callerMemberName = "", params object[]? customContent)
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
