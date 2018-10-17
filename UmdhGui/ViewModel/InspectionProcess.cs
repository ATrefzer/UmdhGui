using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using UmdhGui.View;
// ReSharper disable ExplicitCallerInfoArgument

namespace UmdhGui.ViewModel
{
    internal class InspectionProcess : INotifyPropertyChanged
    {
        private readonly MessageBoxService _messageBox;
        private Process _process;
        private Dispatcher _dispatcher;

        public event EventHandler<EventArgs> ProcessChanged; 

        public InspectionProcess(MessageBoxService messageBox)
        {
            _messageBox = messageBox;
            _dispatcher = Dispatcher.CurrentDispatcher;
            SetProcess(null);
        }

        public bool IsRunning { get; private set; }

        public int ProcessId { get; set; }

        public bool IsUserModeStackTraceDbEnabled { get; set; }

        public int UserModeStackTraceDbSize { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void SetProcess(Process process)
        {
            if (process == null)
                Clear();
            else
                Set(process);

            NotifyPropertyChanged(nameof(ProcessId));
            NotifyPropertyChanged(nameof(IsRunning));


            OnProcessChanged();
        }

        private void Set(Process process)
        {
            try
            {
                _process = process;
                _process.EnableRaisingEvents = true;
                ProcessId = process.Id;

                _process.Exited += OnProcessExited;

                IsRunning = !process.HasExited;
            }
            catch (Exception ex)
            {
                var message = string.Format(CultureInfo.InvariantCulture, Strings.ErrorSelectProcess, ex.Message);
                var caption = Strings.ProcessCaption;

                _messageBox.ShowError(message, caption);
                Clear();
            }
        }

        private void Clear()
        {
            if (_process != null)
                _process.Exited -= OnProcessExited;

            _process = null;
            ProcessId = -1;
            IsRunning = false;
        }

        private void OnProcessExited(object sender, EventArgs e)
        {
           _dispatcher.Invoke(DispatcherPriority.Normal, 
                new Action(() =>
                {
                    SetProcess(null);
                }));
           
        }


        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void OnProcessChanged()
        {
            ProcessChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}