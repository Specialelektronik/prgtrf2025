using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using SE_Crestron_Training.Logging;
using SE_Crestron_Training.System;
//using Directory = Crestron.SimplSharp.CrestronIO.Directory;

namespace SE_Crestron_Training
{
    public class ControlSystem : CrestronControlSystem
    {
        private const string LogHeader = "[CS]";

        private const string ConfigFile = "config.json";
        
        private ConfigManager Config { get; set; } = null!;
        
        private RoomController _room = null!;
        
        /// <summary>
        /// Initialize the max number of threads (no more than 400)
        /// You cannot send/receive data in the ctor
        /// Make sure it’s in a try/catch
        /// Has to exit in a timely fashion
        /// Use it to:
        ///    Register Devices
        ///    Register EventHandlers
        ///    Add Console Commands
        /// </summary>
        public ControlSystem() : base()
        {
            try
            {
                // Initialize Crestron threadpool
                Crestron.SimplSharpPro.CrestronThread.Thread.MaxNumberOfUserThreads = 20;
                CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(ControllerProgramEventHandler);
                CrestronEnvironment.SystemEventHandler += new SystemEventHandler(ControllerSystemEventHandler);
                CrestronEnvironment.EthernetEventHandler += new EthernetEventHandler(ControllerEthernetEventHandler);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in the constructor: {0}", e.Message);
            }
        }

        /// <summary>
        /// Think of this as the first solution in logic
        /// Make sure it’s in a try/catch
        /// Has to exit in a timely fashion
        /// Use it to:
        ///   Start threads
        ///   Configure Com and Versiports
        ///   Start / Initialize socket connections
        ///   Send Initial device configurations
        /// </summary>
        public override void InitializeSystem()
        {
            try
            {
                // Set up logging first
                SeriLog.ConfigureLogger();
                // Spin up a separate thread to handle all the system set up tasks
                Task.Run(SystemSetup);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
        }

        /// <summary>
        /// Set up the system and all devices.
        /// This could be a done after reading a config file
        /// </summary>
        /// <returns>object --> only null, but we need this to meet the task signature.</returns>
        private void SystemSetup()
        {
            // Create the instance.
            // This should've been a singleton, but not for this demo :)
            Config = new ConfigManager();
            
            try
            {
                string configPath;
                switch (CrestronEnvironment.DevicePlatform)
                {
                    case eDevicePlatform.Appliance:
                        configPath = $"/user/{ConfigFile}";
                        break;
                    case eDevicePlatform.Server:
                        configPath = Path.Combine(Crestron.SimplSharp.CrestronIO.Directory.GetApplicationRootDirectory(), $"user/{ConfigFile}");
                        break;
                    default:
                        configPath = string.Empty;
                        break;
                }
                if (Config.ReadConfig(configPath))
                {
                    if (Config.RoomConfig is not null)
                    {
                        _room = new RoomController(Config.RoomConfig, ControllerXioCloudRoomSlotDevice, this);
                    }
                }
                else
                {
                    SeriLog.Log?.Error($"{LogHeader} Unable to create instance of room!");
                }
            }
            catch (Exception e)
            {
                SeriLog.Log?.Fatal(e, $"{LogHeader} Exception trying to read configuration");
            }
        }

        private void ControllerProgramEventHandler(eProgramStatusEventType programStatusEventType)  
        {  
            switch (programStatusEventType)  
            {  
                case (eProgramStatusEventType.Paused):  
                    // The program has been paused. Pause all user threads/timers as needed.  
                    break;  
                case (eProgramStatusEventType.Resumed):  
                    // The program has been resumed. Resume all the user threads/timers as needed.
                    break;  
                case (eProgramStatusEventType.Stopping):  
                    // The program has been stopped.  
                    // Close all threads.            
                    // Shutdown all Client/Servers in the system.  
                    // General cleanup. 
                    // Unsubscribe to all System Monitor events  
                    break;  
            }  
        }
        
        private  void ControllerSystemEventHandler(eSystemEventType systemEventType)  
        {  
            switch (systemEventType)  
            {  
                case (eSystemEventType.DiskInserted):  
                    // Removable media was detected on the system  
                    break;  
                case (eSystemEventType.DiskRemoved):  
                    // Removable media was detached from the system  
                    break;  
                case (eSystemEventType.Rebooting):  
                    // The system is rebooting.   
                    // Very limited time to preform clean up and save any settings to disk.  
                    break;  
            }
        }
        
        private  void ControllerEthernetEventHandler(EthernetEventArgs ethernetEventArgs)  
        {  
            switch (ethernetEventArgs.EthernetEventType)  
            {
                //Determine the event type Link Up or Link Down  
                case (eEthernetEventType.LinkDown):  
                    //Next need to determine which adapter the event is for.   
                    //LAN is the adapter is the port connected to external networks.  
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)  
                    {  

                    }  
                    break;  
                case (eEthernetEventType.LinkUp):  
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)  
                    {

                    }  
                    break;  
            }
        }
    }
}