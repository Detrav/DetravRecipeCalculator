pushd ".."

  FOR /F %%g IN ('git rev-list --count --all') do (SET BUILD_NUMBER=%%g)

  pushd "src/DetravRecipeCalculator.Desktop"
    dotnet publish -r win-x64 -c Release --self-contained /p:DebugType=None /p:DebugSymbols=false /p:AssemblyVersion=0.5.0.%BUILD_NUMBER% -o "%~dp0../_build/out"
  popd

popd