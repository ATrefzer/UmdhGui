using System;

namespace UmdhGui.Model
{
    internal class Snapshot
    {
        public int Order { get; set; }
        public string FilePath { get; set; }
        public string Details { get; set; }
        public DateTime TakenAt { get; set; }
        public string Name { get; set; }
        public bool IsValid { get; set; }
    }
}