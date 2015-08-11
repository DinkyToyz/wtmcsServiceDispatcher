using ColossalFramework.IO;
using System;
using System.IO;

namespace WhatThe.Mods.CitiesSkylines.ServiceDispatcher
{
    /// <summary>
    /// File system helper.
    /// </summary>
    internal static class FileSystem
    {
        /// <summary>
        /// Gets the file path.
        /// </summary>
        /// <value>
        /// The file path.
        /// </value>
        public static string FilePath
        {
            get
            {
                return Path.Combine(DataLocation.localApplicationData, "ModConfig");
            }
        }

        /// <summary>
        /// Check if file exists, with file name automagic.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>True if file exists.</returns>
        public static bool Exists(string fileName = null)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                fileName = FilePathName(".tmp");
            }
            else if (fileName[0] == '.')
            {
                fileName = FilePathName(fileName);
            }

            return File.Exists(fileName);
        }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public static string FileName(string extension = "")
        {
            return Library.Name + extension;
        }

        /// <summary>
        /// Gets the complete path.
        /// </summary>
        /// <value>
        /// The complete path.
        /// </value>
        public static string FilePathName(string fileName = null)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                fileName = FileName(".tmp");
            }
            else if (fileName[0] == '.')
            {
                fileName = FileName(fileName);
            }

            return Path.GetFullPath(Path.Combine(FilePath, fileName));
        }
    }
}
