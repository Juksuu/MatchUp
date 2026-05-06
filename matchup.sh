#!/bin/bash

set -e

STEAM_APP_DIR=${STEAM_APP_DIR:-cs2-dedicated}

base_folder=$HOME/$STEAM_APP_DIR/game/csgo
addons_folder=$base_folder/addons
gameinfo=$base_folder/gameinfo.gi

matchup_version=${MATCHUP_VERSION:-"v0.9.0"}
matchup_version_file=$addons_folder/matchup_version_$matchup_version
matchup_download_url="https://github.com/Juksuu/MatchUp/releases/download/v0.9.0/MatchUp+cssharp+metamod.zip"

echo "[BOOT] base_folder=$base_folder"

install_matchup() {
  if [ -f "$matchup_version_file" ]; then
    echo "[MatchUp] already installed"
    return
  fi

  echo "[MatchUp] installing..."
  cd /tmp
  rm -rf /tmp/matchup_unpack
  mkdir -p /tmp/matchup_unpack

  wget -qO- "$matchup_download_url" | busybox unzip -d /tmp/matchup_unpack -

  mkdir -p "$addons_folder"

  if [ -d /tmp/matchup_unpack/addons/metamod ]; then
    rm -rf "$addons_folder/metamod"
    cp -a /tmp/matchup_unpack/addons/metamod "$addons_folder/"
    find "$addons_folder/metamod/bin" -name "*.so" -type f -exec chmod +x {} + || true
  fi

  if [ -d /tmp/matchup_unpack/addons/counterstrikesharp ]; then
    rm -rf "$addons_folder/counterstrikesharp"
    cp -a /tmp/matchup_unpack/addons/counterstrikesharp "$addons_folder/"
    find "$addons_folder/counterstrikesharp/bin" -type f -exec chmod +x {} + || true
  fi

  touch "$matchup_version_file"
  rm -rf /tmp/matchup_unpack
}

install_cssharp() {
  local latest
  latest=$(wget -qO- https://api.github.com/repos/roflmuffin/CounterStrikeSharp/releases/latest | grep tag_name | cut -d'"' -f4)

  local version_num
  version_num=$(echo "$latest" | sed 's/v//')

  local version_file=$addons_folder/cssharp_version_$latest

  if [ -f "$version_file" ]; then
    echo "[CSSHARP] already up to date"
    return
  fi

  echo "[CSSHARP] installing $latest..."

  wget -q "https://github.com/roflmuffin/CounterStrikeSharp/releases/download/$latest/counterstrikesharp-with-runtime-linux-${version_num}.zip" -O /tmp/cssharp.zip

  cd "$base_folder"
  busybox unzip -o /tmp/cssharp.zip

  find "$addons_folder/counterstrikesharp/bin" -type f -exec chmod +x {} + || true

  rm -f /tmp/cssharp.zip
  rm -f "$addons_folder/cssharp_version_"*
  touch "$version_file"
}

install_pelipaja() {
  echo "[Pelipaja] installing plugin..."

  mkdir -p "$addons_folder/counterstrikesharp/plugins/MatchUp"

  if [ -f /tmp/MatchUp.dll ]; then
    cp /tmp/MatchUp.dll "$addons_folder/counterstrikesharp/plugins/MatchUp/MatchUp.dll"
  else
    echo "[WARN] MatchUp.dll not found in /tmp"
  fi

  echo "[Pelipaja] plugin installed"
}

update_gameinfo() {
  mkdir -p "$base_folder"

  if [ ! -s "$gameinfo" ]; then
    echo "[gameinfo] creating fresh file"

    cat > "$gameinfo" << 'EOF'
"GameInfo"
{
    game        "Counter-Strike 2"
    title       "Counter-Strike 2"
    type        multiplayer_only

    FileSystem
    {
        SearchPaths
        {
            Game    |gameinfo_path|.
            Game    csgo
            Game    csgo_imported
            Game    csgo_core
            Game    core
            Game    csgo/addons/metamod
        }
    }
}
EOF
    return
  fi

  if ! grep -Fq "csgo/addons/metamod" "$gameinfo"; then
    echo "[gameinfo] adding metamod path"
    sed -i '/SearchPaths/a\            Game    csgo/addons/metamod' "$gameinfo"
  fi
}

main() {
  echo "[BOOT] starting setup"

  install_matchup
  install_cssharp
  install_pelipaja
  update_gameinfo

  echo "[BOOT] setup complete"

  echo "[BOOT] IMPORTANT: setup.sh is NOT required anymore"

  echo "[BOOT] keeping container alive"
  tail -f /dev/null
}

main