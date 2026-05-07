FROM juksuu/cs2:matchup

WORKDIR /root
COPY matchup.sh /root/matchup.sh
COPY bin/Release/net8.0/MatchUp.dll /tmp/MatchUp.dll
# Copy the project file into the container working directory so runtime
# scripts that grep MatchUp.csproj (relative path) will find it.
COPY MatchUp.csproj ./MatchUp.csproj
RUN chmod +x /root/matchup.sh