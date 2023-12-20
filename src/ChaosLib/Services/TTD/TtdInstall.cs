using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using ClrDebug;

namespace ChaosLib.TTD
{
    class TtdInstall
    {
        public delegate HRESULT ExecuteTTTracerCommandLineDelegate(
            [In, MarshalAs(UnmanagedType.Interface)] ITtdDiagnosticsSink pSink,
            [In, MarshalAs(UnmanagedType.LPWStr)] string szCommandLine,
            [In] TTD_TRACE_MODE dwMode,
            [In, MarshalAs(UnmanagedType.LPWStr)] string szEula);

        //No EULA parameter in the system32 "Inbox" (part of Windows) version
        public delegate HRESULT ExecuteTTTracerCommandLineDelegate_System32(
            [In, MarshalAs(UnmanagedType.Interface)] ITtdDiagnosticsSink pSink,
            [In, MarshalAs(UnmanagedType.LPWStr)] string szCommandLine,
            [In] TTD_TRACE_MODE dwMode);

        private const string windowsApps = "C:\\Program Files\\WindowsApps";
        private const string system32 = "C:\\Windows\\system32";
        private const string ttdRecordDllName = "TTDRecord.dll";
        private const string ttdExeName = "TTD.exe";
        private const string tttracerExeName = "tttracer.exe";

        public TtdInstallType Type { get; }

        public string Folder { get; }

        public string TTDRecord { get; }

        public string TTTracer { get; }

        public Version Version { get; }

        public ExecuteTTTracerCommandLineDelegate Execute { get; set; }

        private TtdInstall(TtdInstallType type, string folder, string ttdRecord, string tttracer)
        {
            Type = type;
            Folder = folder;
            TTDRecord = ttdRecord;
            TTTracer = tttracer;
            Version = new Version(FileVersionInfo.GetVersionInfo(TTDRecord).FileVersion);
        }

        internal TtdInstall Clone(string newFolder)
        {
            Debug.Assert(Type == TtdInstallType.TTD || Type == TtdInstallType.WinDbg);
            return new TtdInstall(Type, newFolder, Path.Combine(newFolder, ttdRecordDllName), Path.Combine(newFolder, ttdExeName));
        }

        internal static TtdInstall GetTtdInstall()
        {
            //Get the TTD Microsoft Store install (if applicable)

            if (IntPtr.Size == 8 && Directory.Exists(windowsApps))
            {
                var ttdInstallDir = Directory.EnumerateDirectories(windowsApps, "Microsoft.TimeTravelDebugging_*_x64_*").FirstOrDefault();

                if (ttdInstallDir != null)
                {
                    var ttdExePath = Path.Combine(ttdInstallDir, ttdExeName);
                    var ttdRecordPath = Path.Combine(ttdInstallDir, ttdRecordDllName);

                    if (File.Exists(ttdExePath) && File.Exists(ttdRecordPath))
                    {
                        return new TtdInstall(TtdInstallType.TTD, ttdInstallDir, ttdRecordPath, ttdExePath);
                    }
                }
            }

            return null;
        }

        internal static TtdInstall GetWinDbgInstall()
        {
            //Get the Microsoft Store install (if applicable)

            if (Directory.Exists(windowsApps))
            {
                var windbgInstallDir = Directory.EnumerateDirectories(windowsApps, "Microsoft.WinDbg_*_x64_*").FirstOrDefault();

                if (windbgInstallDir != null)
                {
                    var ttdDir = Path.Combine(windbgInstallDir, $"{(IntPtr.Size == 4 ? "x86" : "amd64")}\\ttd");

                    if (Directory.Exists(ttdDir))
                    {
                        var ttdExePath = Path.Combine(ttdDir, ttdExeName);
                        var ttdRecordPath = Path.Combine(ttdDir, ttdRecordDllName);

                        if (File.Exists(ttdExePath) && File.Exists(ttdRecordPath))
                        {
                            return new TtdInstall(TtdInstallType.WinDbg, ttdDir, ttdRecordPath, ttdExePath);
                        }
                    }
                }
            }

            return null;
        }

        internal static TtdInstall GetSystem32Install()
        {
            //Get the system32 install (if applicable)

            var ttdExePath = Path.Combine(system32, tttracerExeName);
            var ttdRecordPath = Path.Combine(system32, ttdRecordDllName);

            if (File.Exists(ttdExePath) && File.Exists(ttdRecordPath))
            {
                return new TtdInstall(TtdInstallType.System32, system32, ttdRecordPath, ttdExePath);
            }

            return null;
        }
    }
}