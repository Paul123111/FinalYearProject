FROM ubuntu:22.04

WORKDIR /unity

RUN apt-get update && apt-get install -y ca-certificates && rm -rf /var/lib/apt/lists/*

# Need to build with UnityEditor in advance.
COPY Builds/Server/ ./

# [Workaround] Wait until the sidecar is ready.
CMD sleep 2 && ./UnitySimpleServer.x86_64 -batchmode -nographics -logFile /dev/stdout

