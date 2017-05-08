using System;
using System.Runtime.InteropServices;

namespace Geisterhaus
{
    public class DLLTools
    {

        [DllImport("fex.dll")]
        public static extern int start();

        [DllImport("fex.dll")]
        public static extern int stop();

        [DllImport("fex.dll")]
        public static extern int learn(double seconds);

        [DllImport("fex.dll")]
        public static extern int fetch(IntPtr dataBuffer, ref int size, ref int is_ready);

    }
}