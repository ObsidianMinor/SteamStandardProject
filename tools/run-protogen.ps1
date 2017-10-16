function DeleteOldFiles
{
    $path = "$($pwd.Path)/../src/Steam.Net/Messages/Protobufs/SteamProtobufs.cs";

    if (![System.IO.File]::Exists($path))
    {
        New-Item $path 
    }

    if ([System.IO.Directory]::Exists($tempDir)) 
    { 
        Remove-Item -Path $tempDir -recurse 
    }
}

$tempDir = "temp"
$outFile = "SteamTracking.zip"
$unzipOut = "repo"
$protoDir = "$($tempDir)\$($unzipOut)\Protobufs-master\steam"

Add-Type -AssemblyName "system.io.compression.filesystem"
DeleteOldFiles
mkdir $tempDir
Invoke-WebRequest -Uri https://github.com/SteamDatabase/Protobufs/archive/master.zip -Method GET -OutFile "./$($tempDir)/$($outFile)"
[io.compression.zipfile]::ExtractToDirectory("$($pwd.Path)/$($tempDir)/$($outFile)", "$($pwd.Path)/$($tempDir)/$($unzipOut)")
$files = Get-ChildItem -Path "$($pwd.Path)\$($tempDir)\$($unzipOut)\Protobufs-master\steam\*" -Recurse -Include *encrypted_app_ticket.proto, *steammessages_base.proto, *steammessages_clientserver.proto, *steammessages_clientserver_2.proto, *steammessages_clientserver_friends.proto, *steammessages_clientserver_login.proto, *steammessages_sitelicenseclient.proto, *content_manifest.proto, *steammessages_unified_base.steamclient.proto, *steammessages_broadcast.steamclient.proto, *steammessages_cloud.steamclient.proto, *steammessages_credentials.steamclient.proto, *steammessages_datapublisher.steamclient.proto, *steammessages_depotbuilder.steamclient.proto, *steammessages_deviceauth.steamclient.proto, *steammessages_econ.steamclient.proto, *steammessages_gamenotifications.steamclient.proto, *steammessages_gameservers.steamclient.proto, *steammessages_linkfilter.steamclient.proto, *steammessages_inventory.steamclient.proto, *steammessages_offline.steamclient.proto, *steammessages_parental.steamclient.proto, *steammessages_partnerapps.steamclient.proto, *steammessages_physicalgoods.steamclient.proto, *steammessages_player.steamclient.proto, *steammessages_publishedfile.steamclient.proto, *steammessages_secrets.steamclient.proto, *steammessages_site_license.steamclient.proto, *steammessages_twofactor.steamclient.proto, *steammessages_useraccount.steamclient.proto, *steammessages_video.steamclient.proto | Foreach -Process { "-i:`"$($_.FullName)`"" }
Invoke-Expression ".\Protogen\protogen.exe -i:`".\Google\Protobuf\descriptor.proto`" $($files -join " ") -o:`"..\src\Steam.Net\Messages\Protobufs\SteamProtobufs.cs`" -ns:`"Steam.Net.Messages.Protobufs`" -p:detectMissing -p:import=Steam.Net.Messages.Protobufs"
Invoke-Expression ".\Protogen\protogen.exe -i:`".\gc\gc.proto`" -o:`"..\src\Steam.Net\GameCoordinators\Messages\Protobufs\GameCoordinatorProtobufs.cs`" -ns:`"Steam.Net.GameCoordinators.Messages.Protobufs`" -p:detectMissing"
Remove-Item $tempDir -Recurse