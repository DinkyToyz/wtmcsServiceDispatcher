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
        /// Check if file exists, with file name automatic.
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
        /// <param name="extension">The extension.</param>
        /// <returns>The name of the file.</returns>
        public static string FileName(string extension = "")
        {
            return Library.Name + extension;
        }

        /// <summary>
        /// Gets the complete path.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>The complete path.</returns>
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
