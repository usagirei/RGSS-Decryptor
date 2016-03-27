// --------------------------------------------------
// RgssDecrypter - StringCoda.cs
// --------------------------------------------------

using System;
using System.Collections.Generic;

namespace RgssDecrypter.Options

{
    internal static class StringCoda
    {
        public static IEnumerable<string> WrappedLines(string self, params int[] widths)
        {
            IEnumerable<int> w = widths;
            return WrappedLines(self, w);
        }

        public static IEnumerable<string> WrappedLines(string self, IEnumerable<int> widths)
        {
            if (widths == null)
                throw new ArgumentNullException(nameof(widths));
            return CreateWrappedLinesIterator(self, widths);
        }

        private static IEnumerable<string> CreateWrappedLinesIterator(string self, IEnumerable<int> widths)
        {
            if (string.IsNullOrEmpty(self))
            {
                yield return string.Empty;
                yield break;
            }
            using (IEnumerator<int> ewidths = widths.GetEnumerator())
            {
                bool? hw = null;
                int width = GetNextWidth(ewidths, int.MaxValue, ref hw);
                int start = 0, end;
                do
                {
                    end = GetLineEnd(start, width, self);
                    char c = self[end - 1];
                    if (char.IsWhiteSpace(c))
                        --end;
                    bool needContinuation = end != self.Length && !IsEolChar(c);
                    string continuation = "";
                    if (needContinuation)
                    {
                        --end;
                        continuation = "-";
                    }
                    string line = self.Substring(start, end - start) + continuation;
                    yield return line;
                    start = end;
                    if (char.IsWhiteSpace(c))
                        ++start;
                    width = GetNextWidth(ewidths, width, ref hw);
                }
                while (start < self.Length);
            }
        }

        private static int GetLineEnd(int start, int length, string description)
        {
            int end = Math.Min(start + length, description.Length);
            int sep = -1;
            for (int i = start; i < end; ++i)
            {
                if (description[i] == '\n')
                    return i + 1;
                if (IsEolChar(description[i]))
                    sep = i + 1;
            }
            if (sep == -1 || end == description.Length)
                return end;
            return sep;
        }

        private static int GetNextWidth(IEnumerator<int> ewidths, int curWidth, ref bool? eValid)
        {
            if (!eValid.HasValue || eValid.Value)
            {
                curWidth = (eValid = ewidths.MoveNext()).Value ? ewidths.Current : curWidth;
                // '.' is any character, - is for a continuation
                const string MIN_WIDTH = ".-";
                if (curWidth < MIN_WIDTH.Length)
                    throw new ArgumentOutOfRangeException(nameof(ewidths),
                        $"Element must be >= {MIN_WIDTH.Length}, was {curWidth}.");
                return curWidth;
            }
            // no more elements, use the last element.
            return curWidth;
        }

        private static bool IsEolChar(char c)
        {
            return !char.IsLetterOrDigit(c);
        }
    }
}
