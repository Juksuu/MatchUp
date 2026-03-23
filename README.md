Pelipaja.net-Plugin (MatchUp fork) - CS2 Match creation plugin
==============

**Pelipaja.net-Plugin** is my fork of **Juksuu/MatchUp** for integrating CS2 match flow with the Pelipaja.net matchmaking website.

It is a CounterStrikeSharp plugin that can run matches directly on the server, and in this fork it can also:
- receive match configuration via an embedded HTTP server (from Pelipaja.net / Next.js), and
- report match status back to the website via a webhook-style API call.

> Note: Internally, many identifiers (plugin folder name, namespaces, etc.) may still use `MatchUp`. This README documents the fork-specific behavior and the Pelipaja integration.

## Installation

1. Install Metamod  
   https://cs2.poggu.me/metamod/installation/

2. Install CounterStrikeSharp (CSS)

3. Install this plugin
   - Download the latest release from this repository’s **Releases** page.
   - Copy/merge the contents into your server’s `cs2/game/csgo` folder.
   - Verify the installation:
     - Run `css_plugins list`
     - You should see the plugin listed (it may appear as **MatchUp**, since this is a fork).

## Features

- Easy creation of a match directly from the server
- Knife round (can be toggled in setup phase)
- **Pelipaja integration**
  - Embedded HTTP server to receive match config from Pelipaja.net (Next.js)
  - Team assignment based on SteamID when players connect
  - Match status reporting back to Pelipaja.net via HTTP

## Pelipaja integration overview

### 1) Embedded HTTP server (config input)

This fork starts an HTTP listener (see `src/HttpServer.cs`) which accepts configuration pushes from Pelipaja.net.

- **Listen address:** `http://*:<port>/`
- **Port env var:** `MATCHUP_API_PORT`
- **Default port:** `27090`
- **Auth:** `Authorization: Bearer <secret>`
- **Secret:** compared against `PelipajaConfig.ApiSecret`
- **Endpoint:** `POST /config`
- **Content-Type:** JSON

#### `POST /config` payload

Example payload (based on the plugin’s `MatchConfigPayload` model):

```json
{
  "mode": "manual",
  "matchId": "abc123",
  "map": "de_mirage",
  "teamSize": 5,
  "knifeRound": true,
  "ownerSteamId": "7656119XXXXXXXXXX",
  "team1": { "name": "Team 1", "players": ["7656119..."] },
  "team2": { "name": "Team 2", "players": ["7656119..."] }
}
```

When config is received, the plugin will apply settings (map, team size, knife round) and start the match flow.

### 2) Waiting for config + auto team assignment

This fork includes a Pelipaja-specific waiting state (`PelipajaWaitingState`) that logs:

- `"[Pelipaja] Waiting for config from Next.js..."`

While waiting / when players connect, players can be automatically assigned to teams by SteamID based on the received config (team1/team2 player lists).

### 3) Status reporting (output to Pelipaja.net)

This fork can post match status updates back to Pelipaja.net (see `src/WebhookClient.cs`).

- Uses `PelipajaConfig.WebhookUrl` and `PelipajaConfig.MatchId`
- Adds header `Authorization: Bearer <ApiSecret>`
- Posts JSON `{ "status": "<status>" }` to:

`<WebhookUrl>/api/matches/<MatchId>/status`

If `WebhookUrl` or `MatchId` are missing, it will skip reporting.

## Commands (in-game chat)

Commands can be triggered with either `!` or `.`

### Setup phase

- `!map` Shows a selection menu for map in chat
- `!config` Print current config to chat
- `!start` Complete setup and transition to ready status state
- `!help` Print setup phase command usages
- `!knife <boolean>` Select if knife round should be played
- `!team_size <number>` Set team size (used when checking for ready status)
- `!version` Show plugin version
- `!demo` Show demo status (if enabled in this fork)

### ReadyUp phase

- `!r` or `!ready` Ready player
- `!ur` or `!unready` Unready player
- `!forceready` Force start match

For testing only

- `!bot_ct` Add bot for ct

### Knife phase

- `!switch` Switch sides
- `!stay` Stay on current side

For testing only

- `!kill` Kill self in game
- `!bot_ct` Add bot for ct

### Live phase

- `!pause` Pause match
- `!unpause` Unpause match
- `!backup` Can be used in pause state to backup rounds
- `!cancelmatch` As a lobby leader, you can use this command to cancel the match and close the game server

For testing only

- `!kill` Kill self in game
- `!bot_ct` Add bot for ct

### Always

- `!reset` Reset MatchUp

## Console Commands

- `matchup_reconfigure`  
  Reload maps and settings. Only allowed during the setup phase.

## Configuration

The upstream MatchUp config convention is:

- `csgo/cfg/MatchUp`

This fork may also ship default config files in the repository under `cfg/` and expects them to be placed under your server
