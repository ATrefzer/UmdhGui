using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using UmdhGui.Infrastructure;

namespace UmdhGui.ViewModel
{
    internal class RunProcessViewModel : INotifyPropertyChanged
    {
        private readonly ApplicationController _controller;
        private readonly InspectionProcess _inspectionProcess;

        private readonly IProcess _process;

        private string _arguments;

        private string _filePath;

        public RunProcessViewModel(IProcess process, ApplicationController controller, InspectionProcess inspectionProcess)
        {
            _controller = controller;
            _inspectionProcess = inspectionProcess;
            _process = process;
            RunProcessCommand = new Command(ExecuteRunProcess, obj => IsValid);
            SelectExecutableCommand = new Command(param => SelectExecutable());
        }

        public Command RunProcessCommand { get; }
        public ICommand SelectExecutableCommand { get; }

        public string FilePath
        {
            get { return _filePath; }

            set
            {
                _filePath = value.Trim();
                NotifyPropertyChanged();

                // STYLE? Ok?
                RunProcessCommand.Refresh();
            }
        }

        public string Arguments
        {
            get { return _arguments; }

            set
            {
                _arguments = value.Trim();
                NotifyPropertyChanged();
            }
        }

        private bool IsValid
        {
            get { return File.Exists(FilePath); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void SelectExecutable()
        {
            var path = _controller.ShowFileDialog("", "Executable|*.exe");
            if (!string.IsNullOrEmpty(path))
                FilePath = path;
        }

        private void ExecuteRunProcess(object param)
        {
            if (IsValid)
            {
                var dict = new Dictionary<string, string>();
                dict.Add(Constants.OaNoCache, "1");
                var id = _process.Start(FilePath, Arguments, dict);

                if (id > 0)
                {
                    _inspectionProcess.SetProcess(Process.GetProcessById(id));
                    var wnd = param as Window;
                    wnd?.Close();
                }
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}