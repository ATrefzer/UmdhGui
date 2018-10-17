using System;
using System.IO;
using System.Windows;
using UmdhGui.Infrastructure;
using UmdhGui.Model;
using UmdhGui.View;
using UmdhGui.ViewModel;
// ReSharper disable RedundantExtendsListEntry

namespace UmdhGui
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private App()
        {
        }

        private ApplicationSettings LoadSettings()
        {
            var folder = Environment.ExpandEnvironmentVariables("%APPDATA%");
            folder = Path.Combine(folder, "UmdhGui");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            var settings = new ApplicationSettings();
            settings.Load();

            Properties.Add(Constants.SettingsKey, settings);
            return settings;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            SaveSettings();
        }

        private void SaveSettings()
        {
            var settings = GetSettings();
            settings.Save();
        }

        private ApplicationSettings GetSettings()
        {
            var settings = Properties[Constants.SettingsKey] as ApplicationSettings;
            if (settings == null)
            {
                throw new InvalidDataException();
            }

            return settings;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var settings = LoadSettings();

            // Alternative to StartupUri in App.xaml
            //StartupUri = new Uri("View/MainWindow.xaml", UriKind.Relative);

            var messageBoxService = new MessageBoxService();
            var processHelper = new ProcessHelper();
            var inspectionProcess = new InspectionProcess(messageBoxService);
            var controller = new ApplicationController(inspectionProcess);
            var snapshotManager = new SnapshotManager(processHelper, settings);
            var settingsViewModel = new SettingsViewModel(settings, controller, snapshotManager);
            var vm = new MainViewModel(inspectionProcess, snapshotManager, settingsViewModel, controller);
         

            var main = new MainWindow();
            main.DataContext = vm;
            main.Show();
        }
    }
}