using System;

namespace Steam.KeyValues.Linq
{
    /// <summary>
    /// Specifies the settings used when merging KeyValue.
    /// </summary>
    public class KeyValueMergeSettings
    {
        private MergeNullValueHandling _mergeNullValueHandling;
        
        /// <summary>
        /// Gets or sets how null value properties are merged.
        /// </summary>
        /// <value>How null value properties are merged.</value>
        public MergeNullValueHandling MergeNullValueHandling
        {
            get { return _mergeNullValueHandling; }
            set
            {
                if (value < MergeNullValueHandling.Ignore || value > MergeNullValueHandling.Merge)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _mergeNullValueHandling = value;
            }
        }
    }
}
