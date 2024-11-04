pushd "../src/DetravRecipeCalculator.Desktop"
  dotnet publish -r win-x64 -c Release --self-contained /p:DebugType=None /p:DebugSymbols=false -o "%~dp0../_build/out"
popd