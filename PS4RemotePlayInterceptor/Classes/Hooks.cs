// PS4RemotePlayInterceptor (File: Classes/Hooks.cs)
//
// Copyright (c) 2018 Komefai
//
// Visit http://komefai.com for more information
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using EasyHook;

namespace PS4RemotePlayInterceptor
{
    /// <summary>
    /// EasyHook will look for a class implementing <see cref="EasyHook.IEntryPoint"/> during injection. This
    /// becomes the entry point within the target process after injection is complete.
    /// </summary>
    public class Hooks : EasyHook.IEntryPoint
    {
        /// <summary>
        /// Reference to the server interface
        /// </summary>
        private readonly InjectionInterface _server = null;

        /// <summary>
        /// Dummy handle used for controller emulation
        /// </summary>
        private readonly IntPtr _dummyHandle = new IntPtr(0xDABDAB);

        private static byte[] ToManagedArray(IntPtr pointer, int size)
        {
            byte[] managedArray = new byte[size];
            Marshal.Copy(pointer, managedArray, 0, size);
            return managedArray;
        }

        private static void RestoreUnmanagedArray(IntPtr pointer, int size, byte[] managedArray)
        {
            unsafe
            {
                byte* ptr = (byte*)pointer.ToPointer();
                for (var i = 0; i < size; i++)
                {
                    ptr[i] = managedArray[i];
                }
            }
        }

        #region Setup
        /// <summary>
        /// EasyHook requires a constructor that matches <paramref name="context"/> and any additional parameters as provided
        /// in the original call to <see cref="EasyHook.RemoteHooking.Inject(int, EasyHook.InjectionOptions, string, string, object[])"/>.
        /// 
        /// Multiple constructors can exist on the same <see cref="EasyHook.IEntryPoint"/>, providing that each one has a corresponding Run method (e.g. <see cref="Run(EasyHook.RemoteHooking.IContext, string)"/>).
        /// </summary>
        /// <param name="context">The RemoteHooking context</param>
        /// <param name="channelName">The name of the IPC channel</param>
        public Hooks(
            EasyHook.RemoteHooking.IContext context,
            string channelName)
        {
            // Connect to server object using provided channel name
            _server = EasyHook.RemoteHooking.IpcConnectClient<InjectionInterface>(channelName);

            // If Ping fails then the Run method will be not be called
            _server.Ping();
        }

        /// <summary>
        /// The main entry point for our logic once injected within the target process. 
        /// This is where the hooks will be created, and a loop will be entered until host process exits.
        /// EasyHook requires a matching Run method for the constructor
        /// </summary>
        /// <param name="context">The RemoteHooking context</param>
        /// <param name="channelName">The name of the IPC channel</param>
        public void Run(
            EasyHook.RemoteHooking.IContext context,
            string channelName)
        {
            // Injection is now complete and the server interface is connected
            _server.OnInjectionSuccess(EasyHook.RemoteHooking.GetCurrentProcessId());

            // Install hooks
            List<EasyHook.LocalHook> hooks = new List<LocalHook>();

            // ReadFile https://msdn.microsoft.com/en-us/library/windows/desktop/aa365467(v=vs.85).aspx
            var readFileHook = EasyHook.LocalHook.Create(
                EasyHook.LocalHook.GetProcAddress("kernel32.dll", "ReadFile"),
                new ReadFile_Delegate(ReadFile_Hook),
                this);

            hooks.Add(readFileHook);
            

            // Activate hooks on all threads except the current thread
            foreach (var h in hooks)
            {
                h.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
            }

            // Wake up the process (required if using RemoteHooking.CreateAndInject)
            EasyHook.RemoteHooking.WakeUpProcess();

            try
            {
                // Loop until injector closes (i.e. IPC fails)
                while (true)
                {
                    System.Threading.Thread.Sleep(100);
                    _server.Ping();
                }
            }
            catch
            {
                // Ping() will raise an exception if host is unreachable
            }

            // Remove hooks
            foreach (var h in hooks)
            {
                h.Dispose();
            }

            // Finalise cleanup of hooks
            EasyHook.LocalHook.Release();
        }

