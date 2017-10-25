# Steam.Net vs SteamKit
Steam.Net and SteamKit are very different libraries. While they accomplish the same task, they return the data in different ways. This article will list the differences between Steam.Net and SteamKit.

## Events
In SteamKit, events were done as "handlers" with "callbacks". Code to subscribe to a callback might look like this:
```csharp
SteamApps appsHandler = client.GetHandler<SteamApps>();
appsHandler.Subscribe<SteamApps.LicenseListCallback>(OnLicenseReceive);
```

In Steam.Net the handler system has been replaced with multicast delegates and all methods and properties exist in the SteamNetworkClient.
```csharp
network.ReceivedLicenses += OnLicenseReceive;
```

Jobs don't have events and can only be awaited by the original task

## Jobs
In SteamKit, jobs had their own events which could be mapped to an AsyncJob object. AsyncJob objects could also be awaited.

```csharp
AsyncJob<PICSChangesCallback> changesJob = PICSGetChangesSince(0);
// subscribe to callback
appsHandler.Subscribe<SteamApps.PICSChangesCallback>(OnPICSChanges);
// or await
PICSChangesCallback changes = await changesJob;
```

In Steam.Net, AsyncJobs have been removed and replaced with normal Task objects.

```csharp
Task<PicsChanges> changesTask = GetPicsChangesAsync(0);
PicsChanges changes = await changesTask;
```

## Extending
In SteamKit, the client could be extended by creating classes that implement ClientMsgHandler and creating them with the `GetHandler` method.

```csharp
public class CustomHandler : ClientMsgHandler
{
    // handler code
}
CustomHandler handler = client.GetHandler<CustomHandler>();
```

In Steam.Net, as stated before, the client is extended by creating a sub class of the SteamNetworkClient.