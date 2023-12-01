using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ChaosLib
{
    public static class DbgEngResolver
    {
        public static string GetDbgEngPath()
        {
            if (!TryGetDbgEngPath(out var path))
                throw new InvalidOperationException("Failed to resolve DbgEng folder path.");

            return path;
        }

        /// <summary>
        /// Attempts to get the directory containing the Debugging Tools for Windows.
        /// </summary>
        /// <param name="path">The directory containing the Debugging Tools for Windows.</param>
        /// <returns>True if the Debugging Tools for Windows could be found. Otherwise, false.</returns>
        public static bool TryGetDbgEngPath(out string path)
        {
            //The DbgEng that is on NuGet is the same version that ships with WinDbgX - which does not have symbols.
            //This is very problematic when attempting to debug DbgEng/DbgHelp. As such, in Debug, we try and use
            //the copy of DbgEng that is installed in the Debugging Tools for Windows (which has symbols) if it exists.
            //Otherwise, we fallback to the DbgEng/DbgHelp that we'll use in production

            var programFiles = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                Environment.GetFolderPath(Environment.SpecialFolder.Programs)
            };

            var candidates = new List<string>();

            foreach (var baseFolder in programFiles)
            {
                if (Directory.Exists(baseFolder))
                {
                    var windowsKits = Path.Combine(baseFolder, "Windows Kits");

                    if (Directory.Exists(windowsKits))
                    {
                        var windowsKitsSubfolders = Directory.GetDirectories(windowsKits);

                        foreach (var windowsKitsSubfolder in windowsKitsSubfolders)
                        {
                            GetDbgEngDebuggersPath(windowsKitsSubfolder, candidates);
                        }
                    }
                }
            }

            //Maybe it's installed to C:\Debuggers?
            GetDbgEngDebuggersPath("C:\\", candidates);

            if (candidates.Count == 0)
            {
                path = null;
                return false;
            }

            if (candidates.Count == 1)
            {
                path = Path.GetDirectoryName(candidates[0]);
                return true;
            }

            var versions = candidates.Select(FileVersionInfo.GetVersionInfo).OrderByDescending(v => new Version(v.ProductVersion)).ToArray();

            var first = versions.First();

            path = Path.GetDirectoryName(first.FileName);
            return true;
        }

        private static void GetDbgEngDebuggersPath(string parent, List<string> candidates)
        {
            var debuggersFolder = Path.Combine(parent, "Debuggers");

            if (Directory.Exists(debuggersFolder))
            {
                var archFolder = Path.Combine(debuggersFolder, IntPtr.Size == 4 ? "x86" : "x64");

                if (Directory.Exists(archFolder))
                {
                    var dbgEng = Path.Combine(archFolder, "dbgeng.dll");

                    if (File.Exists(dbgEng))
                        candidates.Add(dbgEng);
                }
            }
        }
    }
}
