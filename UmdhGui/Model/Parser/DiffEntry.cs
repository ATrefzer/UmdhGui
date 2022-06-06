using System.Diagnostics.CodeAnalysis;

namespace UmdhGui.Model.Parser
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class DiffEntry
    {
        // + BYTES_DELTA (NEW_BYTES - OLD_BYTES) NEW_COUNT allocs BackTrace TRACEID 
        // + COUNT_DELTA (NEW_COUNT - OLD_COUNT) BackTrace TRACEID allocations
        //     ... stack trace ...    

        public string TraceId { get; set; }

        public int CountDelta { get; set; }

        public int NewCount { get; set; }

        public int OldCount { get; set; }

        public int BytesDelta { get; set; }

        public int NewBytes { get; set; }

        public int OldBytes { get; set; }

        public string Stack { get; set; }

        public bool HasBody { get; set; }
    }
}