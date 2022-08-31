using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace NoSleep
{
    public class RDPSession
    {
        public RDPSession()
        {
            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);

        }

        public static bool IsConnected { get; set; } = true;

        static void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.RemoteDisconnect || e.Reason == SessionSwitchReason.ConsoleDisconnect)
            {
                IsConnected = false;// Log off the user...

            }
            else if (e.Reason == SessionSwitchReason.RemoteConnect || e.Reason == SessionSwitchReason.ConsoleConnect)
            {
                IsConnected = true;
            }
            else
            {
                IsConnected = true;// Physical Logon
            }
        }
        //const int SM_REMOTESESSION = 0x1000;

        //const int SM_REMOTECONTROL = 0x2001;


        //[DllImport("user32")]
        //static extern bool GetSystemMetrics(int index);

        //public static bool IsRDP()
        //{
        //    return GetSystemMetrics(SM_REMOTESESSION); 
        //}

        //public static bool IsRemote()
        //{
        //    return GetSystemMetrics(SM_REMOTECONTROL);
        //}


    }

}
