using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using UmdhGui.Infrastructure;
using UmdhGui.Model;

namespace UmdhGui.ViewModel
{
    internal class GFlagsViewModel : INotifyPropertyChanged
    {
        private readonly GFlags _gflags;
        private string _imageName = "";
        private bool _isUserModeStackTraceDbEnabled;
        private int _userModeStackTraceDbSize;

        public GFlagsViewModel(GFlags gflags, int processId)
        {
            _gflags = gflags;

            if (processId > -1)
            {
                var process = Process.GetProcessById(processId);
                _imageName = process.MainModule.ModuleName;
                RefreshOptions();
            }

            WriteCommand =
                new Command(
                    obj => { _gflags.Write(ImageName, IsUserModeStackTraceDbEnabled, UserModeStackTraceDbSize); });
        }

        public ICommand WriteCommand { get; }

        public string ImageName
        {
            get { return _imageName; }
            set
            {
                value = value.Trim();
                if (Equals(value, _imageName)) return;
                _imageName = value;
                NotifyPropertyChanged();

                RefreshOptions();
            }
        }

        public int UserModeStackTraceDbSize
        {
            get { return _userModeStackTraceDbSize; }
            set
            {
                if (value == _userModeStackTraceDbSize) return;
                _userModeStackTraceDbSize = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsUserModeStackTraceDbEnabled
        {
            get { return _isUserModeStackTraceDbEnabled; }
            set
            {
                if (value == _isUserModeStackTraceDbEnabled) return;
                _isUserModeStackTraceDbEnabled = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RefreshOptions()
        {
            if (!string.IsNullOrEmpty(ImageName))
            {
                UserModeStackTraceDbSize = _gflags.GetTraceDbSize(ImageName);
                IsUserModeStackTraceDbEnabled = _gflags.IsUstGFlagSet(ImageName);
            }
            else
            {
                UserModeStackTraceDbSize = 0;
                IsUserModeStackTraceDbEnabled = false;
            }
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}