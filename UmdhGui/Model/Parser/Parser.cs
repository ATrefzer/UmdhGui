using System.Collections.Generic;
using System.Diagnostics;

namespace UmdhGui.Model.Parser
{
    internal class Parser
    {
        private readonly Scanner _scanner;

        public Parser(Scanner scanner)
        {
            _scanner = scanner;
        }

        public List<DiffEntry> Parse()
        {
            var entries = new List<DiffEntry>();


            ReadGarbage();

            DiffEntry de;
            while ((de = ParseDiffEntry()) != null)
            {
                if (de.HasBody)
                {
                    entries.Add(de);
                }
            }


            if (entries.Count == 0)
            {
                // Dummy entry
                entries.Add(new DiffEntry());
            }

            return entries;
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
        private DiffEntry ParseDiffEntry()
        {
            var entry = new DiffEntry();

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

        private void ParseStackTrace(DiffEntry entry)
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

        private void ParseHeader(DiffEntry entry)
        {
            // First line
            // + BYTES_DELTA (NEW_BYTES - OLD_BYTES) NEW_COUNT allocs BackTrace TRACEID 

            var direction = _scanner.ReadDirectionToken();

            entry.BytesDelta = _scanner.ReadNumberToken() * direction;

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


            direction = _scanner.ReadDirectionToken();

            entry.CountDelta = _scanner.ReadNumberToken() * direction;

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
            entry.HasBody = notInteresting == "allocations";
        }
    }
}