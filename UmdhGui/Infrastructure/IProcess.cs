using System.Collections.Generic;

namespace UmdhGui.Infrastructure
{
    internal interface IProcess
    {
        ProcessOutput StartAndWait(string pathToExe, string args, IDictionary<string, string> environmentVariables);

        /// <summary>
        /// -1 = failed
        /// </summary>
        int Start(string filePath, string arguments, Dictionary<string, string> dict);
    }
}