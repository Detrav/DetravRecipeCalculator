pushd ".."

  FOR /F %%g IN ('git rev-list --count --all') do (SET BUILD_NUMBER=%%g)

  pushd "src/DetravRecipeCalculator.Desktop"
    dotnet publish -r win-x64 -c Release --self-contained /p:DebugType=None /p:DebugSymbols=false  /p:PublishSingleFile=true /p:AssemblyVersion=0.5.0.%BUILD_NUMBER% -o "%~dp0../_build/DetravRecipeCalculator-win-x64"
    dotnet publish -r linux-x64 -c Release --self-contained /p:DebugType=None /p:DebugSymbols=false /p:PublishSingleFile=true /p:AssemblyVersion=0.5.0.%BUILD_NUMBER% -o "%~dp0../_build/DetravRecipeCalculator-linux-x64"
    dotnet publish -r osx-x64 -c Release --self-contained /p:DebugType=None /p:DebugSymbols=false  /p:PublishSingleFile=true /p:AssemblyVersion=0.5.0.%BUILD_NUMBER% -o "%~dp0../_build/DetravRecipeCalculator-osx-x64"
  popd

popd