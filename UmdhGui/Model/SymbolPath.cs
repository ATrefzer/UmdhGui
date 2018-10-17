using System;
using System.IO;

namespace UmdhGui.Model
{
    internal class SymbolPath
    {
        public SymbolPath()
        {
            Value = GetNtSymbolPathFromEnvironment();
            IsFromEnvironment = true;
        }

        public string Value { get; private set; }

        public bool IsFromEnvironment { get; set; }

        public bool IsValid
        {
            get { return string.IsNullOrEmpty(Value) == false; }
        }

        /// <summary>
        ///     cache*"%windir\symbols";srv*http://msdl.microsoft.com/download/symbols
        ///     Note:
        ///     Using a default directory does not work properly
        ///     cache*;srv*http://msdl.microsoft.com/download/symbols
        /// </summary>
        public static string GetDefaultSymbolPath()
        {
            var windir = Environment.ExpandEnvironmentVariables("%windir%");
            var localCache = Path.Combine(windir, "Symbols");
            var symbolPath = $"cache*\"{localCache}\";srv*http://msdl.microsoft.com/download/symbols;";
            return symbolPath;
        }

        public string GetNtSymbolPathFromEnvironment()
        {
            var ntsymbolpath = Environment.GetEnvironmentVariable("_NT_SYMBOL_PATH", EnvironmentVariableTarget.Machine);
            if (ntsymbolpath == null) ntsymbolpath = "";
            return ntsymbolpath;
        }

        public void SetNtSymbolPathInvironment()
        {
            Environment.SetEnvironmentVariable("_NT_SYMBOL_PATH", Value, EnvironmentVariableTarget.Machine);
        }

        internal void Overwrite(string symbolPath)
        {
            Value = symbolPath;
            IsFromEnvironment = symbolPath == GetNtSymbolPathFromEnvironment();
        }
    }
}