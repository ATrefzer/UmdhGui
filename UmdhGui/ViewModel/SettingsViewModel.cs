using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using UmdhGui.Infrastructure;
using UmdhGui.Model;
// ReSharper disable ExplicitCallerInfoArgument

namespace UmdhGui.ViewModel
{
    internal class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly ApplicationController _controller;

        private readonly ApplicationSettings _settings;

        public SettingsViewModel(ApplicationSettings settings, ApplicationController controller,
            SnapshotManager snapshotManager)
        {
            _settings = settings;
            _controller = controller;
            ChooseDirectoryCommand = new Command(ExecuteChooseDirectory);
            CleanOutputCommand = new Command(obj => snapshotManager.CleanOutputDirectory());
        }

        /// <summary>
        ///     Which directory to select is passed by the command parameter.
        /// </summary>
        public ICommand ChooseDirectoryCommand { get; }

        public ICommand CleanOutputCommand { get; private set; }

        public bool IsOutputDirectoryValid
        {
            get { return _settings.IsOutputDirectoryValid; }
        }

        public bool IsToolDirectoryValid
        {
            get { return _settings.IsToolDirectoryValid; }
        }


        public bool IsPathToUmdhValid
        {
            get { return _settings.IsPathToUmdhValid; }
        }

        public bool IsPathToGFlagsValid
        {
            get { return _settings.IsPathToGFlagsValid; }
        }

        public bool IsValid
        {
            get { return IsPathToGFlagsValid && IsPathToUmdhValid && IsOutputDirectoryValid && IsSymbolPathValid; }
        }

        public string OutputDirectory
        {
            get { return _settings.OutputDirectory; }

            set
            {
                _settings.OutputDirectory = value;
                NotifyPropertyChanged();
                NotifyValidityChanged();
            }
        }


        public string FilterExpression
        {
            get { return _settings.FilterExpression; }

            set
            {
                _settings.FilterExpression = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        ///     Property for the user interface. Changes are not written back to the symbol path.
        /// </summary>
        public string SymbolPath
        {
            get { return _settings.SymbolPath; }

            set
            {
                _settings.SymbolPath = value;

                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(ShowSymbolPathOverrideWarning));
                NotifyValidityChanged();
            }
        }

        public bool ShowSymbolPathOverrideWarning
        {
            get { return _settings.ShowSymbolPathOverrideWarning; }
        }


        public string ToolDirectory
        {
            get { return _settings.ToolDirectory; }

            set
            {
                _settings.ToolDirectory = value;
                NotifyPropertyChanged();
                NotifyValidityChanged();
            }
        }

        public bool IsSymbolPathValid
        {
            get { return _settings.IsSymbolPathValid; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }


        private void ExecuteChooseDirectory(object parameter)
        {
            var directory = _controller.ShowFolderDialog();
            if (string.IsNullOrEmpty(directory) == false)
            {
                if (Equals(parameter, Constants.SelectOutputDirectoryCommandParameter))
                    OutputDirectory = directory;
                if (Equals(parameter, Constants.SelectToolDirectoryCommandParameter))
                    ToolDirectory = directory;
            }
        }

        private void NotifyValidityChanged()
        {
            NotifyPropertyChanged(nameof(IsValid));
            NotifyPropertyChanged(nameof(IsPathToGFlagsValid));
            NotifyPropertyChanged(nameof(IsPathToUmdhValid));
            NotifyPropertyChanged(nameof(IsOutputDirectoryValid));
            NotifyPropertyChanged(nameof(IsToolDirectoryValid));
            NotifyPropertyChanged(nameof(IsSymbolPathValid));
        }
    }
}