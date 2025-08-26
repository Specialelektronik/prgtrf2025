using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM.Streaming;
using SE_Crestron_Training.Logging;
using SE_Crestron_Training.System.DataModel;

namespace SE_Crestron_Training.System;

public class NvxController
{
    private const string LogHeader = "[NVX]";
    private readonly List<Source> _configSources;
    private readonly List<Screen> _configScreens;

    private CrestronControlSystem _cs;

    private Dictionary<int, DmNvx350>? _dmNvxTransmitters = new Dictionary<int, DmNvx350>();
    private Dictionary<int, DmNvx350>? _dmNvxReceivers = new Dictionary<int, DmNvx350>();
    
    public NvxController(List<Source> currentConfigSources, List<Screen> currentConfigScreens, CrestronControlSystem cs)
    {
        _configSources = currentConfigSources;
        _configScreens = currentConfigScreens;
        
        _cs = cs;
        
        SetupSources();
        SetupOutputs();
    }

    /// <summary>
    /// Configure all the NVX receivers that are in the configuration file
    /// We add them to a dictionary, so that we can reference them at a later stage
    /// </summary>
    private void SetupOutputs()
    {
        try
        {
            foreach (var configScreen in _configScreens)
            {
                var tempNvx = new DmNvx350(configScreen.IpId, _cs);
                tempNvx.Register();
                _dmNvxReceivers?.Add(configScreen.Output, tempNvx);
                SeriLog.Log?.Debug($"{LogHeader} NVX receiver for {configScreen.IpId} added");
            }
        }
        catch (Exception e)
        {
            SeriLog.Log?.Fatal(e, $"{LogHeader} Exception while setting up NVX receivers!");
        }
    }

    /// <summary>
    /// Configure all the NVX sources that are in the configuration file
    /// We add them to a dictionary, so that we can reference them at a later stage
    /// </summary>
    private void SetupSources()
    {
        try
        {
            foreach (var configSource in _configSources)
            {
                // integer to hex conversion is important to remember here!
                var tempNvx = new DmNvx350(configSource.IpId, _cs);
                tempNvx.Register();
                _dmNvxTransmitters?.Add(configSource.Input, tempNvx);
                SeriLog.Log?.Debug($"{LogHeader} NVX transmitter for {configSource.IpId} added");
            }
        }
        catch (Exception e)
        {
            SeriLog.Log?.Fatal(e, $"{LogHeader} Exception while setting up NVX transmitters!");
        }
    }

    /// <summary>
    /// Route a source from input (tx) to output (rx)
    /// At this point we are only setting the RTSP URL, but we can of course do more here
    /// </summary>
    public string? RouteSource(int sourceIndex, int destinationIndex)
    {
        try
        {
            // Shouldn't happen, but still
            if (_dmNvxReceivers is not null && _dmNvxTransmitters is not null)
            {
                _dmNvxReceivers[destinationIndex].Control.ServerUrl.StringValue =
                    _dmNvxTransmitters[sourceIndex].Control.ServerUrlFeedback.StringValue;
                
                SeriLog.Log?.Debug($"{LogHeader} Routed {sourceIndex} to {destinationIndex}");

                var preview = _configSources.SingleOrDefault(d => d.Input == sourceIndex)?.PreviewUrl;
                
                // All Crestron collections are 1-based
                //return _dmNvxReceivers[destinationIndex].PreviewImage.PreviewImages.ImageDetails[1]?.FqdnPathFeedback.StringValue;
                return preview;
            }

            return string.Empty;
        }
        catch (Exception e)
        {
            SeriLog.Log?.Fatal(e, $"{LogHeader} Exception while routing");
            return string.Empty;
        }
    }
}