using System;

namespace Steam.KeyValues.Linq
{
    /// <summary>
    /// Specifies the settings used when loading KeyValue.
    /// </summary>
    public class KeyValueLoadSettings
    {
        private CommentHandling _commentHandling;
        private LineInfoHandling _lineInfoHandling;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueLoadSettings"/> class.
        /// </summary>
        public KeyValueLoadSettings()
        {
            _lineInfoHandling = LineInfoHandling.Load;
            _commentHandling = CommentHandling.Ignore;
        }

        /// <summary>
        /// Gets or sets how KeyValue comments are handled when loading KeyValue.
        /// </summary>
        /// <value>The KeyValue comment handling.</value>
        public CommentHandling CommentHandling
        {
            get { return _commentHandling; }
            set
            {
                if (value < CommentHandling.Ignore || value > CommentHandling.Load)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _commentHandling = value;
            }
        }

        /// <summary>
        /// Gets or sets how KeyValue line info is handled when loading KeyValue.
        /// </summary>
        /// <value>The KeyValue line info handling.</value>
        public LineInfoHandling LineInfoHandling
        {
            get { return _lineInfoHandling; }
            set
            {
                if (value < LineInfoHandling.Ignore || value > LineInfoHandling.Load)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _lineInfoHandling = value;
            }
        }
    }
}
