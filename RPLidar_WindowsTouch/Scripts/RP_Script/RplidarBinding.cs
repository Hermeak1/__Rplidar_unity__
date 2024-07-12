using System.Runtime.InteropServices;
using System.IO;
using System;

namespace RpSensor
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct LidarData
    {
        public byte syncBit;
        public float theta;
        public float distant;
        public uint quality;
    };

    public class RplidarBinding
    {
        static RplidarBinding()
        {
            var currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
            var path = Environment.CurrentDirectory;
            currentPath += Path.PathSeparator + path + "/dll/x86_64/";
            Environment.SetEnvironmentVariable("PATH", currentPath);
        }
        [DllImport(@"RplidarCpp.dll")]
        public static extern int OnConnect(int idx, string port, UInt32 baudrate);
        [DllImport(@"RplidarCpp.dll")]
        public static extern bool OnDisconnect(int idx);

        [DllImport("RplidarCpp.dll")]
        public static extern bool OnSetMotorPwm(int idx, int pwm);

        [DllImport(@"RplidarCpp.dll")]
        public static extern bool StartMotor(int idx);
        [DllImport("RplidarCpp.dll")]
        public static extern bool EndMotor(int idx);

        [DllImport("RplidarCpp.dll")]
        public static extern bool StartScan(int idx, string mode);
        [DllImport("RplidarCpp.dll")]
        public static extern bool EndScan(int idx);

        [DllImport("RplidarCpp.dll")]
        public static extern bool ReleaseDrive(int idx);

        [DllImport("RplidarCpp.dll")]
        public static extern int GetLDataSize();

        [DllImport("RplidarCpp.dll")]
        private static extern void GetLDataSampleArray(IntPtr ptr);

        [DllImport("RplidarCpp.dll")]
        private static extern int GrabData(int idx, IntPtr ptr);

        public static LidarData[] GetSampleData()
        {
            var d = new LidarData[2];
            var handler = GCHandle.Alloc(d, GCHandleType.Pinned);
            GetLDataSampleArray(handler.AddrOfPinnedObject());
            handler.Free();
            return d;
        }

        public static int GetData(int idx, ref LidarData[] data)
        {
            var handler = GCHandle.Alloc(data, GCHandleType.Pinned);
            int count = GrabData(idx, handler.AddrOfPinnedObject());
            handler.Free();

            return count;
        }
    }
}
