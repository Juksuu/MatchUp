#!/usr/bin/env bash
# Fetch the latest CounterStrikeSharp version from GitHub
CSSAPI_VERSION=$(curl -s https://api.github.com/repos/roflmuffin/CounterStrikeSharp/releases/latest | jq -r '.tag_name' | sed 's/v//')

# Dynamically fetch the latest MetaMod 2.0 build number
METAMOD_VERSION=$(curl -s https://mms.alliedmods.net/mmsdrop/2.0/ \
  | grep -oP 'mmsource-2\.0\.0-git\K[0-9]+(?=-linux)' \
  | sort -n | tail -1)

echo "Using MetaMod version: $METAMOD_VERSION"
echo "Using CounterStrikeSharp version: $CSSAPI_VERSION"

mkdir -p build
cd build
wget -q https://github.com/roflmuffin/CounterStrikeSharp/releases/download/v${CSSAPI_VERSION}/counterstrikesharp-with-runtime-linux-${CSSAPI_VERSION}.zip
unzip counterstrikesharp-with-runtime-linux-${CSSAPI_VERSION}.zip
rm counterstrikesharp-with-runtime-linux-${CSSAPI_VERSION}.zip
wget -c https://mms.alliedmods.net/mmsdrop/2.0/mmsource-2.0.0-git${METAMOD_VERSION}-linux.tar.gz -O - | tar -xz

# Ensure metaplugins.ini includes the CounterStrikeSharp plugin
if [ -f addons/metamod/metaplugins.ini ]; then
  if ! grep -q "counterstrikesharp" addons/metamod/metaplugins.ini; then
    echo "addons/counterstrikesharp/bin/linuxsteamrt64/counterstrikesharp" >> addons/metamod/metaplugins.ini
  fi
fi

# Ensure all plugin binaries are executable
chmod +x addons/metamod/bin/linuxsteamrt64/*.so 2>/dev/null || true

cd ..
dotnet build -c Release MatchUp.csproj
mkdir -p build/addons/counterstrikesharp/plugins/MatchUp
mkdir -p build/cfg
cp bin/Release/net8.0/* build/addons/counterstrikesharp/plugins/MatchUp/
cp -R cfg/* build/cfg/

# Ensure CounterStrikeSharp binary is executable in final build
chmod +x build/addons/counterstrikesharp/bin/linuxsteamrt64/counterstrikesharp.so 2>/dev/null || true

pushd build
zip -r MatchUp+cssharp+metamod.zip *
popd