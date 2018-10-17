using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using UmdhGui.Infrastructure;

namespace UmdhGui.Model
{
    internal class GFlags
    {
        private readonly string _toolDirectory;
        private readonly IProcess _process;

        public GFlags(string toolDirectory, IProcess process)
        {
            _process = process;
            _toolDirectory = toolDirectory;
        }

        /// <summary>
        ///     The image name should contain the extension, like opera.exe
        /// </summary>
        public void SetTraceDbSize(string imageName, int sizeInMegs)
        {
            string args = $"/i {imageName} -tracedb {sizeInMegs}";
            var pathToExe = Path.Combine(_toolDirectory, "gflags.exe");

            _process.StartAndWait(pathToExe, args, null);

            // Ignore errors
        }

        internal void Write(string imageName, bool ustEnabled, int dbSizeInMb)
        {
            if (string.IsNullOrEmpty(imageName))
                return;

            if (ustEnabled)
                SetUstGFlag(imageName);
            else
                RemoveUstGFlag(imageName);

            SetTraceDbSize(imageName, dbSizeInMb);
        }

        public void ResetTraceDbSize(string imageName)
        {
            SetTraceDbSize(imageName, 0);
        }

        public int GetTraceDbSize(string imageName)
        {
            // Gflags writes the options in both keys 32 bit and 64 bit.
            // Since 64 bit is virtualized I use this one.
            //var path32 =
            //    @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\";

            var path64 =
                @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options";

            path64 = Path.Combine(path64, imageName);
            try
            {
                var dbSize = (int) Registry.GetValue(path64, "StackTraceDatabaseSizeInMb", null);
                return dbSize;
            }
            catch (Exception)
            {
                Debug.WriteLine("Error reading TraceDb size.");
                return 0;
            }
        }

        public bool IsUstGFlagSet(string imageName)
        {
            var pathToExe = Path.Combine(_toolDirectory, "gflags.exe");
            imageName = Path.GetFileName(imageName);
            var args = $"/i {imageName}";
            var procOut = _process.StartAndWait(pathToExe, args, null);

            // > gflags.exe /i firefox.exe
            //Current Registry Settings for firefox.exe executable are: 00001000
            //ust - Create user mode stack trace database
            var output = procOut.StandardOutput;
            var flag = output.Substring(output.IndexOf(":", StringComparison.Ordinal) + 2, 8);
            if (flag[4] == '1')
                return true;
            return false;
        }

        public void SetUstGFlag(string imagePath)
        {
            var imageName = Path.GetFileName(imagePath);
            var pathToExe = Path.Combine(_toolDirectory, "gflags.exe");
            var args = $"/i {imageName} +ust";
            _process.StartAndWait(pathToExe, args, null);
        }

        public void RemoveUstGFlag(string imagePath)
        {
            var imageName = Path.GetFileName(imagePath);
            var pathToExe = Path.Combine(_toolDirectory, "gflags.exe");
            var args = $"/i {imageName} -ust";
            _process.StartAndWait(pathToExe, args, null);
        }
    }
}