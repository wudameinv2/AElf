#!/bin/sh

SCRIPT='../build.cake'
CAKE_VERSION='0.35.0'


# Install  cake.tool
dotnet tool install --global cake.tool --version $CAKE_VERSION
export PATH="$PATH:$HOME/.dotnet/tools"

dotnet tool restore

# Start Cake
CAKE_ARGS="$SCRIPT -verbosity=verbose"

echo "\035[32mdotnet cake $CAKE_ARGS $@"

dotnet cake $CAKE_ARGS "$@"
