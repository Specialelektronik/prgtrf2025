using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using SE_Crestron_Training.Logging;

namespace SE_Crestron_Training.XiO
{
    public class ProgramConnector
    {
        private const string LogHeader = "[XiO Cloud Room]";

        private readonly XioCloudRoomSlot? _xioCloudRoomSlot;

        private string? _roomName;
        
        public event EventHandler<RoomNameEventArgs> RoomNameChanged = delegate { };

        public ProgramConnector(XioCloudRoomSlot? xioCloudRoomSlot)
        {
            _xioCloudRoomSlot = xioCloudRoomSlot;
            if (_xioCloudRoomSlot is not null)
            {
                _xioCloudRoomSlot.BaseEvent += XioCloudRoomOnBaseEvent;
                _xioCloudRoomSlot.OnlineStatusChange += XioCloudRoomOnOnlineStatusChange;

                if (_xioCloudRoomSlot.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                {
                    SeriLog.Log?.Error($"{LogHeader} Error registering XiO Cloud Room Slot: {_xioCloudRoomSlot.RegistrationFailureReason}");
                }
                else
                {
                    SeriLog.Log?.Information($"{LogHeader} registered successfully!");
                }

                CrestronEnvironment.ProgramStatusEventHandler += (info) =>
                {
                    if (info == eProgramStatusEventType.Stopping)
                    {
                        _xioCloudRoomSlot.UnRegister();
                    }
                };
            }
        }

        private void XioCloudRoomOnOnlineStatusChange(GenericBase currentdevice, OnlineOfflineEventArgs args)
        {
            if (args.DeviceOnLine)
            {
                ResolveAllAlerts();
            }
        }

        private void XioCloudRoomOnBaseEvent(GenericBase device, BaseEventArgs args)
        {
            switch (args.EventId)
            {
                case XioCloudRoomSlot.RoomNameEventId:
                    _roomName = _xioCloudRoomSlot?.RoomName.StringValue;
                    SeriLog.Log?.Information($"{LogHeader} RoomName = {_roomName}");
                    OnRoomNameChanged(new RoomNameEventArgs()
                    {
                        RoomName = _roomName
                    });
                    break;

                case XioCloudRoomSlot.DisplayPowerOffEventId:
                    break;

                case XioCloudRoomSlot.DisplayPowerOnEventId:
                    break;

                case XioCloudRoomSlot.MaintenanceModeEventId:
                    SeriLog.Log?.Information($"{LogHeader} Maintenance Mode is {_xioCloudRoomSlot!.MaintenanceMode.BoolValue}");
                    break;

                case XioCloudRoomSlot.OccupiedEventId:
                    SeriLog.Log?.Information($"{LogHeader} Occupancy = {_xioCloudRoomSlot?.OccupiedFeedback.BoolValue}");
                    break;

                case XioCloudRoomSlot.VacantEventId:
                    break;

                case XioCloudRoomSlot.SystemPowerOffEventId:
                    break;

                case XioCloudRoomSlot.SystemPowerOnEventId:
                    break;

                case XioCloudRoomSlot.SystemCheckEventId:
                    if (_xioCloudRoomSlot is not null)
                    {
                        RunSystemCheck();
                    }

                    break;
            }
        }
        
        public void ResolveAllAlerts()
        {
            if (_xioCloudRoomSlot is not null)
            {
                SeriLog.Log?.Debug($"Resolve all alerts");
                _xioCloudRoomSlot.ResolveAllAlertsFeedback.BoolValue = false;
                Thread.Sleep(500);
                _xioCloudRoomSlot.ResolveAllAlertsFeedback.BoolValue = false;
                Thread.Sleep(500);
                _xioCloudRoomSlot.ResolveAllAlertsFeedback.BoolValue = true;
            }
        }

        private void RunSystemCheck()
        {
            if (_xioCloudRoomSlot is null) return;
            _xioCloudRoomSlot.SystemCheckStateFeedback.Value = XioCloudRoomSlot.eSystemCheckState.Idle; // Idle
            _xioCloudRoomSlot.SystemCheckStateFeedback.Value = XioCloudRoomSlot.eSystemCheckState.Running; // Running
            _xioCloudRoomSlot.SystemCheckMessageFeedback.StringValue = "SystemCheck started";
            Thread.Sleep(2000);
            _xioCloudRoomSlot.SystemCheckMessageFeedback.StringValue = "Microphones are working!";
            _xioCloudRoomSlot.SystemCheckStateFeedback.Value = XioCloudRoomSlot.eSystemCheckState.Success;
        }
        
        public void ClearSystemCheck()
        {
            if (_xioCloudRoomSlot is not null)
            {
                _xioCloudRoomSlot.SystemCheckStateFeedback.Value = XioCloudRoomSlot.eSystemCheckState.Idle; // Idle
                _xioCloudRoomSlot.SystemCheckStateFeedback.Value = XioCloudRoomSlot.eSystemCheckState.Running; // Running
                _xioCloudRoomSlot.SystemCheckStateFeedback.Value = XioCloudRoomSlot.eSystemCheckState.Success;
            }
        }
        
        public void SetRoomPower(bool value)
        {
            if (_xioCloudRoomSlot is not null)
            {
                _xioCloudRoomSlot.SystemPowerFeedback.BoolValue = value;
            }
        }
        
        public void SetDisplayPower(bool value)
        {
            if (_xioCloudRoomSlot is not null)
            {
                _xioCloudRoomSlot.DisplayPowerFeedback.BoolValue = value;
            }
        }
        
        public void SetOccupancy(bool value)
        {
            if (_xioCloudRoomSlot is not null)
            {
                _xioCloudRoomSlot.OccupiedFeedback.BoolValue = value;
            }
        }
        
        private void OnRoomNameChanged(RoomNameEventArgs e)
        {
            RoomNameChanged(null, e);
        }
        
        public class RoomNameEventArgs : EventArgs
        {
            public string? RoomName { get; init; }
        }
    }
}