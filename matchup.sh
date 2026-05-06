#!/bin/bash
setup=$HOME/setup.sh
base_folder=${HOME}/${STEAM_APP_DIR}/game/csgo
addons_folder=$base_folder/addons
gameinfo=$base_folder/gameinfo.gi
matchup_version=${MATCHUP_VERSION-"v1.0.1"}
matchup_version_file=$addons_folder/matchup_version_$matchup_version
matchup_download_url="https://github.com/Tomppahh/Pelipaja.net-Plugin/releases/download/$matchup_version/MatchUp+cssharp+metamod.zip"
gameinfo_string_match='\t\t\tGame_LowViolence\tcsgo_lv // Perfect World content override'
gameinfo_insert_line='\t\t\tGame\tcsgo/addons/metamod'

install_matchup() {
  install=false
  # If current version marker missing, install or update only the addon folders we need.
  if [ ! -f "$matchup_version_file" ]; then
    install=true
  fi

  if [ $install = true ]; then
    echo "installing matchup"
    cd /tmp
    rm -rf /tmp/matchup_unpack
    mkdir -p /tmp/matchup_unpack
    # download and unpack into temp dir
    wget -qO- "$matchup_download_url" | busybox unzip -d /tmp/matchup_unpack -
    # ensure addons folder exists
    mkdir -p "$addons_folder"
    # copy only the addon folders we ship (metamod and counterstrikesharp)
    if [ -d /tmp/matchup_unpack/addons/metamod ]; then
      rm -rf "$addons_folder/metamod"
      cp -a /tmp/matchup_unpack/addons/metamod "$addons_folder/"
    fi
    if [ -d /tmp/matchup_unpack/addons/counterstrikesharp ]; then
      rm -rf "$addons_folder/counterstrikesharp"
      cp -a /tmp/matchup_unpack/addons/counterstrikesharp "$addons_folder/"
    fi
    # create version marker
    touch "$matchup_version_file"
    # clean temp
    rm -rf /tmp/matchup_unpack
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
  install_pelipaja
  update_gameinfo
  exec $setup start
fi
