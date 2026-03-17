# RemoteCommander

A standalone WPF app for controlling multiple DreamPoeBot instances over your LAN. Works with the [RemoteControl](https://github.com/rushtothesun/RemoteControl) plugin.

## What it does

Sends HTTP commands to DreamPoeBot bots running the RemoteControl plugin. You don't need a running Path of Exile client — just this app and the bot IPs.

One window controls all your bots. "Sync All" broadcasts commands to every bot at once.

## Controls

Each bot panel has:

| Control | What it does |
|---------|-------------|
| Start Bot / Stop Bot | Start/stop the bot |
| Follow | Toggle following (on/off) |
| Town / HO / Heist | Toggle follow in those areas |
| Attack | Toggle combat |
| Loot | Toggle looting |
| Auto-TP | Toggle auto-teleport |
| Teleport | Teleport to leader |
| Open Portal | Open a town portal |
| Enter Portal | Enter a nearby portal |
| Stash | Go stash inventory |
| New Instance | Create new map instance |
| AutoDep | Toggle auto-deposit |
| Stash Type | Switch between Regular / Guild stash |
| Ult TP | Toggle portal after Ultimatum |
| Ult Timer | Set Ultimatum portal search time (1-60s) |
| Unloader | Trigger Ultimatum unloader sweep |
| Unload Delay | Set unloader start delay (1500-10000ms) |

## Setup

1. Enable the [RemoteControl](https://github.com/rushtothesun/RemoteControl) plugin on your bots (default port: 5200)
2. Run RemoteCommander
3. Add bots by IP:Port (e.g. `192.168.1.100:5200`) with an optional label
4. Click buttons

If running on the same machine as the bot, use your LAN IP — the plugin binds to LAN adapters only, not localhost.

## Settings

Window opacity, always-on-top, muted colors, and the bot list auto-save to `RemoteCommanderSettings.json` next to the exe.

## Building

```
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

Output: `bin\Release\net8.0-windows\win-x64\publish\RemoteCommander.exe`

Single exe, no dependencies. Generates its config on first run.

## Requirements

- .NET 8 (bundled in the single-file build)
- Bots running DreamPoeBot with the RemoteControl plugin
- Same LAN
