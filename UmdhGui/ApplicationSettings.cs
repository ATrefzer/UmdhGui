

using System.IO;
using UmdhGui.Model;
using UmdhGui.Properties;

namespace UmdhGui
{
    public class ApplicationSettings
    {
        private readonly SymbolPath _symbolPath = new SymbolPath();

        public bool IsOutputDirectoryValid
        {
            get { return Directory.Exists(OutputDirectory); }
        }

        public bool IsToolDirectoryValid
        {
            get { return Directory.Exists(ToolDirectory); }
        }

        public string PathToUmdh
        {
            get { return Path.Combine(ToolDirectory, "umdh.exe"); }
        }

        public string PathToGFlags
        {
            get { return Path.Combine(ToolDirectory, "gflags.exe"); }
        }

        public bool IsPathToUmdhValid
        {
            get { return File.Exists(PathToUmdh); }
        }

        public bool IsPathToGFlagsValid
        {
            get { return File.Exists(PathToGFlags); }
        }

        public bool IsValid
        {
            get { return IsPathToGFlagsValid && IsPathToUmdhValid && IsOutputDirectoryValid && IsSymbolPathValid; }
        }

        public string OutputDirectory { get; set; }

        public string FilterExpression { get; set; }

        /// <summary>
        ///     Property for the user interface. Changes are not written back to the symbol path.
        /// </summary>
        public string SymbolPath
        {
            get { return _symbolPath.Value; }

            set { _symbolPath.Overwrite(value); }
        }

        public bool ShowSymbolPathOverrideWarning
        {
            get { return !_symbolPath.IsFromEnvironment; }
        }


        public string ToolDirectory { get; set; }

        public bool IsSymbolPathValid
        {
            get { return _symbolPath.IsValid; }
        }

        public void Load()
        {
            FilterExpression = Settings.Default.FilterExpression;
            ToolDirectory = Settings.Default.ToolDirectory;
            OutputDirectory = Settings.Default.OutputDirectory;

            // If no value is found at all, provide a default value.
            var path = Settings.Default.SymbolPath;
            if (!_symbolPath.IsValid && string.IsNullOrEmpty(path))
                _symbolPath.Overwrite(Model.SymbolPath.GetDefaultSymbolPath());
            else
                _symbolPath.Overwrite(path);
        }

        public void Save()
        {
            Settings.Default.FilterExpression = FilterExpression;
            Settings.Default.ToolDirectory = ToolDirectory;
            Settings.Default.OutputDirectory = OutputDirectory;

            // Save even if present in environment
            Settings.Default.SymbolPath = _symbolPath.Value;
            Settings.Default.Save();
        }
    }
}