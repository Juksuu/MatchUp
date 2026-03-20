#!/bin/bash
setup=$HOME/setup.sh
base_folder=${HOME}/${STEAM_APP_DIR}/game/csgo
addons_folder=$base_folder/addons
gameinfo=$base_folder/gameinfo.gi
matchup_version=${MATCHUP_VERSION-"v0.9.0"}
matchup_version_file=$addons_folder/matchup_version_$matchup_version
matchup_download_url="https://github.com/Juksuu/MatchUp/releases/download/$matchup_version/MatchUp+cssharp+metamod.zip"
gameinfo_string_match='\t\t\tGame_LowViolence\tcsgo_lv // Perfect World content override'
gameinfo_insert_line='\t\t\tGame\tcsgo/addons/metamod'

install_matchup() {
  install=false
  if [ ! -d "$addons_folder" ]; then
    install=true
  elif [ ! -f "$matchup_version_file" ]; then
    install=true
    rm -rf $addons_folder
  fi
  if [ $install = true ]; then
    echo "installing matchup"
    cd $base_folder
    wget -qO- $matchup_download_url | busybox unzip -
    touch $matchup_version_file
  fi
}

install_pelipaja() {
  echo "Installing Pelipaja plugin..."
  cp /tmp/MatchUp.dll $addons_folder/counterstrikesharp/plugins/MatchUp/MatchUp.dll
  echo "[Pelipaja] plugin installed."
}

update_gameinfo() {
  if ! grep -q "metamod" $gameinfo; then
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