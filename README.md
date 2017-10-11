# The Steam Standard Project

A collection of .NET Standard libraries using common types that provide functionality in one or more Steam services.

### Current libraries
| Library             | Description                                                                                                                                               | Progress                     | .NET Standard version |
|---------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------|-----------------------|
| Steam.Common        | Common types shared across multiple Steam libraries                                                                                                       | Shared - Added to as needed | .NET Standard 1.0     |
| Steam.KeyValues     | A fork of Json.Net 10 modified for editing KeyValues                                                                                                      | Work in progress             | .NET Standard 2.0     |
| Steam.Rest          | Common types for REST and HTTP requests                                                                                                                   | Shared - Added to as needed  | .NET Standard 1.1     |
| Steam.Local         | A library for editing Steam installations on Windows, Mac, or Linux                                                                                       | Work in progress             | .NET Standard 2.0     |
| Steam.Net           | A reimagining of the SteamKit built for async events, task-based asynchronous programming, a self-contained reconnect loop, and an abstracted job system. | Minimal working form             | .NET Standard 2.0     |
| Steam.Web           | A statically typed wrapper around the official Steam Web API                                                                                              | Work in progress             | .NET Standard 2.0     |
| Steam.Authenticator | A port of SteamAuth by geel9 to the Steam Standard project                                                                                                | Work in progress             | .NET Standard 2.0     |
| Steam.Community     | Common interfaces and methods for Steam web API and Steam network clients                                                                                 | Shared - Added to as needed   | .NET Standard 1.0     |
| Steam.Net.GameCoordinators  | Provides game coordinator implementations for Dota2, CSGO, and TF2                                                                                | Work in progress          | .NET Standard 2.0
| Steam.API     | An easy to use wrapper around the Steamworks API. Currently supports Steamworks SDK v1.40                                                                       | Work in progress          | .NET Standard 1.1 |

