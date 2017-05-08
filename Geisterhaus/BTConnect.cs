using System.Runtime.InteropServices;

namespace Geisterhaus
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct BTDeviceInfo
    {
        public System.UInt64 addr;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string name;
    }

    class BTConnect
    {
        [DllImport("BTConnect.dll", SetLastError = true)]
        public static extern bool Init();

        [DllImport("BTConnect.dll", SetLastError = true)]
        public static extern void Release();

        [DllImport("BTConnect.dll", SetLastError = true)]
        public static extern int UpdateDevices();

        [DllImport("BTConnect.dll", SetLastError = true)]
        public static extern int GetDeviceList(int count, [In, Out] BTDeviceInfo[] infos);

        [DllImport("BTConnect.dll", SetLastError = true)]
        public static extern int SendCaptureBitToDevice(int devNum, int command);

        [DllImport("BTConnect.dll", SetLastError = true)]
        public static extern int GetIdentifierFromDevice(int devNum);

        [DllImport("BTConnect.dll", SetLastError = true)]
        public static extern int SendHelloToDevice(int devNum);

        [DllImport("BTConnect.dll", SetLastError = true)]
        public static extern void DisconnectDevice(int devNum);

        [DllImport("BTConnect.dll", SetLastError = true)]
        public static extern void ShutDownCamera(int devNum);

        [DllImport("BTConnect.dll", SetLastError = true)]
        public static extern char GetReceivedMsg();
    }
}
