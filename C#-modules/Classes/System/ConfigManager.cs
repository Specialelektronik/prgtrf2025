using System.Globalization;
using System.Text.Json;
using Crestron.SimplSharp;
using SE_Crestron_Training.Logging;
using SE_Crestron_Training.System.DataModel;
using Directory = Crestron.SimplSharp.CrestronIO.Directory;

namespace SE_Crestron_Training.System
{
    public class ConfigManager
    {
        private const string LogHeader = "[ConfigManager]";
        
        private static readonly CCriticalSection ConfigLock = new();

        private bool _readSuccess;
        
        public Room? RoomConfig { get; private set; }
        
        /// <summary>
        /// Reads a JSON formatted configuration from disc
        /// </summary>
        /// <param name="configFile">Location and name of the config file</param>
        /// <returns>True or False depending on read success</returns>
        public bool ReadConfig(string configFile)
        {
            SeriLog.Log?.Information($"{LogHeader} Started loading config file: {configFile}");
            if (string.IsNullOrEmpty(configFile))
            {
                _readSuccess = false;
                SeriLog.Log?.Information($"{LogHeader} No filename?!?");
            }

            if (!File.Exists(configFile))
            {
                _readSuccess = false;
                SeriLog.Log?.Information($"{LogHeader} Config file doesn't exist");
            }
            else if (File.Exists(configFile))
            {
                ConfigLock.Enter();

                // Open, read and close the file
                string configData;
                using (var file = new StreamReader(configFile))
                {
                    configData = file.ReadToEnd();
                    file.Close();
                }

                try
                {
                    // Try to deserialize into a Room object. If this fails, the JSON file is probably malformed
                    RoomConfig = JsonSerializer.Deserialize<Room>(configData);

                    if (RoomConfig is not null)
                    {
                        if (string.IsNullOrEmpty(RoomConfig.Information!.Guid))
                        {
                            // If there is no GUID available, we create it
                            RoomConfig.Information.Guid = Guid.NewGuid().ToString();

                            // Save the file with updated GUID
                            UpdateConfiguration(configFile);
                        }
                    }

                    SeriLog.Log?.Information($"{LogHeader} Config file loaded!");
                    _readSuccess = true;
                }
                catch (Exception e)
                {
                    _readSuccess = false;
                    SeriLog.Log?.Fatal(e, $"{LogHeader} Exception reading config file");
                }
                finally
                {
                    ConfigLock.Leave();
                }
            }

            return _readSuccess;
        }

        /// <summary>
        /// Update a running configuration
        /// </summary>
        /// <param name="configFile">file name of the config file</param>
        private void UpdateConfiguration(string configFile)
        {
            // Add current date and time to a config file
            try
            {
                SeriLog.Log?.Debug($"Started updating config file: {configFile}");

                if (RoomConfig != null)
                {
                    RoomConfig.LastUpdate = DateTime.Now.ToString(CultureInfo.InvariantCulture);

                    var json = JsonSerializer.Serialize(RoomConfig, new JsonSerializerOptions { WriteIndented = true });
                    var filePath = configFile;

                    using var streamToWrite = new FileStream(filePath, FileMode.Create);
                    using var writer = new StreamWriter(streamToWrite);
                    writer.Write(json);
                }
            }
            catch (Exception e)
            {
                SeriLog.Log?.Fatal(e, $"{LogHeader} Exception updating config file");
            }
        }
    }
}