### Getting started
If you want to help develop this project, you will need a two things:

 * [.NET Core 2.0 preview](https://www.microsoft.com/net/core/preview)
 * [Visual Studio 2017 15.3 Preview](https://www.visualstudio.com/vs/preview/)

#### But why
SteamStandard is built with the goal of providing an easy to use wrapper around any part of Steam, even parts already covered by existing libraries. Existing libraries were built to almost emulate how Steam worked. However writing them that way causes the library to lack what makes C# libraries and code easy to use. Steam was written the way it was because it lacked features other languages had. C++ does not have Tasks or multicast delegates, so they had to write async code and events with callbacks and jobs. So, instead of making these libraries how Steam would make them in C++, I'm making them how they would be written in C#.

#### Navigating this repository
All projects, samples, and tests can be found in the "Steam Standard" solution, documentation for all libraries can be found on [GitHub Pages](https://obsidianminor.github.io/SteamStandardProject) or edited in the docs folder.

### Example code
This section contains example code you might use when using the libraries

#### Steam.API
The Steamworks API starts the first time you access the SteamworksClient.Current property. It is shutdown when you dispose the current client or exit the process.

```cs
if (SteamworksClient.ShouldRestart(appId))
  return 1;

try
{
  SteamworksClient client = SteamworksClient.Current; // this throws if SteamAPI_Init() returns false
  client.SetWarningMessageFunction((severity, message) =>
  {
    Debug.WriteLine(message);
    if(severity >= 1)
      Debugger.Break();
  });

  if(!client.User.LoggedOn)
  {
    Debug.WriteLine("Steam user must be logged in to play this game. (SteamworksClient.User.LoggedOn returned false)");
  }

  if (!client.Controller.Initialize())
  {
    Debug.WriteLine("SteamworksClient.Controller.Initialize() failed");
  }
}
catch (InvalidOperationException e)
{
  Debug.WriteLine($"Steam must be running to play this game: {e}");
}

// run game loop

SteamworksClient.Current.Dispose();

return 0;
```

#### Steam.Local
```cs
SteamInstallation local = SteamInstallation.GetInstallation();
LocalApp tf2 = local.GetApp(440); // if tf2 isn't installed, this returns null
string installPath = tf2.GetInstallDirectory();

// do stuff with the installation path
```

Example script to install a HUD from GitHub releases using PowerShell
```powershell
$Account = "insert github account"
$Project = "insert github project"

Add-Type -path "Steam.Local"
Add-Type -assembly "System.IO.Compression.FileSystem"

function Get-CustomFolder
{
  $install = [Steam.Local.SteamInstallation]::GetInstallation()
  If (!$install)
  {
    Write "Could not detect Steam installation"
    Exit
  }

  $tf2 = $install.GetApp(440)
  If (!$tf2)
  {
    Write "Could not detect TF2 installation"
    Exit
  }

  $installPath = $tf2.GetInstallDirectory()
  $customFolder = $[System.IO.Path]::Combine($installPath, "tf", "custom")
  return $customFolder
}

function Get-LatestRepoUrl
{
  param( [string]$Account, [string]$Project )

  $latestRelease = Invoke-WebRequest https://github.com/$Account/$Project/releases/latest -Headers @{"Accept"="application/json"}
  $json = $latestRelease.Content | ConvertFrom-Json
  $latestVersion = $json.tag_name

  $url = "https://github.com/$Account/$Project/releases/download/$latestVersion/master.zip"
  return $url
}

$customFolder = Get-CustomFolder

If (![System.IO.Directory]::Exists($customFolder)
{
  [System.IO.Directory]::CreateDirectory($customFolder)
}

Write "Discovered custom folder at $customFolder"
Write "Downloading latest release of $Account/$Project"

$url = Get-LatestRepoUrl $Account $Project

Invoke-WebRequest $url -OutFile "master.zip"

Write "Unzipping HUD to $customFolder"

[IO.Compression.ZipFile]::ExtractToDirectory("master.zip", $customFolder)

Write "Cleaning up"

Remove-Item master.zip
```

#### Steam.Authenticator
Steam.Authenticator works similarly to SteamAuth
```cs
// simple codes
SteamGuardAuthenticator auth = SteamGuardAuthenticator.LoadFromJson(File.ReadAllText("steamguard.json"));
string code = auth.GenerateCode();

// confirmations
IReadOnlyCollection<Confirmation> confirmations = await auth.GetConfirmations();
confirmations.Where(confirmation => confirmation.Type == ConfirmationType.Trade).AcceptAsync(); // or deny with Confirmation.DenyAsync()
```

#### Steam.Web
```cs
SteamWebClient client = new SteamWebClient("web api token");
Dictionary<uint, string> apps = await client.GetAppListAsync();

// override the endpoint
SteamWebClientConfig config = new SteamWebClientConfig { Endpoint = "https://partner.steam-api.com/", Token = "web api token" };
client = new SteamWebClient(config);
// use Steamworks API endpoints...
```

#### Steam.Net
```cs
// use websockets with new SteamNetworkClient(new SteamNetworkConfig { SocketClient = new DefaultWebSocketClient() })
static SteamNetworkClient client = new SteamNetworkClient();

static async Task Main(string[] args)
{
  // register for events
  client.Log += (src, message) =>
  {
    Console.WriteLine(message);
  };
  client.LoginActionRequested += async (src, response) =>
  {
    Console.Write("Please enter your two factor or auth token: ");
    await client.ContinueLoginAsync(Console.ReadLine());
  };
  client.CanLogin += async (src, e) =>
  {
    await client.LoginAsync("username", password: "password");
  };
  client.LoggedIn += async (src, e) =>
  {
    PicsProductInfo product = await client.GetAppInfoAsync(440);
    // do something with the product

    await network.StopAsync();
  };
  await network.StartAsync();
  await Task.Delay(-1);
}
```
