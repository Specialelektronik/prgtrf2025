using System.Runtime.InteropServices;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace SE_Example_Program;

public class ControlSystem : CrestronControlSystem
{
    public ControlSystem() : base()
    {
        try
        {
            // Initialize Crestron threadpool
            Crestron.SimplSharpPro.CrestronThread.Thread.MaxNumberOfUserThreads = 20;
            
            CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(ControllerProgramEventHandler);
        }
        catch (Exception e)
        {
            ErrorLog.Error("Error in the constructor: {0}", e.Message);
        }
    }
    
    public override void InitializeSystem()
    {
        try
        {
            ErrorLog.Notice($"Hello world!");
        }
        catch (Exception e)
        {
            ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
        }
    }
    
    void ControllerProgramEventHandler(eProgramStatusEventType programStatusEventType)  
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
}