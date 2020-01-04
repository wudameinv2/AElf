[string]$CAKE_SCRIPT_FILE = '../build.cake'
[string]$CAKE_ARGS="$CAKE_SCRIPT_FILE -verbosity=verbose"
dotnet --list-sdks
dotnet tool install --global cake.tool --version $CAKE_VERSION
export PATH="$PATH:$HOME/.dotnet/tools"

dotnet tool restore
Write-Host "dotnet cake  $CAKE_ARGS $ARGS" -ForegroundColor GREEN
dotnet cake  $CAKE_ARGS $ARGS
