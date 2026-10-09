#!/usr/bin/env bash
set -euo pipefail
 
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$DOTNET_ROOT:$PATH"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1
 
# Install .NET 10 SDK
curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
bash /tmp/dotnet-install.sh --channel 10.0 --install-dir "$DOTNET_ROOT"
 
dotnet --version
 
# Publish the PROJECT (not the .slnx) so -o is honored cleanly.
# Netlify "Publish directory" should be: release/wwwroot
dotnet publish ReelTime.csproj -c Release -o release -nodeReuse:false
