using Steam.KeyValues.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Steam.KeyValues
{
    internal enum KeyValueContainerType
    {
        None = 0,
        Object = 1,
    }

    internal struct KeyValuePosition
    {
        private static readonly char[] SpecialCharacters = { '.', ' ', '[', ']', '(', ')' };

        internal KeyValueContainerType Type;
        internal int Position;
        internal string PropertyName;
        internal bool HasIndex;

        public KeyValuePosition(KeyValueContainerType type)
        {
            Type = type;
            HasIndex = TypeHasIndex(type);
            Position = -1;
            PropertyName = null;
        }

        internal int CalculateLength()
        {
            switch (Type)
            {
                case KeyValueContainerType.Object:
                    return PropertyName.Length + 4;
                default:
                    throw new ArgumentOutOfRangeException("Type");
            }
        }

        internal void WriteTo(StringBuilder sb)
        {
            switch (Type)
            {
                case KeyValueContainerType.Object:
                    string propertyName = PropertyName;
                    if (propertyName.IndexOfAny(SpecialCharacters) != -1)
                    {
                        sb.Append(@"['");
                        sb.Append(propertyName);
                        sb.Append(@"']");
                    }
                    else
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append('.');
                        }

                        sb.Append(propertyName);
                    }
                    break;
            }
        }

        internal static bool TypeHasIndex(KeyValueContainerType type)
        {
            return false;
        }

        internal static string BuildPath(List<KeyValuePosition> positions, KeyValuePosition? currentPosition)
        {
            int capacity = 0;
            if (positions != null)
            {
                for (int i = 0; i < positions.Count; i++)
                {
                    capacity += positions[i].CalculateLength();
                }
            }
            if (currentPosition != null)
            {
                capacity += currentPosition.GetValueOrDefault().CalculateLength();
            }

            StringBuilder sb = new StringBuilder(capacity);
            if (positions != null)
            {
                foreach (KeyValuePosition state in positions)
                {
                    state.WriteTo(sb);
                }
            }
            if (currentPosition != null)
            {
                currentPosition.GetValueOrDefault().WriteTo(sb);
            }

            return sb.ToString();
        }

        internal static string FormatMessage(IKeyValueLineInfo lineInfo, string path, string message)
        {
            // don't add a fullstop and space when message ends with a new line
            if (!message.EndsWith(Environment.NewLine, StringComparison.Ordinal))
            {
                message = message.Trim();

                if (!message.EndsWith('.'))
                {
                    message += ".";
                }

                message += " ";
            }

            message += "Path '{0}'".FormatWith(CultureInfo.InvariantCulture, path);

            if (lineInfo != null && lineInfo.HasLineInfo())
            {
                message += ", line {0}, position {1}".FormatWith(CultureInfo.InvariantCulture, lineInfo.LineNumber, lineInfo.LinePosition);
            }

            message += ".";

            return message;
        }
    }
}
