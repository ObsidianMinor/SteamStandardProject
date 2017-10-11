using System;

namespace Steam.Net
{
    /// <summary>
    /// The exception that is used to communicate a remote job has failed
    /// </summary>
    public class DestinationJobFailedException : Exception
    {
        public SteamGid JobId { get; }

        public DestinationJobFailedException(SteamGid job) : base($"The destination job ({job.ToUInt64()}) failed unexpectedly")
        {
            JobId = job;
        }
    }
}
