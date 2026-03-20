FROM juksuu/cs2:matchup

COPY matchup.sh /root/matchup.sh
COPY bin/Release/net8.0/MatchUp.dll /tmp/MatchUp.dll
RUN chmod +x /root/matchup.sh