using System.Collections.Generic;
using UmdhGui.Model.Parser;

namespace UmdhGui.Model
{
    internal class SnapshotDifference
    {
        public List<DiffEntry> Traces { get; set; }

        public string FilePath { get; set; }

        public string Details { get; set; }
    }
}