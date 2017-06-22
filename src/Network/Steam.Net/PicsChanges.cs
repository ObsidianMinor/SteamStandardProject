using Steam.Net.Messages.Protobufs;
using Steam.Net.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace Steam.Net
{
    /// <summary>
    /// Represent changes that have occured in the PICS database between the specified change number and the most recent change number
    /// </summary>
    public class PicsChanges
    {
        public uint LastChangeNumber { get; internal set; }
        public uint CurrentChangeNumber { get; internal set; }
        public bool RequiresFullUpdate { get; private set; }

        /// <summary>
        /// Get all the changes in this 
        /// </summary>
        public IReadOnlyCollection<PicsChangeData> Changes { get; private set; }

        internal static PicsChanges Create(PicsChangesSinceResponse response)
        {
            return new PicsChanges
            {
                Changes = response.PackageChanges.Select(p => PicsChangeData.Create(p)).Concat(response.app_changes.Select(a => PicsChangeData.Create(a))).ToReadOnlyCollection(),
                CurrentChangeNumber = response.CurrentChangeNumber,
                LastChangeNumber = response.SinceChangeNumber,
                RequiresFullUpdate = response.ForceFullUpdate
            };
        }
    }

    public class PicsChangeData
    {
        /// <summary>
        /// Gets whether this change data is for an app or package
        /// </summary>
        public PicsDataType Type { get; private set; }

        /// <summary>
        /// Gets the app or package Id this change data is for
        /// </summary>
        public uint PicsId { get; private set; }
        /// <summary>
        /// Gets the current change number for this app
        /// </summary>
        public uint ChangeNumber { get; private set; }
        /// <summary>
        /// Signals if an access token is needed for this request
        /// </summary>
        public bool NeedsToken { get; private set; }

        internal static PicsChangeData Create(PicsChangesSinceResponse.AppChange app)
        {
            return new PicsChangeData
            {
                Type = PicsDataType.App,
                ChangeNumber = app.change_number,
                NeedsToken = app.needs_token,
                PicsId = app.appid
            };
        }

        internal static PicsChangeData Create(PicsChangesSinceResponse.PackageChange package)
        {
            return new PicsChangeData
            {
                Type = PicsDataType.Package,
                ChangeNumber = package.change_number,
                NeedsToken = package.needs_token,
                PicsId = package.packageid
            };
        }
    }
}
