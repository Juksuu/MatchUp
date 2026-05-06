#!/bin/bash
setup=$HOME/setup.sh
base_folder=${HOME}/${STEAM_APP_DIR}/game/csgo
addons_folder=$base_folder/addons
gameinfo=$base_folder/gameinfo.gi
matchup_version=${MATCHUP_VERSION-"v0.9.0"}
matchup_version_file=$addons_folder/matchup_version_$matchup_version
matchup_download_url="https://github.com/Juksuu/MatchUp/releases/download/v0.9.0/MatchUp+cssharp+metamod.zip"
gameinfo_string_match='\t\t\tGame_LowViolence\tcsgo_lv // Perfect World content override'
gameinfo_insert_line='\t\t\tGame\tcsgo/addons/metamod'

install_matchup() {
  install=false
  if [ ! -f "$matchup_version_file" ]; then
    install=true
  fi
  if [ $install = true ]; then
    echo "installing matchup"
    cd /tmp
    rm -rf /tmp/matchup_unpack
    mkdir -p /tmp/matchup_unpack
    wget -qO- "$matchup_download_url" | busybox unzip -d /tmp/matchup_unpack -
    mkdir -p "$addons_folder"
    if [ -d /tmp/matchup_unpack/addons/metamod ]; then
      rm -rf "$addons_folder/metamod"
      cp -a /tmp/matchup_unpack/addons/metamod "$addons_folder/"
      # Ensure Metamod binaries are executable
      find "$addons_folder/metamod/bin" -name "*.so" -type f -exec chmod +x {} +
      # Ensure metaplugins.ini includes CounterStrikeSharp
      if [ -f "$addons_folder/metamod/metaplugins.ini" ] && ! grep -q "counterstrikesharp" "$addons_folder/metamod/metaplugins.ini"; then
        echo "addons/counterstrikesharp/bin/linuxsteamrt64/counterstrikesharp" >> "$addons_folder/metamod/metaplugins.ini"
      fi
    fi
    # Copy top-level Metamod plugin descriptors from extraction (or create if missing)
    if [ -f /tmp/matchup_unpack/addons/metamod.vdf ]; then
      cp /tmp/matchup_unpack/addons/metamod.vdf "$addons_folder/"
    else
      cat > "$addons_folder/metamod.vdf" << 'VDFEOF'
"Plugin"
{
	"file"	"addons/metamod/bin/server"
}
VDFEOF
    fi
    if [ -f /tmp/matchup_unpack/addons/metamod_x64.vdf ]; then
      cp /tmp/matchup_unpack/addons/metamod_x64.vdf "$addons_folder/"
    else
      cat > "$addons_folder/metamod_x64.vdf" << 'VDFEOF'
"Plugin"
{
	"file"	"addons/metamod/bin/linux64/server"
}
VDFEOF
    fi
    if [ -d /tmp/matchup_unpack/addons/counterstrikesharp ]; then
      rm -rf "$addons_folder/counterstrikesharp"
      cp -a /tmp/matchup_unpack/addons/counterstrikesharp "$addons_folder/"
      # Ensure CounterStrikeSharp binary is executable
      find "$addons_folder/counterstrikesharp/bin" -name "counterstrikesharp*" -type f -exec chmod +x {} +
    fi
    touch "$matchup_version_file"
    rm -rf /tmp/matchup_unpack
  fi
}

install_cssharp() {
  local latest=$(wget -qO- https://api.github.com/repos/roflmuffin/CounterStrikeSharp/releases/latest | grep tag_name | cut -d'"' -f4)
  local version_num=$(echo $latest | sed 's/v//')
  local version_file=$addons_folder/cssharp_version_$latest

  if [ ! -f "$version_file" ]; then
    echo "Installing CounterStrikeSharp $latest..."
    wget -q "https://github.com/roflmuffin/CounterStrikeSharp/releases/download/$latest/counterstrikesharp-with-runtime-linux-${version_num}.zip" -O /tmp/cssharp.zip
    cd $base_folder
    busybox unzip -o /tmp/cssharp.zip
    # Ensure CounterStrikeSharp binaries are executable
    find "$addons_folder/counterstrikesharp/bin" -name "*.so" -type f -exec chmod +x {} +
    rm /tmp/cssharp.zip
    rm -f $addons_folder/cssharp_version_*
    touch "$version_file"
    echo "CounterStrikeSharp $latest installed."
  else
    echo "CounterStrikeSharp $latest already up to date."
  fi
}
install_pelipaja() {
  echo "Installing Pelipaja plugin..."
  mkdir -p "$addons_folder/counterstrikesharp/plugins/MatchUp"
  cp /tmp/MatchUp.dll $addons_folder/counterstrikesharp/plugins/MatchUp/MatchUp.dll
  echo "[Pelipaja] plugin installed."
}

update_gameinfo() {
  if ! grep -Fxq "$gameinfo_insert_line" "$gameinfo"; then
    echo "updating gameinfo"
    sed -i "s:$gameinfo_string_match:$gameinfo_string_match\n$gameinfo_insert_line:" $gameinfo
  fi
}

if [ ! -z $1 ]; then
  $1
else
  $setup install_and_update
  install_matchup
  install_cssharp
  install_pelipaja
  update_gameinfo
  exec $setup start
fi