using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Data;

namespace UmdhGui.ViewModel
{
    internal class ProcessViewModel
    {
        private string _filter;

        public ProcessViewModel(List<Process> processes)
        {
            Processes = new ListCollectionView(processes);
            Processes.Filter += FilterFunc;
        }

        private bool FilterFunc(object obj)
        {
            var process = obj as Process;
            if (process == null)
                return false;

            if (string.IsNullOrEmpty(Filter))
                return true;

            if (process.ProcessName.IndexOf(Filter, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// From code behind
        /// </summary>
        /// <summary>
        ///     All available processes. Never updated.
        /// </summary>
        public CollectionView Processes { get; set; }

        /// <summary>
        ///     Bound to selected item!
        /// </summary>
        public Process Selected { get; set; }

        public string Filter
        {
            get { return _filter; }
            set
            {
                _filter = value.Trim();
                Processes.Refresh();
            }
        }
    }
}