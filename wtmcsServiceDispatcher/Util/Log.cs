using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ColossalFramework.Plugins;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Log helper.
    /// </summary>
    internal static class Log
    {
        /// <summary>
        /// The number of lines to buffer.
        /// </summary>
        private static int bufferLines = 5120;

        /// <summary>
        /// The line buffer.
        /// </summary>
        private static List<string> lineBuffer = null;

        /// <summary>
        /// The log info all to file.
        /// </summary>
        private static bool logAllToFile;

        /// <summary>
        /// Log a lot of stuff.
        /// </summary>
        private static bool logALot;

        /// <summary>
        /// Log debug stuff.
        /// </summary>
        private static bool logDebug;

        /// <summary>
        /// Log the debug lists.
        /// </summary>
        private static bool logDebugLists;

        /// <summary>
        /// True when log file has been created.
        /// </summary>
        private static bool logFileCreated = false;

        /// <summary>
        /// The log level.
        /// </summary>
        private static Level logLevel;

        /// <summary>
        /// Log object names (slow).
        /// </summary>
        private static bool logNames;

        /// <summary>
        /// True for logging to file.
        /// </summary>
        private static bool logToFile;

        /// <summary>
        /// The log to file value has been set.
        /// </summary>
        private static bool logToFileSet = false;

        /// <summary>
        /// Initializes static members of the <see cref="Log"/> class.
        /// </summary>
        static Log()
        {
            Log.LastFlush = 0;

            Log.LogNames = FileSystem.Exists(".debug.names");
            Log.logDebugLists = FileSystem.Exists(".debug.lists");
            Log.logALot = FileSystem.Exists(".debug.dev");
            Log.logDebug = Library.IsDebugBuild || FileSystem.Exists(".debug");

            SetLogValues();

            try
            {
                AssemblyName name = Assembly.GetExecutingAssembly().GetName();
                Output(Level.None, null, null, null, name.Name + " " + name.Version, Global.EnableExperiments ? "Experiments Enabled" : null);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Log levels.
        /// </summary>
        public enum Level
        {
            /// <summary>
            /// Log nothing.
            /// </summary>
            None = 0,

            /// <summary>
            /// Log errors.
            /// </summary>
            Error = 1,

            /// <summary>
            /// Log warnings.
            /// </summary>
            Warning = 2,

            /// <summary>
            /// Log informational messages.
            /// </summary>
            Info = 3,

            /// <summary>
            /// Log debug messages.
            /// </summary>
            Debug = 4,

            /// <summary>
            /// Log all messages.
            /// </summary>
            All = 5
        }

        /// <summary>
        /// Gets the last flush of buffer stamp.
        /// </summary>
        public static uint LastFlush
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to log a lot of stuff.
        /// </summary>
        /// <value>
        /// <c>true</c> if a lot should be logged; otherwise, <c>false</c>.
        /// </value>
        public static bool LogALot
        {
            get
            {
                return Log.logALot;
            }

            set
            {
                if (value != Log.logALot)
                {
                    Log.logALot = value;
                    SetLogValues();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to log debug stuff.
        /// </summary>
        /// <value>
        ///   <c>true</c> if debug stuff should be logged; otherwise, <c>false</c>.
        /// </value>
        public static bool LogDebug
        {
            get
            {
                return logDebug;
            }

            set
            {
                if (value != logDebug)
                {
                    logDebug = value;
                    SetLogValues();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to log the debug lists.
        /// </summary>
        public static bool LogDebugLists
        {
            get
            {
                return Log.logDebugLists;
            }

            set
            {
                if (value != Log.logDebugLists)
                {
                    Log.logDebugLists = value;
                    SetLogValues();
                }
            }
        }

        /// <summary>
        /// Gets the log level.
        /// </summary>
        public static Level LogLevel
        {
            get
            {
                return Log.logLevel;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to object names (slow).
        /// </summary>
        public static bool LogNames
        {
            get
            {
                return Log.logNames;
            }

            set
            {
                if (value != Log.logNames)
                {
                    Log.logNames = value;
                    SetLogValues();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to log to file.
        /// </summary>
        public static bool LogToFile
        {
            get
            {
                return Log.logToFile;
            }

            set
            {
                logToFileSet = true;

                if (value != Log.logToFile)
                {
                    Log.logToFile = value;
                    SetLogValues();
                }
            }
        }

        /// <summary>
        /// Outputs the specified debugging message.
        /// </summary>
        /// <param name="sourceObject">The source object.</param>
        /// <param name="sourceBlock">The source block.</param>
        /// <param name="messages">The messages.</param>
        public static void Debug(object sourceObject, string sourceBlock, params object[] messages)
        {
            Output(Level.Debug, sourceObject, sourceBlock, null, messages);
        }

        /// <summary>
        /// Outputs the specified warning message.
        /// </summary>
        /// <param name="sourceObject">The source object.</param>
        /// <param name="sourceBlock">The source block.</param>
        /// <param name="messages">The messages.</param>
        public static void DevDebug(object sourceObject, string sourceBlock, params object[] messages)
        {
            Output(Level.All, sourceObject, sourceBlock, null, messages);
        }

        /// <summary>
        /// Outputs the specified error message.
        /// </summary>
        /// <param name="sourceObject">The source object.</param>
        /// <param name="sourceBlock">The source block.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messages">The messages.</param>
        public static void Error(object sourceObject, string sourceBlock, Exception exception, params object[] messages)
        {
            Output(Level.Error, sourceObject, sourceBlock, exception, messages);
        }

        /// <summary>
        /// Flushes the buffer.
        /// </summary>
        public static void FlushBuffer()
        {
            try
            {
                if (lineBuffer != null && lineBuffer.Count > 0)
                {
                    try
                    {
                        using (StreamWriter logFile = new StreamWriter(FileSystem.FilePathName(".log"), logFileCreated))
                        {
                            if (Log.logALot)
                                lineBuffer.Add((DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " Flush\n").ConformNewlines());

                            logFile.Write(String.Join("", lineBuffer.ToArray()));
                            logFile.Close();
                        }

                        logFileCreated = true;
                    }
                    catch
                    {
                    }
                    finally
                    {
                        LastFlush = Global.CurrentFrame;
                        lineBuffer.Clear();
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Outputs the specified informational message.
        /// </summary>
        /// <param name="sourceObject">The source object.</param>
        /// <param name="sourceBlock">The source block.</param>
        /// <param name="messages">The messages.</param>
        public static void Info(object sourceObject, string sourceBlock, params object[] messages)
        {
            Output(Level.Info, sourceObject, sourceBlock, null, messages);
        }

        /// <summary>
        /// Convert log level to message type.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns>The message type.</returns>
        public static PluginManager.MessageType MessageType(this Level level)
        {
            switch (level)
            {
                case Level.None:
                case Level.Error:
                    return PluginManager.MessageType.Error;

                case Level.Warning:
                    return PluginManager.MessageType.Warning;

                default:
                    return PluginManager.MessageType.Message;
            }
        }

        /// <summary>
        /// Do nothing (except trigger the class constructor unless it has run already).
        /// </summary>
        public static void NoOp()
        {
        }

        /// <summary>
        /// Outputs the specified message.
        /// </summary>
        /// <param name="level">The message level.</param>
        /// <param name="sourceObject">The source object.</param>
        /// <param name="sourceBlock">The source block.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messages">The messages.</param>
        public static void Output(Level level, object sourceObject, string sourceBlock, Exception exception, params object[] messages)
        {
            if (level > logLevel && !logAllToFile)
            {
                return;
            }

            try
            {
                DateTime now = DateTime.Now;

                StringBuilder msg = new StringBuilder();
                if (sourceObject != null)
                {
                    if (sourceObject is String || sourceObject is string || sourceObject is ValueType)
                    {
                        if (!String.IsNullOrEmpty(sourceObject.ToString()))
                        {
                            msg.Append('<').Append(sourceObject.ToString());
                        }
                    }
                    else if (sourceObject is Type)
                    {
                        msg.Append('<').Append(((Type)sourceObject).Name);
                    }
                    else if (sourceObject is Dispatcher && ((Dispatcher)sourceObject).DispatcherType != Dispatcher.DispatcherTypes.None)
                    {
                        msg.Append('<').Append(((Dispatcher)sourceObject).DispatcherType.ToString());
                    }
                    else
                    {
                        msg.Append('<').Append(sourceObject.GetType().Name);
                    }
                }
                if (!String.IsNullOrEmpty(sourceBlock))
                {
                    if (msg.Length == 0)
                    {
                        msg.Append('<');
                    }
                    else
                    {
                        msg.Append('.');
                    }
                    msg.Append(sourceBlock);
                }
                if (msg.Length > 0)
                {
                    msg.Append('>');
                }

                int mc = 0;
                for (int i = 0; i < messages.Length; i++)
                {
                    if (messages[i] == null)
                    {
                        continue;
                    }

                    string message;

                    if (messages[i] is string)
                    {
                        message = (string)messages[i];
                    }
                    else if (messages[i] is float)
                    {
                        message = ((float)messages[i]).ToString("#,0.##", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        message = messages[i].ToString();
                    }

                    if (message == null)
                    {
                        continue;
                    }

                    if (mc > 0)
                    {
                        msg.Append("; ");
                    }
                    else if (msg.Length > 0)
                    {
                        msg.Append(' ');
                    }

                    msg.Append(message.Trim());
                    mc++;
                }

                if (exception != null)
                {
                    if (msg.Length > 0)
                    {
                        msg.Append(' ');
                    }
                    msg.Append("[").Append(exception.GetType().Name).Append("] ").Append(exception.Message.Trim());
                }

                if (msg.Length == 0)
                {
                    return;
                }

                if (Global.LevelLoaded)
                {
                    msg.Insert(0, ' ').Insert(0, Global.CurrentFrame.ToString()).Insert(0, '@');
                }

                msg.Insert(0, "] ").Insert(0, Library.Name).Insert(0, "[");

                if (level != Level.None && level != Level.All && level <= logLevel)
                {
                    try
                    {
                        DebugOutputPanel.AddMessage(level.MessageType(), msg.CleanNewLines());
                    }
                    catch
                    {
                    }
                }

                if (exception != null)
                {
                    msg.Append("\n").Append(exception.StackTrace).Append("\n");
                    while (exception.InnerException != null)
                    {
                        exception = exception.InnerException;
                        msg.Append("\nInner: [").Append(exception.GetType().Name).Append("] ").Append(exception.Message).Append("\n").Append(exception.StackTrace).Append("\n");
                    }
                }

                try
                {
                    switch (level)
                    {
                        case Level.Info:
                            msg.Insert(0, "Info:    ");
                            if (level <= logLevel)
                                UnityEngine.Debug.Log(msg.CleanNewLines());
                            break;

                        case Level.Warning:
                            msg.Insert(0, "Warning: ");
                            if (level <= logLevel)
                                UnityEngine.Debug.LogWarning(msg.CleanNewLines());
                            break;

                        case Level.Error:
                            msg.Insert(0, "Error:   ");
                            if (level <= logLevel)
                                UnityEngine.Debug.LogError(msg.CleanNewLines());
                            break;

                        case Level.None:
                        case Level.All:
                            msg.Insert(0, "         ");
                            break;

                        default:
                            msg.Insert(0, "Debug:   ");
                            if (level <= logLevel)
                                UnityEngine.Debug.Log(msg.CleanNewLines());
                            break;
                    }
                }
                catch
                {
                }

                if (Log.logToFile)
                {
                    try
                    {
                        msg.Insert(0, ' ').Insert(0, now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                        msg.Append("\n");

                        if (lineBuffer != null)
                        {
                            lineBuffer.Add(msg.ConformNewlines());
                            if (level <= Level.Warning || lineBuffer.Count >= bufferLines)
                            {
                                FlushBuffer();
                            }
                        }
                        else
                        {
                            using (StreamWriter logFile = new StreamWriter(FileSystem.FilePathName(".log"), logFileCreated))
                            {
                                logFile.Write(msg.ConformNewlines());
                                logFile.Close();
                            }

                            logFileCreated = true;
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Outputs the specified warning message.
        /// </summary>
        /// <param name="sourceObject">The source object.</param>
        /// <param name="sourceBlock">The source block.</param>
        /// <param name="messages">The messages.</param>
        public static void Warning(object sourceObject, string sourceBlock, params object[] messages)
        {
            Output(Level.Warning, sourceObject, sourceBlock, null, messages);
        }

        /// <summary>
        /// Sets the log values.
        /// </summary>
        private static void SetLogValues()
        {
            if (Log.logALot)
            {
                Log.logDebug = true;
            }

            if (Log.logDebug)
            {
                Log.logLevel = Log.Level.Info;
            }
            else
            {
                Log.logLevel = Log.Level.Warning;
            }

            if (logDebug || Log.logALot || Log.logDebugLists)
            {
                if (!Log.logToFileSet)
                {
                    Log.logToFile = true;
                }

                Log.logAllToFile = Log.logToFile;
            }
            else
            {
                if (!Log.logToFileSet)
                {
                    Log.logToFile = false;
                }

                Log.logAllToFile = false;
            }

            try
            {
                if (Log.logToFile)
                {
                    if (lineBuffer == null)
                    {
                        Log.lineBuffer = new List<string>();
                    }
                }
                else
                {
                    Log.logAllToFile = false;

                    if (Log.lineBuffer != null)
                    {
                        Log.FlushBuffer();
                        Log.lineBuffer = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.logAllToFile = false;
                Log.logToFile = false;
                Log.lineBuffer = null;

                Log.Error(typeof(Log), "SetLogValues", ex);
            }
        }

        /// <summary>
        /// Named info list for log lines.
        /// </summary>
        public class InfoList
        {
            /// <summary>
            /// The string escape regex.
            /// </summary>
            private static Regex escapeRex = new Regex("([;^\"])");

            /// <summary>
            /// The information list.
            /// </summary>
            private StringBuilder info = new StringBuilder();

            /// <summary>
            /// The prefix length.
            /// </summary>
            private int prefixLen = 0;

            /// <summary>
            /// Initializes a new instance of the <see cref="InfoList"/> class.
            /// </summary>
            /// <param name="prefix">The prefix.</param>
            public InfoList(string prefix = null)
            {
                if (prefix != null)
                {
                    this.info.Append(prefix);
                    this.prefixLen = prefix.Length;
                }
            }

            /// <summary>
            /// Adds the info to the list.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="data">The data.</param>
            public void Add(string name, params object[] data)
            {
                if (data.Length == 0)
                {
                    this.AddNameOrSeparator(name);
                    return;
                }

                int dc = 0;

                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i] == null)
                    {
                        continue;
                    }

                    if (data[i] is IEnumerable<string>)
                    {
                        bool sa = false;
                        foreach (string str in (IEnumerable<string>)data[i])
                        {
                            if (str == null)
                            {
                                continue;
                            }

                            if (!sa)
                            {
                                this.AddNameOrSeparator(name, dc);
                                sa = true;
                            }

                            this.info.Append(escapeRex.Replace(str.Trim(), "^$1"));
                        }

                        if (!sa)
                        {
                            continue;
                        }
                    }
                    else if (data[i] is string)
                    {
                        this.AddNameOrSeparator(name, dc);
                        this.info.Append(escapeRex.Replace(((string)data[i]).Trim(), "^$1"));
                    }
                    else if (data[i] is float)
                    {
                        this.AddNameOrSeparator(name, dc);
                        this.info.Append(((float)data[i]).ToString("#,0.##", CultureInfo.InvariantCulture));
                    }
                    else if (data[i] is int || data[i] is Int16 || data[i] is Int32 || data[i] is Int64 || data[i] is short || data[i] is byte ||
                             data[i] is uint || data[i] is UInt16 || data[i] is UInt32 || data[i] is UInt64 || data[i] is ushort)
                    {
                        this.AddNameOrSeparator(name, dc);
                        this.info.Append(data[i].ToString());
                    }
                    else
                    {
                        string text = data[i].ToString();
                        if (text == null)
                        {
                            continue;
                        }

                        this.AddNameOrSeparator(name, dc);
                        this.info.Append(escapeRex.Replace(text.Trim(), "^$1"));
                    }

                    dc++;
                }
            }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return this.info.ToString();
            }

            /// <summary>
            /// Adds the name or separator if it should.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="paramPos">The parameter position.</param>
            private void AddNameOrSeparator(string name, int paramPos = -1)
            {
                if (paramPos <= 0)
                {
                    if (this.info.Length > this.prefixLen)
                    {
                        this.info.Append("; ");
                    }

                    this.info.Append(escapeRex.Replace(name, "^$1"));

                    if (paramPos == 0)
                    {
                        this.info.Append('=');
                    }
                }
                else
                {
                    this.info.Append(", ");
                }
            }
        }
    }
}