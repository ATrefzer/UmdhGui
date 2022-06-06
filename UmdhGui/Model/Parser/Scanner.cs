using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace UmdhGui.Model.Parser
{
    /// <summary>
    ///     Splits UMDH diff file into tokens
    /// </summary>
    internal class Scanner
    {
        private readonly StreamReader _inputStream;
        private readonly bool _parseOutputAsHex;

        public Scanner(StreamReader inputStream, bool parseOutputAsHex)
        {
            _inputStream = inputStream;
            _parseOutputAsHex = parseOutputAsHex;
        }

        private void EatSpaces()
        {
            while (!_inputStream.EndOfStream && IsWhiteSpace(_inputStream.Peek()))
            {
                _inputStream.Read();
            }
        }

        private static bool IsHexDigit(char c)
        {
            if (char.IsDigit(c))
            {
                return true;
            }

            var ch = char.ToUpper(c, CultureInfo.InvariantCulture);
            return ch >= 'A' && ch <= 'F';
        }

        private static bool IsWhiteSpace(int c)
        {
            switch (c)
            {
                case 9:
                case 10:
                case 13:
                case 0x20:
                    return true;
            }

            return false;
        }

        public char PeekChar()
        {
            EatSpaces();
            if (_inputStream.EndOfStream)
            {
                return '\0';
            }

            return (char)_inputStream.Peek();
        }

        public int ReadNumberToken()
        {
            EatSpaces();
            if (_inputStream.EndOfStream)
            {
                throw new EndOfStreamException();
            }

            var builder = new StringBuilder(100);
            while (!_inputStream.EndOfStream && IsHexDigit((char)_inputStream.Peek()))
            {
                builder.Append((char)_inputStream.Read());
            }

            var fromBase = 10;
            if (_parseOutputAsHex)
            {
                fromBase = 0x10;
            }

            return Convert.ToInt32(builder.ToString(), fromBase);
        }

        public string ReadTextBlock()
        {
            EatSpaces();
            var builder = new StringBuilder(100);
            for (var str = _inputStream.ReadLine(); str != null && str.Trim().Length > 0; str = _inputStream.ReadLine())
            {
                builder.AppendLine(str.Trim());
            }

            return builder.ToString();
        }

        public string ReadTextToken()
        {
            EatSpaces();
            if (_inputStream.EndOfStream)
            {
                throw new EndOfStreamException();
            }

            var builder = new StringBuilder(100);
            while (!_inputStream.EndOfStream && !IsWhiteSpace(_inputStream.Peek()))
            {
                builder.Append((char)_inputStream.Read());
            }

            return builder.ToString();
        }

        public int ReadDirectionToken()
        {
            EatSpaces();
            if (_inputStream.EndOfStream)
            {
                throw new EndOfStreamException();
            }

            var direction = (char)_inputStream.Read();

            if (direction != '-' && direction != '+')
            {
                throw new InvalidOperationException("Parse error");
            }

            var result = 1;
            if (direction == '-')
            {
                result = -1;
            }

            return result;
        }
    }
}