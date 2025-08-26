using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.UI;
using Programmerartraff;
using SE_Crestron_Training.Language;
using SE_Crestron_Training.Logging;
using SE_Crestron_Training.System.DataModel;
using SE_Crestron_Training.XiO;

namespace SE_Crestron_Training.System
{
    public class RoomController
    {
        private const string LogHeader = "[RoomController]";
    
        private readonly CrestronControlSystem _cs;
        
        private Ts1070? _touchpanel;

        private readonly XioCloudRoomSlot? _xioCloudRoomSlot;

        private ProgramConnector? _xioProgramConnector;

        private NvxController? _nvxController;
        
        private UserHandler? _userHandler;
        
        private Contract? _contract;
        
        private Room? CurrentConfig { get; set; }
        
        private const ushort DefaultVolumeLevel = 50;
        private ushort LastVolumeLevel { get; set; }
        
        public RoomController(Room? config, XioCloudRoomSlot? xioCloudRoomSlot, CrestronControlSystem cs)
        {
            _cs = cs;
            _xioCloudRoomSlot = xioCloudRoomSlot;

            // Store local configuration
            CurrentConfig = config;

            if (CurrentConfig is not null)
            {
                // Set up the system based on the config
                SetupSystem();
            }
        }

        private void SetupSystem()
        {
            try
            {
                SeriLog.Log?.Information($"{LogHeader} Setting up the touchpanel!");
                // Set up the touchpanel/XPanel
                SetupTouchpanel();
                
                SeriLog.Log?.Information($"{LogHeader} Setting up XiO Cloud program connector!");
                // Set up the XiO Cloud Program Connector
                SetupXiOProgramConnector(_xioCloudRoomSlot);
                
                SeriLog.Log?.Information($"{LogHeader} Setting up NVX!");
                // Set up the NVX related stuff
                SetupNvx();
                
                SeriLog.Log?.Information($"{LogHeader} Setting up user handler");
                // Set up user handler related stuff
                SetupUserHandler();
            }
            catch (Exception e)
            {
                SeriLog.Log?.Fatal(e, $"{LogHeader} Exception in SetupSystem()");
            }
        }

