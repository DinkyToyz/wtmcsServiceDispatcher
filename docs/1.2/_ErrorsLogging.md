When reporting severe errors, please upload [the games complete log file](http://steamcommunity.com/sharedfiles/filedetails/?id=463645931){:target="_blank"} and/or the separate log file (see below) somewhere and post a link.

The mod logs to [the games normal output log](http://steamcommunity.com/sharedfiles/filedetails/?id=463645931){:target="_blank"}, and can also log to a separate log file, wtmcsServiceDispatcher.log, stored in the same directory as the settings.

Create one or more of the following files in the settings directory in order to enable debug log stuff (which might slow things down quite a bit) and logging to file.

- **wtmcsServiceDispatcher.debug**: 
  Enables logging debug data to mod log file, and slightly more logging to the games standard log destinations.

- **wtmcsServiceDispatcher.debug.dev**: 
  Log more stuff to mod log file.

- **wtmcsServiceDispatcher.debug.names**: 
  Log object names in some debug log entries.

- **wtmcsServiceDispatcher.debug.lists**: 
  Log object debug lists to mod log file at startup. What this actually does, if anything, may differ between builds.
