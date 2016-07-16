date /t

set ROOTDIR=%~dp0

for %%f in (DSQLClassGenerator.Common DSQLClassGenerator.DotNetCore DSQLClassGenerator.Generator DSQLClassGenerator.Postgres DSQLClassGenerator.SqlServer) do (
	pushd "%ROOTDIR%\src\%%f"
	dotnet restore
	dotnet pack -c Release -o "%ROOTDIR%\build"
	popd
)