        /// <summary>
        /// Set up everything that is necessary for a touchpanel
        /// In this case, this includes the Construct Contract
        /// </summary>
        private void SetupTouchpanel()
        {
            try
            {
                _touchpanel = new Ts1070(0x03, _cs);
                _touchpanel.OnlineStatusChange += TouchpanelOnOnlineStatusChange;
                // We are using Contracts, but we still need this for the hard buttons
                _touchpanel.ButtonStateChange += TouchpanelOnButtonStateChange;

                if (_touchpanel.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                {
                    SeriLog.Log?.Error($"Unable to register the touchpanel!");
                }
                else
                {
                    SeriLog.Log?.Information($"Registered touchpanel at IPID 0x03");
                }

                _touchpanel.ExtenderTouchDetectionReservedSigs.DeviceExtenderSigChange +=
                    ExtenderTouchDetectionReservedSigsOnDeviceExtenderSigChange;
                _touchpanel.ExtenderTouchDetectionReservedSigs.Use();

                _contract = new Contract(_touchpanel);
                // Setup handlers for individual pages
                // Inside each page could be more .Setup() calls
                _contract.Main.Setup();

                // Power button
                _contract.Main.Poweronoff_PressEvent += MainOnPoweronoff_PressEvent;

                // Source buttons
                _contract.Main.Source1_PressEvent += MainOnSource1_PressEvent;
                _contract.Main.Source2_PressEvent += MainOnSource2_PressEvent;
                _contract.Main.Source3_PressEvent += MainOnSource3_PressEvent;
                _contract.Main.Source4_PressEvent += MainOnSource4_PressEvent;

                // Volume
                _contract.Main.Volume_LowerTouchEvent += MainOnVolume_LowerTouchEvent;

                // Language buttons
                _contract.Main.Swedish_PressEvent += MainOnSwedish_PressEvent;
            }
            catch (Exception e)
            {
                SeriLog.Log?.Fatal(e, $"{LogHeader} Exception in SetupTouchpanel()");
            }
        }

        private void MainOnSwedish_PressEvent(object? sender, UIEventArgs e)
        {
            if (e.SigArgs.Sig.BoolValue)
            {
                if (_userHandler is not null)
                {
                    _ = _userHandler.GetUserInformation();
                }
            }
        }

        private void MainOnPoweronoff_PressEvent(object? sender, UIEventArgs e)
        {
            if (e.SigArgs.Sig.BoolValue)
            {
                // Set volume level on touchpanel
                LastVolumeLevel = DefaultVolumeLevel;

                if (_contract is not null)
                {
                    // Volume to default level
                    _contract.Main.Volume_LowerTouchfb(LastVolumeLevel);
                    // Clear NVX preview
                    _contract.Main.ShowNvxPreview(string.Empty);
                    // Clear XiO status text
                    _contract.Main.xioStatustext_Indirect(string.Empty);
                    // Turn off room in XiO Cloud
                    _xioProgramConnector?.SetRoomPower(false);
                    _xioProgramConnector?.SetDisplayPower(false);
                }
            }
        }

        private void MainOnVolume_LowerTouchEvent(object? sender, UIEventArgs e)
        {
            // Turn off mute?
            // Set level on touchpanel
            LastVolumeLevel = e.SigArgs.Sig.UShortValue;
            _contract?.Main.Volume_LowerTouchfb(LastVolumeLevel);
            SeriLog.Log?.Debug($"{LogHeader} Volume set to: {LastVolumeLevel}");
        }

        private void MainOnSource1_PressEvent(object? sender, UIEventArgs e)
        {
            if (e.SigArgs.Sig.BoolValue)
            {
                // Use ? instead of if(_nvxController is not null)
                var result = _nvxController?.RouteSource(1,1);
                if (result is not null)
                {
                    _contract?.Main.ShowNvxPreview(result);
                }

                _xioProgramConnector?.SetRoomPower(true);
            }
        }
        
        private void MainOnSource2_PressEvent(object? sender, UIEventArgs e)
        {
            if (e.SigArgs.Sig.BoolValue)
            {
                // Use ? instead of if(_nvxController is not null)
                var result = _nvxController?.RouteSource(2,1);
                if (result is not null)
                {
                    _contract?.Main.ShowNvxPreview(result);
                }
                
                _xioProgramConnector?.SetRoomPower(true);
            }
        }
        
        private void MainOnSource3_PressEvent(object? sender, UIEventArgs e)
        {
            if (e.SigArgs.Sig.BoolValue)
            {
                // Use ? instead of if(_nvxController is not null)
                var result = _nvxController?.RouteSource(3,1);
                if (result is not null)
                {
                    _contract?.Main.ShowNvxPreview(result);
                }
                
                _xioProgramConnector?.SetRoomPower(true);
            }
        }
        
        private void MainOnSource4_PressEvent(object? sender, UIEventArgs e)
        {
            if (e.SigArgs.Sig.BoolValue)
            {
                // Use ? instead of if(_nvxController is not null)
                var result = _nvxController?.RouteSource(4,1);
                if (result is not null)
                {
                    _contract?.Main.ShowNvxPreview(result);
                }
                
                _xioProgramConnector?.SetRoomPower(true);
            }
        }

        /// <summary>
        /// Set up everything that is necessary for XiO Cloud Program Connector to work
        /// </summary>
        /// <param name="xioCloudRoomSlot">XiOCloudRoomSlot on this control system</param>
        private void SetupXiOProgramConnector(XioCloudRoomSlot? xioCloudRoomSlot)
        {
            try
            {
                _xioProgramConnector = new ProgramConnector(xioCloudRoomSlot);
                _xioProgramConnector.RoomNameChanged += XioProgramConnectorOnRoomNameChanged;
            }
            catch (Exception e)
            {
                SeriLog.Log?.Fatal(e, $"{LogHeader} Exception in SetupXiOProgramConnector()");
            }
        }
        
        /// <summary>
        /// Set up everything that is necessary for DM-NVX to work.
        /// This includes things like preview URL, RTSP url, etc
        /// </summary>
        private void SetupNvx()
        {
            try
            {
                if(CurrentConfig is not null)
                    _nvxController = new NvxController(CurrentConfig.Sources, CurrentConfig.Screens, _cs);
            }
            catch (Exception e)
            {
                SeriLog.Log?.Fatal(e, $"{LogHeader} Exception in SetupXiOProgramConnector()");
            }
        }

        private void SetupUserHandler()
        {
            _userHandler = new Language.UserHandler();
        }

        private void TouchpanelOnOnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            if (args.DeviceOnLine)
            {
            }
        }
        
        private void TouchpanelOnButtonStateChange(GenericBase device, ButtonEventArgs args)
        {
            switch (args.Button.Name)
            {
                case eButtonName.Power:
                {
                    if (args.Button.State == eButtonState.Pressed)
                    {
                        
                    }
                    break;
                }
            }
        }
        
        private void ExtenderTouchDetectionReservedSigsOnDeviceExtenderSigChange(DeviceExtender currentdeviceextender, SigEventArgs args)
        {
            SeriLog.Log?.Information($"{LogHeader} Touch Detection changed: {args.Sig.Number} - {args.Sig.BoolValue}");
            
            if (args.Sig.Type == eSigType.Bool)
            {
                // we want to change the occupancy of the room based on the touch detection feedback
                if (args.Sig is { Number: 29726 })
                {
                    _xioProgramConnector?.SetOccupancy(args.Sig.BoolValue);
                    // Alternative would be:
                    // if (XioProgramConnector is not null)
                    // {
                    //     
                    // }
                }
            }
        }

        private void XioProgramConnectorOnRoomNameChanged(object? sender, ProgramConnector.RoomNameEventArgs e)
        {
            if (e.RoomName is not null) 
                _contract?.Main.xioStatustext_Indirect(e.RoomName);
            SeriLog.Log?.Information($"XiO Cloud room name change from event handler: {e.RoomName}");
        }
    }
}