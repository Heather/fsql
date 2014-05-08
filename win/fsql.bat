@echo off
cls
SET EnableNuGetPackageRestore=true
if %PROCESSOR_ARCHITECTURE%==x86 (
         set MSBUILD="%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
) else ( set MSBUILD="%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe"
)
if not exist .nuget\nuget.exe %MSBUILD% .nuget\nuget.targets /t:CheckPrerequisites
if not exist packages\FAKE\tools\Fake.exe ( 
	echo Downloading FAKE...
	".nuget\NuGet.exe" "install" "FAKE" "-OutputDirectory" "packages" "-ExcludeVersion" "-Prerelease"
)

if not exist "..\src\Design\bin\Release\WinForms.dll" ( 
	echo Building Design WinForms...
    %MSBUILD% "..\src\Design\WinForms.fsproj" /p:TargetFramework=net40 /p:Configuration=Release
)

if not exist "..\src\FSQL\bin\Release\FSQL.dll" ( 
	echo Building FSQL...
    %MSBUILD% "..\src\FSQL\FSQL.fsproj" /p:TargetFramework=net40 /p:Configuration=Release
)

start packages\FAKE\tools\Fsi.exe ..\fsql.fsx