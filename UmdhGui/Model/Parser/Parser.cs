using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace UmdhGui.Model.Parser
{
    public class Trace
    {
        // + BYTES_DELTA ( - ) NEW_COUNT allocs BackTrace TRACEID 
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


    internal class Parser
    {
        private readonly Scanner _scanner;

        public Parser(Scanner scanner)
        {
            _scanner = scanner;
        }

        public List<Trace> Parse()
        {
            var list = new List<Trace>();


            ReadGarbage();

            Trace de;
            while ((de = ParseDiffEntry()) != null)
            {
                if (de.HasBody)
                {
                    list.Add(de);
                }
            }

            return list;
        }

        // Reads until we find the first line of this kind
        //+      64 (     64 -      0)      1 allocs	BackTrace1180950
        // This removes all non interesting header information of the diff file!
        private void ReadGarbage()
        {
            // Fine tuning
            char c;
            while ((c = _scanner.PeekChar()) != 0)
            {
                if (c == '+' || c == '-')
                {
                    return;
                }

                _scanner.ReadTextBlock(); //Garbage
            }
        }

        /// <summary>
        ///     Reads one stack item from the difference file. If there is no more null is returned.
        /// </summary>
        /// <returns></returns>
        private Trace ParseDiffEntry()
        {
            var entry = new Trace();

            // + or - (Trace file may be empty!)
            var c = _scanner.PeekChar();
            if (c != '+' && c != '-')
            {
                return null;
            }

            ParseHeader(entry);

            ParseStackTrace(entry);


            return entry;
        }

        private void ParseStackTrace(Trace entry)
        {
            // It is possible that the stack trace is empty!
            var c = _scanner.PeekChar();
            if (c == '+' || c == '-')
            {
                entry.Stack = "N.A";
                return;
            }

            // Stack is terminated by empty line
            entry.Stack = _scanner.ReadTextBlock();
        }

        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "direction")]
        private void ParseHeader(Trace entry)
        {
            // First line
            // + BYTES_DELTA (NEW_BYTES - OLD_BYTES) NEW_COUNT allocs BackTrace TRACEID 

            // ReSharper disable once NotAccessedVariable
            var direction = _scanner.ReadTextToken();

            entry.BytesDelta = _scanner.ReadNumberToken();

            var notInteresting = _scanner.ReadTextToken();
            Debug.Assert(notInteresting == "(");

            entry.NewBytes = _scanner.ReadNumberToken();

            notInteresting = _scanner.ReadTextToken();
            Debug.Assert(notInteresting == "-");

            entry.OldBytes = _scanner.ReadNumberToken();

            notInteresting = _scanner.ReadTextToken();
            Debug.Assert(notInteresting == ")");

            entry.NewCount = _scanner.ReadNumberToken();

            notInteresting = _scanner.ReadTextToken();

            //System.Diagnostics.Debug.Assert(notInteresting == "allocs");

            //Sometimes there are entries that are not valid.
            //This filters them out.
            if (notInteresting != "allocs")
            {
                entry.HasBody = false;
            }

            //Set HasBody to false
            entry.TraceId = _scanner.ReadTextToken();

            // Second line
            // + COUNT_DELTA (NEW_COUNT - OLD_COUNT) BackTrace TRACEID allocations

            var c = _scanner.PeekChar();
            if (c != '+' && c != '-')
            {
                return;
            }


            // ReSharper disable once RedundantAssignment
            direction = _scanner.ReadTextToken();

            entry.CountDelta = _scanner.ReadNumberToken();

            notInteresting = _scanner.ReadTextToken();
            Debug.Assert(notInteresting == "(");

            entry.NewCount = _scanner.ReadNumberToken();

            notInteresting = _scanner.ReadTextToken();
            Debug.Assert(notInteresting == "-");

            entry.OldCount = _scanner.ReadNumberToken();

            notInteresting = _scanner.ReadTextToken();
            Debug.Assert(notInteresting == ")");

            entry.TraceId = _scanner.ReadTextToken();

            notInteresting = _scanner.ReadTextToken();
            //System.Diagnostics.Debug.Assert(notInteresting == "allocations");
            entry.HasBody = notInteresting == "allocations";
        }
    }
}