# Central Services Dispatcher (WtM)

This is a mod for Cities: Skylines.

The mod changes the dispatch behaviour of at least one service.

## Files

Settings and logs are stored in the directory "ModConfig" in `<DataLocation.localApplicationData>`.
On windows that usually means `<AppData>\Local\Colossal Order\Cities_Skylines\ModConfig`, where `<AppData>` usually is at `C:\Users\<UserName>\AppData`, where `<UserName>` is the currently logged in user.
So, if your user name is _Gruhimi Klampamejmera_, the files might be in `C:\Users\Gruhimi Klampamejmera\AppData\Local\Colossal Order\Cities_Skylines\ModConfig`.

Settings are stored in `wtmcsServiceDispatcher.xml`.
When logging to file, logs will be written to `wtmcsServiceDispatcher.log`.

## Logging

Create one or more the following files in order to enable debug log stuff (wich might slow things down quite a bit).

- `wtmcsServiceDispatcher.debug`
  Enables logging debug data to mod log file, and slightly more logging to the games standard log destinations.

- `wtmcsServiceDispatcher.debug.dev`
  Log more stuff to mod log file.

- `wtmcsServiceDispatcher.debug.names`
  Log object names in some debug log entries.

- `wtmcsServiceDispatcher.debug.lists`
  Log object debug lists to mod log file at startup. What this actually does, if anything, may differ between builds.

## Dispatch Strategies

At the time of writing, the following dispatch strategies are available.

 - **First first**:
   All buldings regardless of range.

 - **Forgotten first**:
   Forgotten buidlings in range, followed by forgotten buildings out of range, buildings in range and finally problematic buildings in or out of range.

 - **In range**:
   Buildings in range followed by forgotten out of range.

 - **In range first**:
   Buildings in range followed by problematic buildings in or out of range.

 - **Problematic first**:
   Problematic buildings in range followed by problematic buildings out of range and finally buildings in range.

 - **Very problematic first**:
   Very problematic buildings in range followed by very problematic buildings out of range, buildings in range and finally problematic buildings in or out of range.

 - **Custom**:
   Custom strategy manually defined in the configuration file.

Services are dispatched based on problem magnitude within the strategy rules.