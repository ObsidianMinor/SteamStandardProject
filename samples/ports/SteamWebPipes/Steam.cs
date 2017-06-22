using System;
using System.Threading;
using Steam.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Steam.Common;

namespace SteamWebPipes
{
    internal class Steam
    {
        private readonly SteamNetworkClient Client;
        private bool IsLoggedOn = false;

        public uint PreviousChangeNumber;
        public bool IsRunning = true;

        public Steam()
        {
            Client = new SteamNetworkClient();
            
            Client.CanLogin += OnConnected;
            Client.Disconnected += OnDisconnected;
            Client.LoggedOn += OnLoggedOn;
            Client.LoggedOff += OnLoggedOff;
        }

        public async Task StartAsync(CancellationToken token)
        {
            await Client.StartAsync();

            try
            {
                while (IsRunning && !token.IsCancellationRequested)
                {
                    if (IsLoggedOn)
                    {
                        Task delayTask = Task.Delay(5000);
                        OnPICSChanges(await Client.GetPicsChangesSince(PreviousChangeNumber, true, true));

                        await delayTask;
                    }
                }
            }
            catch (OperationCanceledException) { }
        }

        private void OnPICSChanges(PicsChanges changes)
        {
            var previous = PreviousChangeNumber;

            if (previous == changes.CurrentChangeNumber)
            {
                return;
            }

            PreviousChangeNumber = changes.CurrentChangeNumber;

            IEnumerable<PicsChangeData> appChanges = changes.Changes.Where(c => c.Type == PicsDataType.App);
            IEnumerable<PicsChangeData> packageChanges = changes.Changes.Where(c => c.Type == PicsDataType.Package);

            var packageChangesCount = packageChanges.Count();
            var appChangesCount = appChanges.Count();

            Bootstrap.Log("Changelist {0} -> {1} ({2} apps, {3} packages)", changes.LastChangeNumber, changes.CurrentChangeNumber, appChangesCount, packageChangesCount);

            if (previous == 0)
            {
                return;
            }

            // Group apps and package changes by changelist, this will seperate into individual changelists
            var appGrouping = appChanges.GroupBy(a => a.ChangeNumber);
            var packageGrouping = packageChanges.GroupBy(p => p.ChangeNumber);

            // Join apps and packages back together based on changelist number
            var changeLists = Utils.FullOuterJoin(appGrouping, packageGrouping, a => a.Key, p => p.Key, (a, p, key) => new SteamChangelist
                {
                    ChangeNumber = key,

                    Apps = a.Select(x => x.PicsId),
                    Packages = p.Select(x => x.PicsId),
                },
                new EmptyGrouping<uint, PicsChangeData>(),
                new EmptyGrouping<uint, PicsChangeData>())
                .OrderBy(c => c.ChangeNumber);

            foreach (var changeList in changeLists)
            {
                Bootstrap.Broadcast(new ChangelistEvent(changeList));
            }
        }

        private async void OnConnected(object sender, EventArgs e)
        {
            Bootstrap.Log("Connected to Steam, logging in...");
            await Client.LoginAnonymousAsync();
        }

        private void OnDisconnected(object sender, Exception ex)
        {
            if (!IsRunning)
            {
                Bootstrap.Log("Shutting down...");
                return;
            }

            if (IsLoggedOn)
            {
                Bootstrap.Broadcast(new LogOffEvent());
                IsLoggedOn = false;
            }
        }

        private void OnLoggedOn(object sender, EventArgs e)
        {
            IsLoggedOn = true;

            Bootstrap.Broadcast(new LogOnEvent());

            Bootstrap.Log($"Logged in, current valve time is {DateTime.Now}");
        }

        private void OnLoggedOff(object sender, Result result)
        {
            if (IsLoggedOn)
            {
                Bootstrap.Broadcast(new LogOffEvent());
                IsLoggedOn = false;
            }

            Bootstrap.Log("Logged off from Steam");
        }
    }
}
