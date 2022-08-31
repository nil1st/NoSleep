using System.Runtime.InteropServices;

namespace NoSleep
{
    public class RDPSession
    {
        const int SM_REMOTESESSION = 0x1000;

        const int SM_REMOTECONTROL = 0x2001;


        [DllImport("user32")]
        static extern bool GetSystemMetrics(int index);

        public static bool IsRDP()
        {
            return GetSystemMetrics(SM_REMOTESESSION); 
        }

        public static bool IsRemote()
        {
            return GetSystemMetrics(SM_REMOTECONTROL);
        }
    }

}
