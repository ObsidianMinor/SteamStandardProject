# Samples

### Block all friend requests from unqualified users
```csharp
static async Task Main(string[] args)
{
    SteamNetworkClient client = new SteamNetworkClient(new SteamNetworkConfig
    {
        WebApiKey = "web_api_key"
    });
    client.NotificationReceived += async (notification) =>
    {
        if (notification is FriendNotification friendRequest)
        {
            int level = await client.GetInterface<IPlayerService>().GetSteamLevel(friendRequest.User.Id);
            if (level < 5)
                await client.RejectFriendRequest(friendRequest.User);
        }
    };

    await client.StartAndLoginAsync("username", password: "mahPassword");
}
```