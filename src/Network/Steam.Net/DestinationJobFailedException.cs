using Steam.Common;
using System;

namespace Steam.Net
{
    /// <summary>
    /// The exception that is used to communicate a remote job has failed
    /// </summary>
    public class DestinationJobFailedException : Exception
    {
        public SteamGuid JobId { get; }

        public DestinationJobFailedException(SteamGuid job) : base($"The destination job ({job.ToUInt64()}) failed unexpectedly")
        {
            JobId = job;
        }
    }
}
