using System.Collections.Generic;
using System.Diagnostics;

namespace UmdhGui.Infrastructure
{
    internal class ProcessHelper : IProcess
    {
        public ProcessOutput StartAndWait(string pathToExe, string args,
            IDictionary<string, string> environmentVariables)
        {
            ProcessOutput procOut;
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    // Needed to pass enviromnent variables.
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    FileName = pathToExe,
                    Arguments = args,
                    CreateNoWindow = true
                };

                // Patch environment variables
                if (environmentVariables != null)
                    foreach (var variable in environmentVariables)
                        process.StartInfo.EnvironmentVariables[variable.Key] = variable.Value;

                process.Start();

                var errorOutput = process.StandardError.ReadToEnd();
                var standardOutput = process.StandardOutput.ReadToEnd();

                procOut = new ProcessOutput
                {
                    StandardError = errorOutput,
                    StandardOutput = standardOutput
                };

                process.WaitForExit();
            }

            return procOut;
        }

        public int Start(string pathToExe, string args, Dictionary<string, string> environmentVariables)
        {
            int processId = -1;
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    // Needed to pass enviromnent variables.
                    UseShellExecute = false,
                    FileName = pathToExe,
                    Arguments = args
                };

                // Patch environment variables
                if (environmentVariables != null)
                    foreach (var variable in environmentVariables)
                        process.StartInfo.EnvironmentVariables[variable.Key] = variable.Value;

                if (process.Start())
                {
                    processId = process.Id;
                }

            }
            return processId;
        }
    }
}