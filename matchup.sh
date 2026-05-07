#!/bin/bash
setup=$HOME/setup.sh
base_folder=${HOME}/${STEAM_APP_DIR}/game/csgo
addons_folder=$base_folder/addons
gameinfo=$base_folder/gameinfo.gi
matchup_version=${MATCHUP_VERSION:-"v0.9.1"}
matchup_version_file=$addons_folder/matchup_version_$matchup_version
matchup_download_url="https://github.com/Juksuu/MatchUp/releases/download/$matchup_version/MatchUp+cssharp+metamod.zip"
gameinfo_insert_line='          Game    csgo/addons/metamod'

install_matchup() {
  if [ ! -f "$matchup_version_file" ]; then
    echo "Installing MatchUp $matchup_version..."
    wget -q "$matchup_download_url" -O /tmp/matchup.zip
    mkdir -p "$addons_folder"
    cd $base_folder
    busybox unzip -o /tmp/matchup.zip
    rm /tmp/matchup.zip
    rm -f $addons_folder/matchup_version_*
    touch "$matchup_version_file"
    echo "MatchUp $matchup_version installed."
  else
    echo "MatchUp $matchup_version already up to date."
  fi
}

install_pelipaja() {
  echo "Installing Pelipaja plugin..."
  mkdir -p "$addons_folder/counterstrikesharp/plugins/MatchUp"
  cp /tmp/MatchUp.dll "$addons_folder/counterstrikesharp/plugins/MatchUp/MatchUp.dll"
  echo "[Pelipaja] plugin installed."
}

update_gameinfo() {
  if ! grep -qF "csgo/addons/metamod" "$gameinfo"; then
    echo "updating gameinfo"
    sed -i "s:Game_LowViolence\tcsgo_lv // Perfect World content override:Game_LowViolence\tcsgo_lv // Perfect World content override\n$gameinfo_insert_line:" "$gameinfo"
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