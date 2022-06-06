using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UmdhGui.Infrastructure;
using UmdhGui.Model.Parser;

namespace UmdhGui.Model
{
    internal class SnapshotManager
    {
        private readonly object _lock = new object();
        private readonly ProcessHelper _process;
        private int _order;

        /// <summary>
        ///     The settings, not the view model!
        /// </summary>
        private readonly ApplicationSettings _settings;

        public SnapshotManager(ProcessHelper process, ApplicationSettings settings)
        {
            _settings = settings;
            _process = process;
            Snapshots = new List<Snapshot>();
        }

        public List<Snapshot> Snapshots { get; set; }


        private ProcessOutput RunProcessWithSymbolPath(string pathToExe, string arguments)
        {
            // Configure symbol path. We may override the system environment.
            var dict = new Dictionary<string, string>();
            var ntSymbolPath = "_NT_SYMBOL_PATH";
            dict.Add(ntSymbolPath, _settings.SymbolPath);

            return _process.StartAndWait(pathToExe, arguments, dict);
        }

        private static string GetSnapshotArguments(int processId, string fileName, string outputDir)
        {
            var args = "-p:";
            args = args + processId;
            args = args + " -f:\"" + Path.Combine(outputDir, fileName) + "\"";

            return args;
        }


        private static string GenerateSnapshotFileName()
        {
            var now = DateTime.Now;

            var builder = new StringBuilder(100);
            builder.Append("Snapshot_");
            builder.AppendFormat(CultureInfo.InvariantCulture, "{0:d4}", now.Year);
            builder.AppendFormat(CultureInfo.InvariantCulture, "{0:d2}", now.Month);
            builder.AppendFormat(CultureInfo.InvariantCulture, "{0:d2}", now.Day);

            builder.AppendFormat(CultureInfo.InvariantCulture, "{0:d2}", now.Hour);
            builder.AppendFormat(CultureInfo.InvariantCulture, "{0:d2}", now.Minute);
            builder.AppendFormat(CultureInfo.InvariantCulture, "{0:d2}", now.Second);

            builder.Append(".umdhsnapshot");

            return builder.ToString();
        }

        private string GetDiffArguments(string olderSnapshot, string newerSnapshot, string outputFileName)
        {
            // always decimal numbers!
            var args = "-d ";
            args = args + "\"" + Path.Combine(_settings.OutputDirectory, olderSnapshot) + "\" ";
            args = args + "\"" + Path.Combine(_settings.OutputDirectory, newerSnapshot) + "\"";


            args = args + " -f:\"" + Path.Combine(_settings.OutputDirectory, outputFileName) + "\"";

            return args;
        }

        private static string GenerateOutputFilename(string olderSnapshot, string newerSnapshot)
        {
            var outputFilename = "Diff_"
                                 + olderSnapshot.Substring(olderSnapshot.IndexOf('_') + 1, 14)
                                 + "-"
                                 + newerSnapshot.Substring(newerSnapshot.IndexOf('_') + 1, 14)
                                 + ".umdhdiff";
            return outputFilename;
        }

        public SnapshotDifference LoadDifferenceFile(string path)
        {
            List<DiffEntry> traces;
            using (var stream = File.OpenText(path))
            {
                var scanner = new Scanner(stream, false);
                var parser = new Parser.Parser(scanner);
                traces = parser.Parse();
            }

            var diff = new SnapshotDifference();
            diff.Traces = traces;
            diff.FilePath = path;
            diff.Details = "";
            return diff;
        }

        public void CleanOutputDirectory()
        {
            if (Directory.Exists(_settings.OutputDirectory))
            {
                foreach (var path in Directory.EnumerateFiles(_settings.OutputDirectory, "*.umdhdiff"))
                    File.Delete(path);

                foreach (var path in Directory.EnumerateFiles(_settings.OutputDirectory, "*.umdhsnapshot"))
                    File.Delete(path);
            }
        }

        public Task<SnapshotDifference> DiffSnapshotsAsync(Snapshot olderSnapshot, Snapshot newerSnapshot)
        {
            if (olderSnapshot == null || newerSnapshot == null)
                throw new ArgumentException("At lease one of the snapshots is null.");

            return Task.Run(() =>
            {
                var outputFileName = GenerateOutputFilename(olderSnapshot.FilePath, newerSnapshot.FilePath);
                var outputFilePath = Path.Combine(_settings.OutputDirectory, outputFileName);
                var procExe = Path.Combine(_settings.ToolDirectory, "umdh.exe");
                var arguments = GetDiffArguments(olderSnapshot.FilePath, newerSnapshot.FilePath, outputFilePath);
                var procOut = RunProcessWithSymbolPath(procExe, arguments);

                var snapshotDifference = LoadDifferenceFile(outputFilePath);
                snapshotDifference.Details = GetMessage(procOut);
                return snapshotDifference;
            });
        }

        public Task<Snapshot> CreateSnapshotAsync(int processId)
        {
            _order++;

            return Task.Run(() =>
            {
                var procExe = Path.Combine(_settings.ToolDirectory, "umdh.exe");
                var snapshotFile = GenerateSnapshotFileName();
                var arguments = GetSnapshotArguments(processId, snapshotFile, _settings.OutputDirectory);
                var procOut = RunProcessWithSymbolPath(procExe, arguments);

                var snapshot = new Snapshot
                {
                    TakenAt = DateTime.Now,
                    Order = _order,
                    Name = "Snapshot " + _order,
                    Details = GetMessage(procOut),
                    FilePath = snapshotFile
                };

                lock (_lock)
                {
                    Snapshots.Add(snapshot);
                }

                return snapshot;
            });
        }

        private static string GetMessage(ProcessOutput procOut)
        {
            return procOut.StandardOutput + "\n" + procOut.StandardError;
        }

        public void ClearSnapshots()
        {
            Snapshots.Clear();
        }
    }
}