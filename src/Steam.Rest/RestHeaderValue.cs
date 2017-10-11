using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Steam.Rest
{
    /// <summary>
    /// Represents one or more header values
    /// </summary>
    public class RestHeaderValue : IEnumerable<string>
    {
        private readonly string _value;
        private readonly IReadOnlyCollection<string> _manyValues;

        /// <summary>
        /// All values in this <see cref="RestHeaderValue"/>
        /// </summary>
        public IEnumerable<string> Value
        {
            get
            {
                if (_value != null)
                {
                    yield return _value;
                }
                else if (_manyValues != null)
                {
                    foreach (var value in _manyValues)
                    {
                        yield return value;
                    }
                }
                else
                {
                    yield break;
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="RestHeaderValue"/> from the specified string
        /// </summary>
        /// <param name="value"></param>
        public RestHeaderValue(string value)
        {
            _value = value;
        }

        /// <summary>
        /// Creates a new <see cref="RestHeaderValue"/> from multiple strings
        /// </summary>
        /// <param name="values"></param>
        public RestHeaderValue(IEnumerable<string> values)
        {
            _manyValues = values.Where(v => v != null).ToList();
        }

        public RestHeaderValue(params string[] values) : this((IEnumerable<string>)values) { }

        public IEnumerator<string> GetEnumerator() => Value.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString()
        {
            if (_value != null)
                return _value;
            else
                return string.Join(",", _manyValues);
        }
    }
}
