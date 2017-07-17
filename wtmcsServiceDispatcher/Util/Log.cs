using ColossalFramework;
using ColossalFramework.Plugins;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

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
                Output(Level.None, null, null, null, name.Name + " " + name.Version, AssemblyInfo.PreBuildStamps.DateTime.ToString("yyyy-MM-dd HH:mm"), Global.EnableExperiments ? "Experiments Enabled" : null, Global.EnableDevExperiments ? "Development Experiments Enabled" : null);
                Output(Level.None, null, null, null, "Cities Skylines", BuildConfig.applicationVersionFull, GetDLCString());
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
            Dev = 5
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
        /// Instanciate a new info-list data item.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="data">The data.</param>
        /// <returns>A new info-list data item.</returns>
        public static InfoList.InfoData Data(string name, params object[] data)
        {
            return new InfoList.InfoData(name, data);
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
            Output(Level.Dev, sourceObject, sourceBlock, null, messages);
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
                        using (StreamWriter logFile = OpenLogFile())
                        {
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
        /// Instanciates and fills a new information list.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <param name="data">The data, either as InfoList.InfoData, or in pairs of name, data.</param>
        /// <returns>
        /// A new information list.
        /// </returns>
        public static InfoList List(string prefix, params object[] data)
        {
            InfoList info = new InfoList(prefix);

            object[] objects;
            if (data.Length == 1 && data[0] is object[] && (data[0] as object[]).Length > 0)
            {
                objects = data[0] as object[];
            }
            else
            {
                objects = data;
            }

            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] is InfoList.InfoData)
                {
                    info.Add((InfoList.InfoData)objects[i]);
                }
                else if (i == objects.Length - 1)
                {
                    info.Add(null, objects[i]);
                }
                else
                {
                    info.Add(ObjectToString(objects[i]), objects[i + 1]);
                    i++;
                }
            }

            return info;
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
                    else if (sourceObject is Dispatcher && ((Dispatcher)sourceObject).Service.ServiceType != ServiceHelper.ServiceType.None)
                    {
                        msg.Append('<').Append(((Dispatcher)sourceObject).Service.ServiceType.ToString());
                    }
                    else if (sourceObject is DispatchService && ((DispatchService)sourceObject).ServiceType != ServiceHelper.ServiceType.None)
                    {
                        msg.Append('<').Append(((Dispatcher)sourceObject).Service.ServiceType.ToString() + "Service");
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

                    string message = null;

                    try
                    {
                        message = ObjectToString(messages[i]);
                    }
                    catch (Exception ex)
                    {
                        message = "(unable to log value: " + messages[i].GetType().ToString() + ", " + ex.GetType().ToString();
                        if (!String.IsNullOrEmpty(ex.Message))
                        {
                            message += ", " + ex.Message;
                        }
                        message += ")";
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

                if (level != Level.None && level <= Level.Warning && level <= logLevel)
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

                msg.Insert(0, (((level == Level.None) ? "" : level.ToString()) + ":").PadRight(9));

                if (level != Level.None && level != Level.Dev && level <= logLevel && (level < Level.Debug || !LogToFile))
                {
                    try
                    {
                        UnityEngine.Debug.Log(msg.CleanNewLines());
                    }
                    catch
                    {
                    }
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
                            using (StreamWriter logFile = OpenLogFile())
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
        /// Gets the DLC string.
        /// </summary>
        /// <value>
        /// The DLC string.
        /// </value>
        private static string GetDLCString()
        {
            return String.Join(
                ", ",
                ((IList<SteamHelper.DLC>)Enum.GetValues(typeof(SteamHelper.DLC)))
                .WhereSelectToArray(dlc => dlc != SteamHelper.DLC.None && SteamHelper.IsDLCOwned(dlc), dlc => dlc.ToString()));
        }

        /// <summary>
        /// Gets the mod string.
        /// </summary>
        /// <param name="enabled">if set to <c>true</c> return only enabled mods, otherwise return only disabled mods.</param>
        /// <returns>A comma separated list of mod names.</returns>
        private static string GetModString(bool enabled)
        {
            return String.Join(", ", Singleton<PluginManager>.instance.GetPluginsInfo().WhereSelectToArray(pi => pi.isEnabled, pi => pi.name));
        }

        /// <summary>
        /// Converts objects to a list of readable strings.
        /// </summary>
        /// <param name="objects">The objects.</param>
        /// <returns>
        /// A list of readable strings.
        /// </returns>
        private static IEnumerable<string> ObjectsToStrings(params object[] objects)
        {
            foreach (object obj in objects)
            {
                yield return ObjectToString(obj);
            }
        }

        /// <summary>
        /// Converts a list of objects to a list of readable strings.
        /// </summary>
        /// <typeparam name="T">The type of the objects.</typeparam>
        /// <param name="objects">The objects.</param>
        /// <returns>
        /// A list of readable strings.
        /// </returns>
        private static IEnumerable<string> ObjectsToStrings<T>(IEnumerable<T> objects)
        {
            foreach (T obj in objects)
            {
                yield return ObjectToString(obj);
            }
        }

        /// <summary>
        /// Converts object to readable string.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        /// A readable string.
        /// </returns>
        private static string ObjectToString(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            string str;

            if (obj is string)
            {
                str = (string)obj;
            }
            else if (obj is float)
            {
                str = ((float)obj).ToString("#,0.##", CultureInfo.InvariantCulture);
            }
            else if (obj is double)
            {
                str = ((double)obj).ToString("#,0.##", CultureInfo.InvariantCulture);
            }
            else if (obj is Vector3)
            {
                str = VectorToString((Vector3)obj);
            }
            else if (obj is Vector2)
            {
                str = VectorToString((Vector2)obj);
            }
            else if (obj is Vector4)
            {
                str = VectorToString((Vector4)obj);
            }
            else if (obj is IEnumerable<InfoList.InfoData>)
            {
                str = (new InfoList(obj as IEnumerable<InfoList.InfoData>)).ToString();
            }
            else if (obj is IEnumerable<UInt64>)
            {
                str = String.Join(", ", ObjectsToStrings<UInt64>((IEnumerable<UInt64>)obj).ToArray());
            }
            else
            {
                str = obj.ToString();
            }

            return (str == null) ? str : str.Trim();
        }

        /// <summary>
        /// Converts object to a list of readable strings.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        /// A list of readable strings.
        /// </returns>
        private static IEnumerable<string> ObjectToStrings(object obj)
        {
            if (obj is IEnumerable<object>)
            {
                foreach (object item in obj as IEnumerable<object>)
                {
                    yield return ObjectToString(item);
                }
            }
            else
            {
                yield return ObjectToString(obj);
            }
        }

        /// <summary>
        /// Opens the log file.
        /// </summary>
        /// <returns>The open log file writer.</returns>
        private static StreamWriter OpenLogFile()
        {
            string filePathName = FileSystem.FilePathName(".log");
            string filePath = Path.GetDirectoryName(filePathName);
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            return new StreamWriter(filePathName, logFileCreated);
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
        /// Converts vector to readable string.
        /// </summary>
        /// <param name="vector">The vector.</param>
        /// <returns>A readable string.</returns>
        private static string VectorToString(Vector4 vector)
        {
            return (new StringBuilder())
                    .Append('(')
                    .Append(vector.x.ToString("#,0.###", CultureInfo.InvariantCulture))
                    .Append(", ")
                    .Append(vector.y.ToString("#,0.###", CultureInfo.InvariantCulture))
                    .Append(", ")
                    .Append(vector.z.ToString("#,0.###", CultureInfo.InvariantCulture))
                    .Append(", ")
                    .Append(vector.w.ToString("#,0.###", CultureInfo.InvariantCulture))
                    .Append(')')
                    .ToString();
        }

        /// <summary>
        /// Converts vector to readable string.
        /// </summary>
        /// <param name="vector">The vector.</param>
        /// <returns>A readable string.</returns>
        private static string VectorToString(Vector3 vector)
        {
            return (new StringBuilder())
                    .Append('(')
                    .Append(vector.x.ToString("#,0.###", CultureInfo.InvariantCulture))
                    .Append(", ")
                    .Append(vector.y.ToString("#,0.###", CultureInfo.InvariantCulture))
                    .Append(", ")
                    .Append(vector.z.ToString("#,0.###", CultureInfo.InvariantCulture))
                    .Append(')')
                    .ToString();
        }

        /// <summary>
        /// Converts vector to readable string.
        /// </summary>
        /// <param name="vector">The vector.</param>
        /// <returns>A readable string.</returns>
        private static string VectorToString(Vector2 vector)
        {
            return (new StringBuilder())
                    .Append('(')
                    .Append(vector.x.ToString("#,0.###", CultureInfo.InvariantCulture))
                    .Append(", ")
                    .Append(vector.y.ToString("#,0.###", CultureInfo.InvariantCulture))
                    .Append(')')
                    .ToString();
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
            /// Initializes a new instance of the <see cref="InfoList"/> class.
            /// </summary>
            /// <param name="data">The data.</param>
            public InfoList(InfoData data)
            {
                this.Add(data);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="InfoList"/> class.
            /// </summary>
            /// <param name="data">The data.</param>
            public InfoList(IEnumerable<InfoData> data)
            {
                this.Add(data);
            }

            /// <summary>
            /// Adds the info to the list.
            /// </summary>
            /// <param name="data">The data.</param>
            public void Add(IEnumerable<InfoData> data)
            {
                foreach (InfoData item in data)
                {
                    this.Add(item);
                }
            }

            /// <summary>
            /// Adds the info to the list.
            /// </summary>
            /// <param name="data">The data.</param>
            public void Add(InfoData data)
            {
                this.Add(data.Name, data.Data);
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

                this.AddData(name, data);
            }

            /// <summary>
            /// Adds the info to the list.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="data">The data.</param>
            public void AddData(string name, object[] data)
            {
                int dc = 0;

                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i] == null)
                    {
                        continue;
                    }

                    try
                    {
                        IEnumerable<string> strings;

                        if (data[i] is IEnumerable<string>)
                        {
                            strings = data[i] as IEnumerable<string>;
                        }
                        else
                        {
                            strings = new string[] { ObjectToString(data[i]) };
                        }

                        bool sa = false;
                        foreach (string str in strings)
                        {
                            if (str == null)
                            {
                                continue;
                            }

                            if (!sa)
                            {
                                this.AddNameOrSeparator(name, dc);

                                sa = true;
                                dc++;
                            }

                            this.info.Append(escapeRex.Replace(str.Trim(), "^$1"));
                        }
                    }
                    catch (Exception ex)
                    {
                        this.AddNameOrSeparator(name, dc);
                        this.info.Append("(unable to show value: ").Append(data[i].GetType().ToString()).Append(", ").Append(ex.GetType().ToString());
                        if (!String.IsNullOrEmpty(ex.Message))
                        {
                            info.Append(", ").Append(ex.Message);
                        }
                        info.Append(")");

                        dc++;
                    }
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

            /// <summary>
            /// Named item for info list for log lines.
            /// </summary>
            public struct InfoData
            {
                /// <summary>
                /// The data.
                /// </summary>
                public readonly object[] Data;

                /// <summary>
                /// The name.
                /// </summary>
                public readonly string Name;

                /// <summary>
                /// Initializes a new instance of the <see cref="InfoData"/> struct.
                /// </summary>
                /// <param name="name">The name.</param>
                /// <param name="data">The data.</param>
                public InfoData(string name, params object[] data)
                {
                    this.Name = name;
                    this.Data = new object[data.Length];

                    if (data.Length > 0)
                    {
                        data.CopyTo(this.Data, 0);
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
                    return (new InfoList(this)).ToString();
                }
            }
        }
    }
}