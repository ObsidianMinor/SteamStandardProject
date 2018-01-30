using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Steam.KeyValues
{
    /// <summary>
    /// Represents a reader that provides fast, non-cached, forward-only access to KeyValue text data
    /// </summary>
    public class KeyValueTextReader : KeyValueReader, IKeyValueLineInfo
    {
        private readonly TextReader _reader;

        public KeyValueTextReader(TextReader reader)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public int LineNumber => throw new NotImplementedException();

        public int LinePosition => throw new NotImplementedException();

        public bool HasLineInfo()
        {
            throw new NotImplementedException();
        }

        public override bool Read()
        {
            throw new NotImplementedException();
        }
    }
}
