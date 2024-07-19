// PS4RemotePlayInterceptor (File: Classes/Interceptor.cs)
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

using EasyHook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Ipc;
using System.Security.Principal;
using System.Text;

namespace PS4RemotePlayInterceptor
{
    public delegate void InterceptionDelegate(ref byte[] state);

    public enum InjectionMode
    {
        Auto,
        Compatibility
    }

    public class Interceptor
    {
        #region Win32 API
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr PostMessage(IntPtr hWnd, uint msg, uint wParam, uint lParam);
        #endregion

        // Constants
        private const string INJECT_DLL_NAME = "PS4RemotePlayInterceptor.dll";
        private const string INJECT_APP_NAME = @"C:\Program Files (x86)\Sony\PS Remote Play\RemotePlay.exe";

        // EasyHook
        private static string _channelName = null;
        private static IpcServerChannel _ipcServer;
        private static bool _noGAC = false;

        public static DateTime LastPingTime { get; set; }

        // Injection
        public static InjectionMode InjectionMode = InjectionMode.Auto;

        // Delegate
        public static InterceptionDelegate Callback { get; set; }

        public static int Inject()
        {
            // Full path to our dll file
            string injectionLibrary = Path.Combine(Path.GetDirectoryName(typeof(InjectionInterface).Assembly.Location), INJECT_DLL_NAME);

            try
            {
                bool shouldInject = false;

                if (InjectionMode == InjectionMode.Auto)
                {
                    if (_ipcServer == null)
                    {
                        // Setup remote hooking
                        _channelName = DateTime.Now.ToString();
                        _ipcServer = RemoteHooking.IpcCreateServer<InjectionInterface>(ref _channelName, WellKnownObjectMode.Singleton, WellKnownSidType.WorldSid);
                        shouldInject = true;
                    }
                }
                else if (InjectionMode == InjectionMode.Compatibility)
                {
                    // Setup remote hooking
                    _channelName = null;
                    _ipcServer = RemoteHooking.IpcCreateServer<InjectionInterface>(ref _channelName, WellKnownObjectMode.Singleton);
                    shouldInject = true;
                }

                // Inject dll into the process
                int processId = -1;
                if (shouldInject)
                {
                    RemoteHooking.CreateAndInject(
                        INJECT_APP_NAME, // executable to run
                        "", // command line arguments for target
                        0, // additional process creation flags to pass to CreateProcess
                        (_noGAC ? InjectionOptions.DoNotRequireStrongName : InjectionOptions.Default),
                        // if not using GAC allow assembly without strong name
                        injectionLibrary, // 32-bit version (the same because AnyCPU)
                        injectionLibrary, // 64-bit version (the same because AnyCPU)
                        out processId, // retrieve the newly created process ID
                        _channelName
                    );
                }

                // Success
                return processId;
            }
            catch (Exception ex)
            {
                throw new InterceptorException(string.Format("Failed to inject to target: {0}", ex.Message), ex);
            }
        }

        public static void StopInjection()
        {
            if (_ipcServer == null)
                return;

            _ipcServer.StopListening(null);
            _ipcServer = null;
        }
    }
}
