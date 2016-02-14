@echo off

SET APPID=255710
SET STEAMLIBRARY=D:\Program Files\SteamLibrary

SET MODID=%1
SET SOLUTIONNAME=%2
SET TARGETFILENAME=%3
SET CONFIGURATION=%4

IF "%MODID%"=="" GOTO APPDATA
IF NOT EXIST "%STEAMLIBRARY%\steamapps\workshop\content\%APPID%" GOTO APPDATA

SET MODPATH=%STEAMLIBRARY%\steamapps\workshop\content\%APPID%\%MODID%\
IF EXIST "%MODPATH%" GOTO WORK

:APPDATA
SET MODPATH=%LOCALAPPDATA%\Colossal Order\Cities_Skylines\Addons\Mods\%SOLUTIONNAME%\
IF NOT EXIST "%MODPATH%" MKDIR "%MODPATH%"

:WORK

IF NOT EXIST ..\..\Build\SteamBBCode2MarkDown.pl GOTO NODESC
IF NOT EXIST ..\..\Dox\SteamDescription.txt GOTO NODESC
IF NOT EXIST ..\..\Dox\ReadMe.head.md GOTO NODESC

..\..\Build\SteamBBCode2MarkDown.pl ..\..\Dox\ReadMe.head.md ..\..\Dox\SteamDescription.txt > ..\..\..\README.md

:NODESC

IF "%CONFIGURATION%"=="Debug" GOTO END
IF NOT EXIST "%TARGETFILENAME%" GOTO END

IF NOT EXIST "%MODPATH%" GOTO NOGAME

IF EXIST "%MODPATH%%TARGETFILENAME%" del "%MODPATH%%TARGETFILENAME%"
xcopy /f /y "%TARGETFILENAME%" "%MODPATH%"

:NOGAME

zip -u -j -9 -o -X "..\..\..\CentralServicesDispatcherWTM.zip" "..\..\..\README.md" "..\..\Dox\PreviewImage.png" "%TARGETFILENAME%"

:END
