#!/usr/bin/env bash
set -euo pipefail

# deploy-plugin.sh
# Usage: ./scripts/deploy-plugin.sh <container-name> [version]
# Copies built MatchUp.dll into running container, ensures plugin dir exists,
# updates gameinfo.gi with metamod entry if missing, and creates the matchup_version file.

CONTAINER=${1:-}
if [ -z "$CONTAINER" ]; then
  echo "Usage: $0 <container-name> [version]" >&2
  exit 2
fi
VERSION=${2:-v1.0.1}

LOCAL_DLL=bin/Release/net8.0/MatchUp.dll
REMOTE_ADDONS=/root/cs2-dedicated/game/csgo/addons
REMOTE_PLUGIN_DIR="$REMOTE_ADDONS/counterstrikesharp/plugins/MatchUp"
REMOTE_GAMEINFO=/root/cs2-dedicated/game/csgo/gameinfo.gi
REMOTE_MARKER="$REMOTE_ADDONS/matchup_version_${VERSION}"
GAMEINFO_INSERT_LINE=$'\t\t\tGame\tcsgo/addons/metamod'

if [ ! -f "$LOCAL_DLL" ]; then
  echo "Local DLL not found: $LOCAL_DLL" >&2
  exit 1
fi

echo "Ensuring plugin directory exists inside container $CONTAINER..."
docker exec "$CONTAINER" mkdir -p "$REMOTE_PLUGIN_DIR"

echo "Copying $LOCAL_DLL -> $CONTAINER:$REMOTE_PLUGIN_DIR/MatchUp.dll"
docker cp "$LOCAL_DLL" "$CONTAINER:$REMOTE_PLUGIN_DIR/MatchUp.dll"

echo "Setting file permissions"
docker exec "$CONTAINER" chmod 644 "$REMOTE_PLUGIN_DIR/MatchUp.dll"

echo "Ensuring gameinfo contains metamod entry"
docker exec "$CONTAINER" bash -lc "if ! grep -Fxq \"$GAMEINFO_INSERT_LINE\" $REMOTE_GAMEINFO 2>/dev/null; then sed -i 's:$gameinfo_string_match:$gameinfo_string_match\n$GAMEINFO_INSERT_LINE:' $REMOTE_GAMEINFO || true; fi"

echo "Creating version marker $REMOTE_MARKER"
docker exec "$CONTAINER" touch "$REMOTE_MARKER"

echo "Deploy complete. Check container logs for plugin load messages (no restart performed)."

exit 0
