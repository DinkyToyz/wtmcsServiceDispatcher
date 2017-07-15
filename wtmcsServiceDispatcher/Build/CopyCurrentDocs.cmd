@echo off

SET GHPages=..\..\..\..\GH-Pages

SET "current_version="
FOR /F "tokens=1,2,3 delims=: " %%A IN (%GHPages%\_config.yml) DO (
    IF "%%A"=="current_version" SET "current_version=%%B" & goto found
)
:found

IF "%current_version%"=="" (
    rem robocopy "%GHPages%\docs" ..\..\Dox\Documentation /MIR > nul
) ELSE (
    robocopy "%GHPages%\docs\%current_version%" ..\..\Dox\Documentation /MIR > nul
)