        /// <summary>
        /// P/Invoke to determine the filename from a file handle
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/aa364962(v=vs.85).aspx
        /// </summary>
        /// <param name="hFile"></param>
        /// <param name="lpszFilePath"></param>
        /// <param name="cchFilePath"></param>
        /// <param name="dwFlags"></param>
        /// <returns></returns>
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern uint GetFinalPathNameByHandle(IntPtr hFile, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszFilePath, uint cchFilePath, uint dwFlags);

        #endregion

        #region CreateFileW Hook
        /// <summary>
        /// The CreateFile delegate, this is needed to create a delegate of our hook function <see cref="CreateFile_Hook(string, uint, uint, IntPtr, uint, uint, IntPtr)"/>.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="desiredAccess"></param>
        /// <param name="shareMode"></param>
        /// <param name="securityAttributes"></param>
        /// <param name="creationDisposition"></param>
        /// <param name="flagsAndAttributes"></param>
        /// <param name="templateFile"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall,
                    CharSet = CharSet.Unicode,
                    SetLastError = true)]
        delegate IntPtr CreateFile_Delegate(
                    String filename,
                    UInt32 desiredAccess,
                    UInt32 shareMode,
                    IntPtr securityAttributes,
                    UInt32 creationDisposition,
                    UInt32 flagsAndAttributes,
                    IntPtr templateFile);

        /// <summary>
        /// Using P/Invoke to call original method.
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/aa363858(v=vs.85).aspx
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="desiredAccess"></param>
        /// <param name="shareMode"></param>
        /// <param name="securityAttributes"></param>
        /// <param name="creationDisposition"></param>
        /// <param name="flagsAndAttributes"></param>
        /// <param name="templateFile"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll",
            CharSet = CharSet.Unicode,
            SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        static extern IntPtr CreateFileW(
            String filename,
            UInt32 desiredAccess,
            UInt32 shareMode,
            IntPtr securityAttributes,
            UInt32 creationDisposition,
            UInt32 flagsAndAttributes,
            IntPtr templateFile);

        /// <summary>
        /// The CreateFile hook function. This will be called instead of the original CreateFile once hooked.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="desiredAccess"></param>
        /// <param name="shareMode"></param>
        /// <param name="securityAttributes"></param>
        /// <param name="creationDisposition"></param>
        /// <param name="flagsAndAttributes"></param>
        /// <param name="templateFile"></param>
        /// <returns></returns>
        IntPtr CreateFile_Hook(
            String filename,
            UInt32 desiredAccess,
            UInt32 shareMode,
            IntPtr securityAttributes,
            UInt32 creationDisposition,
            UInt32 flagsAndAttributes,
            IntPtr templateFile)
        {
            // SPOOF
            if (filename != null && filename.StartsWith(@"\\?\hid#"))
            {
                return _dummyHandle;
            }

            IntPtr result = CreateFileW(
                filename,
                desiredAccess,
                shareMode,
                securityAttributes,
                creationDisposition,
                flagsAndAttributes,
                templateFile
            );

            try
            {
                string mode = string.Empty;
                switch (creationDisposition)
                {
                    case 1:
                        mode = "CREATE_NEW";
                        break;
                    case 2:
                        mode = "CREATE_ALWAYS";
                        break;
                    case 3:
                        mode = "OPEN_ALWAYS";
                        break;
                    case 4:
                        mode = "OPEN_EXISTING";
                        break;
                    case 5:
                        mode = "TRUNCATE_EXISTING";
                        break;
                }

                // Send to server
                _server.OnCreateFile(filename.ToString(), result.ToString());
            }
            catch
            {
                // swallow exceptions so that any issues caused by this code do not crash target process
            }

            // now call the original API...
            return result;
        }
        #endregion

        #region ReadFile Hook

        // FrameCounter
        private static int __frameCounter = -1;

        /// <summary>
        /// The ReadFile delegate, this is needed to create a delegate of our hook function <see cref="ReadFile_Hook(IntPtr, IntPtr, uint, out uint, IntPtr)"/>.
        /// </summary>
        /// <param name="hFile"></param>
        /// <param name="lpBuffer"></param>
        /// <param name="nNumberOfBytesToRead"></param>
        /// <param name="lpNumberOfBytesRead"></param>
        /// <param name="lpOverlapped"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        delegate bool ReadFile_Delegate(
            IntPtr hFile,
            IntPtr lpBuffer,
            uint nNumberOfBytesToRead,
            out uint lpNumberOfBytesRead,
            IntPtr lpOverlapped);

        /// <summary>
        /// Using P/Invoke to call the orginal function
        /// </summary>
        /// <param name="hFile"></param>
        /// <param name="lpBuffer"></param>
        /// <param name="nNumberOfBytesToRead"></param>
        /// <param name="lpNumberOfBytesRead"></param>
        /// <param name="lpOverlapped"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        static extern bool ReadFile(
            IntPtr hFile,
            IntPtr lpBuffer,
            uint nNumberOfBytesToRead,
            out uint lpNumberOfBytesRead,
            IntPtr lpOverlapped);

        /// <summary>
        /// The ReadFile hook function. This will be called instead of the original ReadFile once hooked.
        /// </summary>
        /// <param name="hFile"></param>
        /// <param name="lpBuffer"></param>
        /// <param name="nNumberOfBytesToRead"></param>
        /// <param name="lpNumberOfBytesRead"></param>
        /// <param name="lpOverlapped"></param>
        /// <returns></returns>
        bool ReadFile_Hook(
            IntPtr hFile,
            IntPtr lpBuffer,
            uint nNumberOfBytesToRead,
            out uint lpNumberOfBytesRead,
            IntPtr lpOverlapped)
        {
            bool result = false;
            lpNumberOfBytesRead = 0;

            const int bufferSize = 64;

            try
            {
                // Call original first so we have a value for lpNumberOfBytesRead
                result = ReadFile(hFile, lpBuffer, nNumberOfBytesToRead, out lpNumberOfBytesRead, lpOverlapped);

                try
                {
                    // Retrieve filename from the file handle
                    StringBuilder filename = new StringBuilder(255);
                    GetFinalPathNameByHandle(hFile, filename, 255, 0);

                    //// Log for debug
                    //_server.ReportLog(
                    //    string.Format("[{0}:{1}]: READ ({2} bytes) \"{3}\"",
                    //    EasyHook.RemoteHooking.GetCurrentProcessId(), EasyHook.RemoteHooking.GetCurrentThreadId()
                    //    , lpNumberOfBytesRead, filename));

                    // Only respond if it is a device stream
                    if (string.IsNullOrWhiteSpace(filename.ToString()) && lpNumberOfBytesRead == bufferSize)
                    {
                        // Copy unmanaged array for server
                        byte[] managedArray = ToManagedArray(lpBuffer, bufferSize);

                        // Make sure it is a input report (USB type)
                        if (managedArray[0] == 0x1)
                        {
                            // Send to server
                            _server.OnReadFile(filename.ToString(), ref managedArray);

                            // Restore managedArray back to unmanaged array
                            RestoreUnmanagedArray(lpBuffer, bufferSize, managedArray);
                        }
                    }
                }
                catch
                {
                    // swallow exceptions so that any issues caused by this code do not crash target process
                }
            }
            catch
            {
                // swallow exceptions so that any issues caused by this code do not crash target process
            }

            return result;
        }
        #endregion

        #region WriteFile Hook

        /// <summary>
        /// The WriteFile delegate, this is needed to create a delegate of our hook function <see cref="WriteFile_Hook(IntPtr, IntPtr, uint, out uint, IntPtr)"/>.
        /// </summary>
        /// <param name="hFile"></param>
        /// <param name="lpBuffer"></param>
        /// <param name="nNumberOfBytesToWrite"></param>
        /// <param name="lpNumberOfBytesWritten"></param>
        /// <param name="lpOverlapped"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        delegate bool WriteFile_Delegate(
            IntPtr hFile,
            IntPtr lpBuffer,
            uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten,
            IntPtr lpOverlapped);

        /// <summary>
        /// Using P/Invoke to call original WriteFile method
        /// </summary>
        /// <param name="hFile"></param>
        /// <param name="lpBuffer"></param>
        /// <param name="nNumberOfBytesToWrite"></param>
        /// <param name="lpNumberOfBytesWritten"></param>
        /// <param name="lpOverlapped"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool WriteFile(
            IntPtr hFile,
            IntPtr lpBuffer,
            uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten,
            IntPtr lpOverlapped);

        /// <summary>
        /// The WriteFile hook function. This will be called instead of the original WriteFile once hooked.
        /// </summary>
        /// <param name="hFile"></param>
        /// <param name="lpBuffer"></param>
        /// <param name="nNumberOfBytesToWrite"></param>
        /// <param name="lpNumberOfBytesWritten"></param>
        /// <param name="lpOverlapped"></param>
        /// <returns></returns>
        bool WriteFile_Hook(
            IntPtr hFile,
            IntPtr lpBuffer,
            uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten,
            IntPtr lpOverlapped)
        {
            bool result = false;

            // Call original first so we get lpNumberOfBytesWritten
            result = WriteFile(hFile, lpBuffer, nNumberOfBytesToWrite, out lpNumberOfBytesWritten, lpOverlapped);

            try
            {
                // Retrieve filename from the file handle
                StringBuilder filename = new StringBuilder(255);
                GetFinalPathNameByHandle(hFile, filename, 255, 0);
            }
            catch
            {
                // swallow exceptions so that any issues caused by this code do not crash target process
            }

            return result;
        }

        #endregion

        #region HidD_GetAttributes Hook
        [StructLayout(LayoutKind.Sequential)]
        internal struct HIDD_ATTRIBUTES
        {
            public Int32 Size;
            public Int16 VendorID;
            public Int16 ProductID;
            public Int16 VersionNumber;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate bool HidD_GetAttributes_Delegate(IntPtr hidDeviceObject, ref HIDD_ATTRIBUTES attributes);

        [DllImport("hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool HidD_GetAttributes(IntPtr hidDeviceObject, ref HIDD_ATTRIBUTES attributes);

        bool HidD_GetAttributes_Hook(IntPtr hidDeviceObject, ref HIDD_ATTRIBUTES attributes)
        {
            bool result = false;

            try
            {
                // Call original first so we get the result
                result = HidD_GetAttributes(hidDeviceObject, ref attributes);
            }
            catch
            {
                // swallow exceptions so that any issues caused by this code do not crash target process
            }

            return result;
        }
        #endregion

        #region HidD_GetFeature Hook
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate bool HidD_GetFeature_Delegate(IntPtr hidDeviceObject, ref Byte lpReportBuffer, Int32 reportBufferLength);

        [DllImport("hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool HidD_GetFeature(IntPtr hidDeviceObject, ref Byte lpReportBuffer, Int32 reportBufferLength);

        bool HidD_GetFeature_Hook(IntPtr hidDeviceObject, ref Byte lpReportBuffer, Int32 reportBufferLength)
        {
            bool result = false;

            try
            {
                // Call original first so we get the result
                result = HidD_GetFeature(hidDeviceObject, ref lpReportBuffer, reportBufferLength);
            }
            catch
            {
                // swallow exceptions so that any issues caused by this code do not crash target process
            }

            return result;
        }
        #endregion

        #region HidD_SetFeature Hook
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate bool HidD_SetFeature_Delegate(IntPtr hidDeviceObject, ref Byte lpReportBuffer, Int32 reportBufferLength);

        [DllImport("hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool HidD_SetFeature(IntPtr hidDeviceObject, ref Byte lpReportBuffer, Int32 reportBufferLength);

        bool HidD_SetFeature_Hook(IntPtr hidDeviceObject, ref Byte lpReportBuffer, Int32 reportBufferLength)
        {
            bool result = false;

            try
            {
                result = HidD_SetFeature(hidDeviceObject, ref lpReportBuffer, reportBufferLength);
            }
            catch
            {
                // swallow exceptions so that any issues caused by this code do not crash target process
            }

            return result;
        }
        #endregion

        #region HidD_GetPreparsedData Hook
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate bool HidD_GetPreparsedData_Delegate(IntPtr hidDeviceObject, ref IntPtr preparsedData);

        [DllImport("hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool HidD_GetPreparsedData(IntPtr hidDeviceObject, ref IntPtr preparsedData);

        bool HidD_GetPreparsedData_Hook(IntPtr hidDeviceObject, ref IntPtr preparsedData)
        {
            bool result = false;

            try
            {
                result = HidD_GetPreparsedData(hidDeviceObject, ref preparsedData);
            }
            catch
            {
                // swallow exceptions so that any issues caused by this code do not crash target process
            }

            return result;
        }
        #endregion

        #region HidD_FreePreparsedData Hook
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate bool HidD_FreePreparsedData_Delegate(IntPtr preparsedData);

        [DllImport("hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool HidD_FreePreparsedData(IntPtr preparsedData);

        bool HidD_FreePreparsedData_Hook(IntPtr preparsedData)
        {
            bool result = false;

            try
            {
                // Call original first so we get the result
                result = HidD_FreePreparsedData(preparsedData);
            }
            catch
            {
                // swallow exceptions so that any issues caused by this code do not crash target process
            }

            return result;
        }
        #endregion

        #region HidD_GetManufacturerString Hook
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate bool HidD_GetManufacturerString_Delegate(IntPtr hidDeviceObject, ref Byte lpReportBuffer, Int32 reportBufferLength);

        [DllImport("hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool HidD_GetManufacturerString(IntPtr hidDeviceObject, ref Byte lpReportBuffer, Int32 reportBufferLength);

        bool HidD_GetManufacturerString_Hook(IntPtr hidDeviceObject, ref Byte lpReportBuffer, Int32 reportBufferLength)
        {
            bool result = false;

            try
            {
                result = HidD_GetManufacturerString(hidDeviceObject, ref lpReportBuffer, reportBufferLength);
            }
            catch
            {
                // swallow exceptions so that any issues caused by this code do not crash target process
            }

            return result;
        }
        #endregion

        #region HidD_GetProductString Hook
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate bool HidD_GetProductString_Delegate(IntPtr hidDeviceObject, ref Byte lpReportBuffer, Int32 reportBufferLength);

        [DllImport("hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool HidD_GetProductString(IntPtr hidDeviceObject, ref Byte lpReportBuffer, Int32 reportBufferLength);

        bool HidD_GetProductString_Hook(IntPtr hidDeviceObject, ref Byte lpReportBuffer, Int32 reportBufferLength)
        {
            bool result = false;

            try
            {
                result = HidD_GetProductString(hidDeviceObject, ref lpReportBuffer, reportBufferLength);
            }
            catch
            {
                // swallow exceptions so that any issues caused by this code do not crash target process
            }

            return result;
        }
        #endregion

        #region HidD_GetSerialNumberString Hook
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate bool HidD_GetSerialNumberString_Delegate(IntPtr hidDeviceObject, ref Byte lpReportBuffer, Int32 reportBufferLength);

        [DllImport("hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool HidD_GetSerialNumberString(IntPtr hidDeviceObject, ref Byte lpReportBuffer, Int32 reportBufferLength);

        bool HidD_GetSerialNumberString_Hook(IntPtr hidDeviceObject, ref Byte lpReportBuffer, Int32 reportBufferLength)
        {
            bool result = false;

            try
            {
                result = HidD_GetSerialNumberString(hidDeviceObject, ref lpReportBuffer, reportBufferLength);
            }
            catch
            {
                // swallow exceptions so that any issues caused by this code do not crash target process
            }

            return result;
        }
        #endregion

        #region HidP_GetCaps Hook
        [StructLayout(LayoutKind.Sequential)]
        internal struct HIDP_CAPS
        {
            public Int16 Usage;
            public Int16 UsagePage;
            public Int16 InputReportByteLength;
            public Int16 OutputReportByteLength;
            public Int16 FeatureReportByteLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
            public Int16[] Reserved;
            public Int16 NumberLinkCollectionNodes;
            public Int16 NumberInputButtonCaps;
            public Int16 NumberInputValueCaps;
            public Int16 NumberInputDataIndices;
            public Int16 NumberOutputButtonCaps;
            public Int16 NumberOutputValueCaps;
            public Int16 NumberOutputDataIndices;
            public Int16 NumberFeatureButtonCaps;
            public Int16 NumberFeatureValueCaps;
            public Int16 NumberFeatureDataIndices;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate int HidP_GetCaps_Delegate(IntPtr preparsedData, ref HIDP_CAPS capabilities);

        [DllImport("hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern int HidP_GetCaps(IntPtr preparsedData, ref HIDP_CAPS capabilities);

        int HidP_GetCaps_Hook(IntPtr preparsedData, ref HIDP_CAPS capabilities)
        {
            int result = 0;

            try
            {
                // Call original first so we get the result
                result = HidP_GetCaps(preparsedData, ref capabilities);
            }
            catch
            {
                // swallow exceptions so that any issues caused by this code do not crash target process
            }

            return result;
        }
        #endregion

        #region HidP_GetValueCaps Hook
        internal enum HIDP_REPORT_TYPE
        {
            HidP_Input,
            HidP_Output,
            HidP_Feature
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HidP_Range
        {
            public short UsageMin;
            public short UsageMax;
            public short StringMin;
            public short StringMax;
            public short DesignatorMin;
            public short DesignatorMax;
            public short DataIndexMin;
            public short DataIndexMax;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HidP_NotRange
        {
            public short Usage;
            public short Reserved1;
            public short StringIndex;
            public short Reserved2;
            public short DesignatorIndex;
            public short Reserved3;
            public short DataIndex;
            public short Reserved4;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct HidP_Value_Caps
        {
            [FieldOffset(0)]
            public ushort UsagePage;
            [FieldOffset(2)]
            public byte ReportID;
            [FieldOffset(3), MarshalAs(UnmanagedType.U1)]
            public bool IsAlias;
            [FieldOffset(4)]
            public ushort BitField;
            [FieldOffset(6)]
            public ushort LinkCollection;
            [FieldOffset(8)]
            public ushort LinkUsage;
            [FieldOffset(10)]
            public ushort LinkUsagePage;
            [FieldOffset(12), MarshalAs(UnmanagedType.U1)]
            public bool IsRange;
            [FieldOffset(13), MarshalAs(UnmanagedType.U1)]
            public bool IsStringRange;
            [FieldOffset(14), MarshalAs(UnmanagedType.U1)]
            public bool IsDesignatorRange;
            [FieldOffset(15), MarshalAs(UnmanagedType.U1)]
            public bool IsAbsolute;
            [FieldOffset(16), MarshalAs(UnmanagedType.U1)]
            public bool HasNull;
            [FieldOffset(17)]
            public byte Reserved;
            [FieldOffset(18)]
            public short BitSize;
            [FieldOffset(20)]
            public short ReportCount;
            [FieldOffset(22)]
            public ushort Reserved2a;
            [FieldOffset(24)]
            public ushort Reserved2b;
            [FieldOffset(26)]
            public ushort Reserved2c;
            [FieldOffset(28)]
            public ushort Reserved2d;
            [FieldOffset(30)]
            public ushort Reserved2e;
            [FieldOffset(32)]
            public int UnitsExp;
            [FieldOffset(36)]
            public int Units;
            [FieldOffset(40)]
            public int LogicalMin;
            [FieldOffset(44)]
            public int LogicalMax;
            [FieldOffset(48)]
            public int PhysicalMin;
            [FieldOffset(52)]
            public int PhysicalMax;

            [FieldOffset(56)]
            public HidP_Range Range;
            [FieldOffset(56)]
            public HidP_NotRange NotRange;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate int HidP_GetValueCaps_Delegate(HIDP_REPORT_TYPE reportType, ref Byte valueCaps, ref short valueCapsLength, IntPtr preparsedData);

        [DllImport("hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern int HidP_GetValueCaps(HIDP_REPORT_TYPE reportType, ref Byte valueCaps, ref short valueCapsLength, IntPtr preparsedData);

        int HidP_GetValueCaps_Hook(HIDP_REPORT_TYPE reportType, ref Byte valueCaps, ref short valueCapsLength, IntPtr preparsedData)
        {
            int result = 0;

            try
            {
                // Call original first so we get the result
                result = HidP_GetValueCaps_Hook(reportType, ref valueCaps, ref valueCapsLength, preparsedData);
            }
            catch
            {
                // swallow exceptions so that any issues caused by this code do not crash target process
            }

            return result;
        }
        #endregion
    }
}
