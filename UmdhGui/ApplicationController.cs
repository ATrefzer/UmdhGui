using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using UmdhGui.Infrastructure;
using UmdhGui.Model;
using UmdhGui.View;
using UmdhGui.ViewModel;

namespace UmdhGui
{
    internal class ApplicationController
    {
        private readonly InspectionProcess _inspectionProcess;

        public ApplicationController(InspectionProcess inspectionProcess)
        {
            _inspectionProcess = inspectionProcess;
        }

        public string ShowFolderDialog(string startDir)
        {
            var path = string.Empty;
            using (var dlg = new CommonOpenFileDialog())
            {
                if (string.IsNullOrEmpty(startDir) is false)
                {
                    dlg.InitialDirectory = startDir;
                }
                dlg.IsFolderPicker = true;
                dlg.Multiselect = false;
                if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    path = dlg.FileName;
                }
            }

            return path;
        }

        public void ShowSettingsDialog(SettingsViewModel settings)
        {
            var dlg = new SettingsWindow();
            dlg.DataContext = settings;
            dlg.Owner = Application.Current.MainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.ShowDialog();
        }

    

        public Process ShowProcessDialog()
        {
            var dlg = new ProcessWindow();

            var processes = Process.GetProcesses().OrderBy(proc => proc.ProcessName).Select(p => new ProcessDetails(p)).ToList();

            var vm = new ProcessViewModel(processes);
            dlg.DataContext = vm;
            dlg.Owner = Application.Current.MainWindow;
            var result = dlg.ShowDialog();
            if (result == true && vm.Selected != null)
            {
                return vm.Selected.Process;
            }


            return null;
        }

        public void ShowGFlagsDialog(int processId)
        {
            var messageBox = new MessageBoxService();
            try
            {
                var settings = Application.Current.Properties[Constants.SettingsKey] as ApplicationSettings;
                if (settings == null)
                {
                    return;
                }

                var dlg = new GFlagsWindow();
                var vm = new GFlagsViewModel(new GFlags(settings.ToolDirectory, new ProcessHelper()), processId);

                dlg.Owner = Application.Current.MainWindow;
                dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                dlg.DataContext = vm;
                dlg.ShowDialog();
            }
            catch (Exception ex)
            {
                messageBox.ShowError(ex.Message, "");
            }
        }

        public string ShowFileDialog(string initDir, string filter)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.CheckFileExists = true;
            openFileDialog.Multiselect = false;
            openFileDialog.InitialDirectory = initDir;
            openFileDialog.Filter = filter;
            if (openFileDialog.ShowDialog() == false)
            {
                return null;
            }

            return openFileDialog.FileName;
        }

        internal void ShowRunProcessDialog()
        {
            var dlg = new RunProcessWindow();
            dlg.Owner = Application.Current.MainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dlg.DataContext = new RunProcessViewModel(new ProcessHelper(), this, _inspectionProcess);
            dlg.ShowDialog();
        }
    }
}