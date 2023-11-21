@echo off

REM DLLNAME
set dllName="NoBackgroundAudio.dll"

REM Game Variable
set gameName="Lethal Company.exe"

REM Game Path
set gamePath="D:\SteamLibrary\steamapps\common\Lethal Company\"

REM Close any open instances of the game
taskkill /f /im %gameName%

REM Build the plugin
dotnet build 

REM Copy the plugin to the game folder
copy /y bin\Debug\netstandard2.1\%dllName% %gamePath%BepInEx\plugins

REM If Copy fails, exit
if %errorlevel% neq 0 (
		echo Copy failed
		pause
		exit
)

REM Start the game
start "" %gamePath%%gameName%


