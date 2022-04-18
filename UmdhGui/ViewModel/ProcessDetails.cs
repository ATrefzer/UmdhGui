using System.Diagnostics;
using Vanara.PInvoke;

namespace UmdhGui.ViewModel
{
    internal class ProcessDetails
    {
        public ProcessDetails(Process process)
        {
            Process = process;
            Architecture = GetArchitecture(process);
        }

        public Process Process { get; }

        public string ProcessName => Process.ProcessName;
        public int ProcessId => Process.Id;
        public string Architecture { get; }


        private string GetArchitecture(Process process)
        {
            try
            {
                if (Kernel32.IsWow64Process(process.Handle, out var isWow64Process))
                {
                    return isWow64Process ? "x86" : "x64";
                }
            }
            catch
            {
                // Likely access denied.
            }

            return "?";
        }
    }
}