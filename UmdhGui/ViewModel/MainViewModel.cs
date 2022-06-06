using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using UmdhGui.Infrastructure;
using UmdhGui.Model;
using UmdhGui.Model.Parser;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace UmdhGui.ViewModel
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        private CollectionView _collectionView;
        private string _details;
        private Snapshot _firstSnapshot;


        private bool _isUmdhActive;
        private Snapshot _secondSnapshot;
        private DiffEntry _selectedDiffEntry;
        private List<Snapshot> _snapshots;

        public MainViewModel(InspectionProcess inspectionProcess, SnapshotManager snapshotManager, SettingsViewModel settingsViewModel,
            ApplicationController controller)
        {
            InspectionProcess = inspectionProcess;
            SnapshotManager = snapshotManager;
            Settings = settingsViewModel;
            ApplicationController = controller;

            InspectionProcess.ProcessChanged += InspectionProcessOnProcessChanged;

            TakeSnapshotCommand = Command.CreateWithRegistration(obj => ExecuteTakeSnapshot(),
                obj => { return IsUmdhActive == false && InspectionProcess.IsRunning; });

            CompareSnapshotsCommand = Command.CreateWithRegistration(obj => ExecuteCompareSnapshots(),
                obj => { return IsUmdhActive == false && FirstSnapshot != null && SecondSnapshot != null; });

            ConfigureGFlagsCommand = new Command(obj => ExecuteConfigureGFlags());
            SettingsCommand = new Command(obj => ExecuteSettings());
            SelectProcessCommand = new Command(obj => ExecuteSelectProcess());
            LoadDiffFileCommand = new Command(obj => ExecuteLoadDiffFile());


            StartProcessCommand = new Command(obj => ApplicationController.ShowRunProcessDialog());
        }


        public bool IsUmdhActive
        {
            get => _isUmdhActive;

            set
            {
                _isUmdhActive = value;
                NotifyPropertyChanged();

                Command.RefreshAll();
            }
        }

        public ICommand SelectProcessCommand { get; }
        public ICommand SettingsCommand { get; }
        public ICommand LoadDiffFileCommand { get; }
        public ICommand StartProcessCommand { get; }

        public CollectionView TracesView
        {
            get => _collectionView;

            set
            {
                if (_collectionView != null)
                {
                    // ReSharper disable once DelegateSubtraction
                    _collectionView.Filter -= FilterFunc;
                }

                _collectionView = value;
                NotifyPropertyChanged();
            }
        }

        public List<Snapshot> Snapshots
        {
            get => _snapshots;
            set
            {
                _snapshots = value;
                NotifyPropertyChanged();
            }
        }

        public Snapshot FirstSnapshot
        {
            get => _firstSnapshot;
            set
            {
                _firstSnapshot = value;
                NotifyPropertyChanged();
            }
        }


        public string FilterExpression
        {
            get => Settings.FilterExpression;
            set
            {
                // See TextBox_KeyDown in MainWindow to see why this property is updated.

                // Triggered on enter in code behind.
                Settings.FilterExpression = value.Trim();
                NotifyPropertyChanged();
                TracesView?.Refresh();
            }
        }

        public Snapshot SecondSnapshot
        {
            get => _secondSnapshot;
            set
            {
                _secondSnapshot = value;
                NotifyPropertyChanged();
            }
        }


        public ICommand TakeSnapshotCommand { get; }
        public ICommand ConfigureGFlagsCommand { get; }
        public ICommand CompareSnapshotsCommand { get; }

        public SettingsViewModel Settings { get; }

        /// <summary>
        ///     This is the view model.
        /// </summary>
        public InspectionProcess InspectionProcess { get; }

        public SnapshotManager SnapshotManager { get; }

        /// <summary>
        ///     This field shows outputs from umdh process and the stack taces once a trace is selected.
        /// </summary>
        public string Details
        {
            get => _details;

            set
            {
                _details = value;
                NotifyPropertyChanged();
            }
        }

        public ApplicationController ApplicationController { get; }

        public DiffEntry SelectedDiffEntry
        {
            get => _selectedDiffEntry;

            set
            {
                _selectedDiffEntry = value;
                NotifyPropertyChanged();

                Details = _selectedDiffEntry?.Stack;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void InspectionProcessOnProcessChanged(object sender, EventArgs eventArgs)
        {
            TracesView = null;
            Details = "";
            SnapshotManager.ClearSnapshots();
            FirstSnapshot = null;
            SecondSnapshot = null;

            Command.RefreshAll();
        }

        private bool FilterFunc(object obj)
        {
            // For the CollectionView showing the traces.
            var trace = obj as DiffEntry;
            if (trace == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(FilterExpression))
            {
                return true;
            }

            var tokens = FilterExpression.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList();

            var exclusive = tokens
                .Where(t => t.StartsWith("--"))
                .Select(t => t.Substring(2))
                .Where(t => string.IsNullOrEmpty(t) is false)
                .ToList();

            var inclusive = tokens.Where(t => t.StartsWith("--") is false).ToList();

            return CheckAtLeastOneInclusivePatternMatches(trace, inclusive) &&
                   CheckNoExclusivePatternMatches(trace, exclusive);
        }

        static bool CheckAtLeastOneInclusivePatternMatches(DiffEntry diffEntry, IReadOnlyCollection<string> inclusive)
        {
            if (inclusive.Count == 0)
            {
                return true;
            }

            return inclusive.Any(token => diffEntry.Stack.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        static bool CheckNoExclusivePatternMatches(DiffEntry diffEntry, IReadOnlyCollection<string> exclusive)
        {
            if (exclusive.Count == 0)
            {
                return true;
            }

            return exclusive.All(token => diffEntry.Stack.IndexOf(token, StringComparison.OrdinalIgnoreCase) < 0);
        }


        private void ExecuteSelectProcess()
        {
            var process = ApplicationController.ShowProcessDialog();
            if (process != null)
            {
                InspectionProcess.SetProcess(process);
            }
        }

        private void ExecuteSettings()
        {
            ApplicationController.ShowSettingsDialog(Settings);
        }

        private void ExecuteConfigureGFlags()
        {
            ApplicationController.ShowGFlagsDialog(InspectionProcess.ProcessId);
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateAvailableSnapshots()
        {
            Snapshots = new List<Snapshot>(SnapshotManager.Snapshots);
            FirstSnapshot = Snapshots.FirstOrDefault();
            SecondSnapshot = Snapshots.LastOrDefault();
        }

        private void ExecuteLoadDiffFile()
        {
            var filePath = ApplicationController.ShowFileDialog(Settings.OutputDirectory, "Diff|*.umdhdiff");
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            var diff = SnapshotManager.LoadDifferenceFile(filePath);
            ApplyDiff(diff);
        }

        private async void ExecuteCompareSnapshots()
        {
            try
            {
                IsUmdhActive = true;
                if (FirstSnapshot == null || SecondSnapshot == null)
                {
                    return;
                }

                var diff = await SnapshotManager.DiffSnapshotsAsync(FirstSnapshot, SecondSnapshot);

                ApplyDiff(diff);
            }
            finally
            {
                IsUmdhActive = false;
            }
        }

        private void ApplyDiff(SnapshotDifference diff)
        {
            // Show errors if any.
            Details = diff.Details;

            // Set IsSynchronizedWithCurrentItem on DataGrid. Otherwise the first item of the collection view gets automatically
            // selected. In this app the detail window shows also the error messages from the UMDH process.

            var collectionView = new ListCollectionView(diff.Traces);
            collectionView.Filter += FilterFunc;
            TracesView = collectionView;
        }

        private async void ExecuteTakeSnapshot()
        {
            IsUmdhActive = true;

            try
            {
                var snapshot = await SnapshotManager.CreateSnapshotAsync(InspectionProcess.ProcessId);
                Details = snapshot.Details;
                UpdateAvailableSnapshots();
            }
            finally
            {
                IsUmdhActive = false;
            }
        }
    }
}