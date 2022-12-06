// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriberLoadTest.TestConsole.Clients.Common
{
    using System;
    using static System.Net.Mime.MediaTypeNames;

    /// <summary>
    /// A helper class to write text to the console in a consistant manner.
    /// </summary>
    public class ReportWriter
    {
        private int _maxLinesWritten;
        private int _maxLinesWrittenThisPass;

        /// <summary>
        /// Begins a new report (moves console cursor).
        /// </summary>
        public void BeginNew()
        {
            Console.SetCursorPosition(0, 0);
            _maxLinesWrittenThisPass = 0;
        }

        /// <summary>
        /// Writes the specified text to the currnet cursor position.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="color">The color to write in.</param>
        public void Write(string text, ConsoleColor? color = null)
        {
            text = text ?? string.Empty;

            ConsoleColor currentColor = Console.ForegroundColor;
            if (color.HasValue)
                Console.ForegroundColor = color.Value;

            Console.Write(text);

            Console.ForegroundColor = currentColor;
        }

        /// <summary>
        /// Writes the specified text to the current cursor position.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="totalWidth">The total width of the text towrite.</param>
        /// <param name="padChar">The padding character to write to the left of the text.</param>
        /// <param name="color">The color to write in.</param>
        public void WritePadLeft(string text, int totalWidth, char padChar = ' ', ConsoleColor? color = null)
        {
            this.WritePadded(text, true, totalWidth, padChar, color);
        }

        /// <summary>
        /// Writes the specified text to the current cursor position.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="totalWidth">The total width of the text towrite.</param>
        /// <param name="padChar">The padding character to write to the right of the text.</param>
        /// <param name="color">The color to write in.</param>
        public void WritePadRight(string text, int totalWidth, char padChar = ' ', ConsoleColor? color = null)
        {
            this.WritePadded(text, false, totalWidth, padChar, color);
        }

        private void WritePadded(
            string text,
            bool padToLeft,
            int totalWidth,
            char padChar = ' ',
            ConsoleColor? color = null)
        {
            text = text ?? string.Empty;
            if (padToLeft)
                text = text.PadLeft(totalWidth, padChar);
            else
                text = text.PadRight(totalWidth, padChar);

            this.Write(text, color);
        }

        /// <summary>
        /// Writes a fully line of text.
        /// </summary>
        /// <param name="text">The text to write.</param>
        /// <param name="color">The color to write in.</param>
        public void WriteLine(string text = "", ConsoleColor? color = null)
        {
            text = text ?? string.Empty;

            this.Write(text.PadRight(Console.BufferWidth - Console.CursorLeft, ' '), color);
            Console.WriteLine();
            _maxLinesWrittenThisPass++;

            if (_maxLinesWrittenThisPass > _maxLinesWritten)
                _maxLinesWritten = _maxLinesWrittenThisPass;
        }

        /// <summary>
        /// Clears the remainder of the print area.
        /// </summary>
        public void ClearRemaining()
        {
            for (var i = _maxLinesWrittenThisPass; i < _maxLinesWritten; i++)
            {
                Console.WriteLine(string.Empty.PadRight(Console.BufferWidth, ' '));
            }
        }
    }
}