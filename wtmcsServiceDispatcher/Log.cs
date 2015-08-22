using ColossalFramework.Plugins;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// Log helper.
    /// </summary>
    internal static class Log
    {
        /// <summary>
        /// Log a lot of stuff.
        /// </summary>
        public static readonly bool LogALot;

        /// <summary>
        /// The number of lines to buffer.
        /// </summary>
        public static int BufferLines = 5120;

        /// <summary>
        /// The last flush of buffer.
        /// </summary>
        public static uint LastFlush = 0;

        /// <summary>
        /// The log level.
        /// </summary>
        public static Level LogLevel;

        /// <summary>
        /// True for logging to file.
        /// </summary>
        public static bool LogToFile = true;

        /// <summary>
        /// The line buffer.
        /// </summary>
        private static List<string> lineBuffer;

        /// <summary>
        /// The log info all to file.
        /// </summary>
        private static bool LogAllToFile;

        /// <summary>
        /// True when log file has been created.
        /// </summary>
        private static bool logFileCreated = false;

        /// <summary>
        /// Initializes the <see cref="Log"/> class.
        /// </summary>
        static Log()
        {
            if (Library.IsDebugBuild || FileSystem.Exists(".debug"))
            {
                Log.LogLevel = Log.Level.Info;
                Log.LogToFile = true;
                Log.LogAllToFile = true;

                if (Library.IsDebugBuild)
                {
                    Log.LogALot = true;
                    Log.lineBuffer = new List<string>();
                }
                else
                {
                    Log.LogALot = false;
                    Log.lineBuffer = null;
                }
            }
            else
            {
                Log.LogLevel = Log.Level.Warning;
                Log.LogToFile = false;
                Log.LogAllToFile = false;
                Log.LogALot = false;
                Log.lineBuffer = null;
            }

            try
            {
                AssemblyName name = Assembly.GetExecutingAssembly().GetName();
                Output(Level.None, null, null, null, name.Name + " " + name.Version);
            }
            catch { }
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
        /// Gets or sets a value indicating whether this <see cref="Log"/> is buffered.
        /// </summary>
        /// <value>
        ///   <c>true</c> if buffered; otherwise, <c>false</c>.
        /// </value>
        public static bool Buffer
        {
            get
            {
                return (lineBuffer != null);
            }
            set
            {
                if (value)
                {
                    if (lineBuffer == null)
                    {
                        lineBuffer = new List<string>();
                    }
                }
                else
                {
                    if (lineBuffer != null)
                    {
                        lineBuffer = null;
                    }
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
                            logFile.Write(String.Join("", lineBuffer.ToArray()));
                            logFile.Close();
                        }

                        logFileCreated = true;
                    }
                    catch { }
                    finally
                    {
                        LastFlush = Global.CurrentFrame;
                        lineBuffer.Clear();
                    }
                }
            }
            catch { }
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
        /// Comvert log level to message type.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns></returns>
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
        /// Do nothing (except trigger the class constructor unless it has run allready).
        /// </summary>
        public static void NoOp()
        { }

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
            if (level > LogLevel && !LogAllToFile)
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
                        message = ((float)messages[i]).ToString("0,0.##", CultureInfo.InvariantCulture);
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

                if (level != Level.None && level != Level.All && level <= LogLevel)
                {
                    try
                    {
                        DebugOutputPanel.AddMessage(level.MessageType(), msg.CleanNewLines());
                    }
                    catch { }
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
                            if (level <= LogLevel) UnityEngine.Debug.Log(msg.CleanNewLines());
                            break;

                        case Level.Warning:
                            msg.Insert(0, "Warning: ");
                            if (level <= LogLevel) UnityEngine.Debug.LogWarning(msg.CleanNewLines());
                            break;

                        case Level.Error:
                            msg.Insert(0, "Error:   ");
                            if (level <= LogLevel) UnityEngine.Debug.LogError(msg.CleanNewLines());
                            break;

                        case Level.None:
                        case Level.All:
                            msg.Insert(0, "         ");
                            break;

                        default:
                            msg.Insert(0, "Debug:   ");
                            if (level <= LogLevel) UnityEngine.Debug.Log(msg.CleanNewLines());
                            break;
                    }
                }
                catch { }

                if (LogToFile)
                {
                    try
                    {
                        msg.Insert(0, ' ').Insert(0, now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                        msg.Append("\n");

                        if (lineBuffer != null)
                        {
                            lineBuffer.Add(msg.ConformNewlines());
                            if (level >= Level.Warning || lineBuffer.Count >= BufferLines)
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
                    catch { }
                }
            }
            catch { }
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
    }
}