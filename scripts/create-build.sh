#!/usr/bin/env bash

METAMOD_VERSION=1387
CSSAPI_VERSION=$(dotnet list package --format json | jq -r '.projects[0].frameworks.[0].topLevelPackages.[] | select(.id == "CounterStrikeSharp.API") | .requestedVersion')

mkdir -p build
cd build

wget -q https://github.com/roflmuffin/CounterStrikeSharp/releases/download/v${CSSAPI_VERSION}/counterstrikesharp-with-runtime-linux-${CSSAPI_VERSION}.zip
unzip counterstrikesharp-with-runtime-linux-${CSSAPI_VERSION}.zip
rm counterstrikesharp-with-runtime-linux-${CSSAPI_VERSION}.zip

wget -c https://mms.alliedmods.net/mmsdrop/2.0/mmsource-2.0.0-git${METAMOD_VERSION}-linux.tar.gz -O - | tar -xz

cd ..

dotnet build -c Release

mkdir -p build/addons/counterstrikesharp/plugins/MatchUp
mkdir -p build/cfg

cp bin/Release/net8.0/* build/addons/counterstrikesharp/plugins/MatchUp/

cp -R cfg/* build/cfg/


pushd build
zip -r MatchUp+cssharp+metamod.zip *
popd
