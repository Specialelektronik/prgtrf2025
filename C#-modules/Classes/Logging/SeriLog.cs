using Crestron.SimplSharp;
using Crestron.SimplSharp.WebScripting;
using Serilog;
using Serilog.Core;
using Serilog.Crestron.Sinks;
using Serilog.Events;

namespace SE_Crestron_Training.Logging
{
    public static class SeriLog
    {
        public static Logger? Log;

        private static LoggerConfiguration? _logConfig;
        
        private static LoggingLevelSwitch? _levelSwitch;

        static SeriLog()
        {
            if(CrestronEnvironment.DevicePlatform == eDevicePlatform.Appliance)
                CrestronConsole.AddNewConsoleCommand(ConsoleLogLevel, "setloglevel", "", ConsoleAccessLevelEnum.AccessOperator);
        }
        
        /// <summary>
        /// Configure SeriLog according to our wishes
        /// </summary>
        public static void ConfigureLogger()
        {
            try
            {
                // We set this up this way because we want to be able to change the Logging Level on the fly
                _levelSwitch = new LoggingLevelSwitch
                {
                    MinimumLevel = LogEventLevel.Debug
                };
            
                // Set up a new LoggerConfiguration
                _logConfig = new LoggerConfiguration();
            
                // Add the level switch
                _logConfig.MinimumLevel.ControlledBy(_levelSwitch);
            
                // We *always* want to write to error log
                _logConfig.WriteTo.CrestronErrorLog();
                // If we are on a control system, we can device that we want to write to console as well
                if (CrestronEnvironment.DevicePlatform == eDevicePlatform.Appliance)
                {
                    _logConfig.WriteTo.CrestronConsole();
                }

                // Create the logger
                Log = _logConfig.CreateLogger();
            }
            catch (Exception e)
            {
                ErrorLog.Exception($"Exception configuring SeriLog", e);
            }
        }

        /// <summary>
        /// Set the minimum log level
        /// Mostly interesting for SEQ and Console
        /// </summary>
        /// <param name="level">SeriLog.Events.LogEventLevel level</param>
        private static void SetLogLevel(LogEventLevel level)
        {
            if(_levelSwitch != null)
                _levelSwitch.MinimumLevel = level;
        }

        /// <summary>
        /// Method to set the serilog loglevel through a console command.
        /// We have not implemented a way to do this on VC-4, but we could've (should've?) have used CWS for this :)
        /// </summary>
        /// <param name="args"></param>
        private static void ConsoleLogLevel(string args)
        {
            switch (args)
            {
                case "debug":
                    SetLogLevel(LogEventLevel.Debug);
                    break;
                case "warning":
                    SetLogLevel(LogEventLevel.Warning);
                    break;
                case "error":
                    SetLogLevel(LogEventLevel.Error);
                    break;
                case "info":
                    SetLogLevel(LogEventLevel.Information);
                    break;
                case "verbose":
                    SetLogLevel(LogEventLevel.Verbose);
                    break;
                default:
                    SetLogLevel(LogEventLevel.Warning);
                    CrestronConsole.ConsoleCommandResponse("Unknown log level provided!");
                    break;
            }
        }
    }
}