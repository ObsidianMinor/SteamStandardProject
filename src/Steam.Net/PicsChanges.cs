using Steam.Net.Messages.Protobufs;
using System.Collections.Generic;
using System.Linq;

namespace Steam.Net
{
    public class PicsChanges
    {
        private PicsChanges(uint currentChange, uint providedChange, IReadOnlyCollection<AppChange> appChanges, IReadOnlyCollection<PackageChange> packageChanges, bool forceFullUpdate, bool forceFullAppUpdate, bool forceFullPackageUpdate)
        {
            CurrentChangeNumber = currentChange;
            SinceChangeNumber = providedChange;
            AppChanges = appChanges;
            PackageChanges = packageChanges;
            ForceFullUpdate = forceFullUpdate;
            ForceFullAppUpdate = forceFullAppUpdate;
            ForceFullPackageUpdate = forceFullPackageUpdate;
        }

        public long CurrentChangeNumber { get; }

        public long SinceChangeNumber { get; }

        public IReadOnlyCollection<AppChange> AppChanges { get; }

        public IReadOnlyCollection<PackageChange> PackageChanges { get; }

        public bool ForceFullUpdate { get; }

        public bool ForceFullAppUpdate { get; }

        public bool ForceFullPackageUpdate { get; }

        internal static PicsChanges Create(CMsgClientPICSChangesSinceResponse changes)
        {
            return new PicsChanges(changes.current_change_number,
                changes.since_change_number,
                changes.app_changes.Select(c => AppChange.Create(c)).ToList(),
                changes.package_changes.Select(c => PackageChange.Create(c)).ToList(),
                changes.force_full_update,
                changes.force_full_app_update,
                changes.force_full_package_update);
        }
    }

    public class AppChange
    {
        private AppChange(uint id, uint changeNumber, bool token)
        {
            Id = id;
            ChangeNumber = changeNumber;
            RequiresToken = token;
        }

        /// <summary>
        /// Gets the app ID for the app this change is for
        /// </summary>
        public long Id { get; }

        /// <summary>
        /// Gets this app's most recent change number
        /// </summary>
        public long ChangeNumber { get; }

        /// <summary>
        /// Gets whether this app's PICS info requires a PICS token to access
        /// </summary>
        public bool RequiresToken { get; }

        internal static AppChange Create(CMsgClientPICSChangesSinceResponse.AppChange change)
        {
            return new AppChange(change.appid, change.change_number, change.needs_token);
        }
    }

    public class PackageChange
    {
        private PackageChange(uint id, uint changeNumber, bool token)
        {
            Id = id;
            ChangeNumber = changeNumber;
            RequiresToken = token;
        }

        /// <summary>
        /// Gets the package ID for the package this change is for
        /// </summary>
        public long Id { get; }

        /// <summary>
        /// Gets this app's most recent change number
        /// </summary>
        public long ChangeNumber { get; }

        /// <summary>
        /// Gets whether this app's PICS info requires a PICS token to access
        /// </summary>
        public bool RequiresToken { get; }

        internal static PackageChange Create(CMsgClientPICSChangesSinceResponse.PackageChange change)
        {
            return new PackageChange(change.packageid, change.change_number, change.needs_token);
        }
    }
}
