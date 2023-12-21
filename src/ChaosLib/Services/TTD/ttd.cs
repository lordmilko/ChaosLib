using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using ChaosLib.TTD;
using ClrDebug;

namespace ChaosLib
{
    public class ttd
    {
        private static TtdInstall install;
        private static TtdInstall windbgInstall;

        private static TtdTraceSession currentSession;

        public static TtdTraceSession CurrentSession
        {
            get => currentSession;
            set
            {
                if (value != null)
                {
                    if (currentSession != null)
                        throw new InvalidOperationException($"Cannot set {nameof(CurrentSession)}: an existing session already exists");
                }

                currentSession = value;
            }
        }

        static ttd()
        {
            var ttdInstall = TtdInstall.GetTtdInstall();
            windbgInstall = TtdInstall.GetWinDbgInstall();
            var system32Install = TtdInstall.GetSystem32Install();

            void CopyInstallFiles()
            {
                //You can't call LoadLibrary against DLLs in the WindowsApps folder for some reason. Neither the DLLs nor EXEs have
                //execute permissions so its a bit strange. So, plan B: copy the TTD directory to us!

                var appDir = AppContext.BaseDirectory;
                var appTtdDir = Path.Combine(appDir, "TTD");

                var shouldCopy = true;

                if (Directory.Exists(appTtdDir))
                {
                    //If a process that we previously launched is still running, TTDRecordCPU.dll will be locked
                    //and we won't be able to replace the TTD folder. Check whether TTDRecordCPU.dll is locked, and
                    //if so skip overwriting local TTD copy
                    var ttdRecordCPUPath = Path.Combine(appTtdDir, "TTDRecordCPU.dll");

                    var hr = Kernel32.TryCreateFileW(ttdRecordCPUPath, FileAccess.ReadWrite, 0, FileMode.Open, out var hFile);

                    if (hr == HRESULT.S_OK)
                    {
                        //The file wasn't in use
                        Kernel32.CloseHandle(hFile);
                        Directory.Delete(appTtdDir, true);
                    }
                    else if (hr == HRESULT.ERROR_FILE_NOT_FOUND)
                    {
                        //Our local TTD directory exists, but it's empty. Proceed with deletion
                        Directory.Delete(appTtdDir, true);
                    }
                    else
                    {
                        //A process is still running using TTDRecordCPU.dll. Don't bother trying to replace our local TTD install
                        shouldCopy = false;
                    }

                    if (shouldCopy)
                    {
                        foreach (var remoteDir in Directory.GetDirectories(install.Folder, "*", SearchOption.AllDirectories))
                            Directory.CreateDirectory(remoteDir.Replace(install.Folder, appTtdDir));

                        foreach (var remoteFile in Directory.GetFiles(install.Folder, "*.*", SearchOption.AllDirectories))
                            File.Copy(remoteFile, remoteFile.Replace(install.Folder, appTtdDir), true);
                    }

                    install = install.Clone(appTtdDir);
                }
            }

            if (windbgInstall != null)
            {
                if (ttdInstall != null)
                {
                    install = windbgInstall.Version > ttdInstall.Version ? windbgInstall : ttdInstall;
                }
                else
                    install = windbgInstall;

                CopyInstallFiles();
            }
            else
            {
                if (ttdInstall != null)
                {
                    install = ttdInstall;
                    CopyInstallFiles();
                }
                else
                {
                    if (system32Install == null)
                        throw new FileNotFoundException($"Could not find TTD/TTTracer in either the TTD/WinDbg Microsoft Store Apps or in System32.");

                    install = system32Install;
                }
            }

            var hModule = Kernel32.LoadLibrary(install.TTDRecord);
            var fnPtr = Kernel32.GetProcAddress(hModule, "ExecuteTTTracerCommandLine");

            if (install.Type == TtdInstallType.System32)
            {
                var execute = Marshal.GetDelegateForFunctionPointer<TtdInstall.ExecuteTTTracerCommandLineDelegate_System32>(fnPtr);

                install.Execute = (sink, cmdLine, mode, eula) => execute(sink, cmdLine, mode);
            }
            else
            {
                install.Execute = Marshal.GetDelegateForFunctionPointer<TtdInstall.ExecuteTTTracerCommandLineDelegate>(fnPtr);
            }
        }

        //WinDbg's TtdRecordSessionInfo will execute a stop process upon cancellation, and then will
        //go through and delete any files that were created if the operation wasn't flagged as success

        /// <summary>
        /// Launches the specified process and waits for it to exit.
        /// </summary>
        /// <param name="processName">The name of the process to launch.</param>
        /// <param name="arguments">Any arguments that should be passed to the process.</param>
        /// <param name="outOfProcess">Whether to launch ttd.exe directly out of process.</param>
        /// <param name="launchAndAttach">Whether to create the process first and then attach.
        /// This provides a more reliable means of determining the process ID.</param>
        /// <param name="callback">The callback to execute for any log messages that are received from TTD.</param>
        public static void Launch(
            string processName,
            string arguments = null,
            bool outOfProcess = false,
            bool launchAndAttach = false,
            Action<TtdDiagnosticMessage> callback = null) =>
            LaunchAsync(processName, arguments, outOfProcess, launchAndAttach, callback);

        public static TtdTraceSession LaunchAsync(
            string processName,
            string arguments = null,
            bool outOfProcess = false,
            bool launchAndAttach = false,
            Action<TtdDiagnosticMessage> callback = null)
        {
            if (processName == null)
                throw new ArgumentNullException(nameof(processName));

            if (processName.Contains(" "))
                processName = $"\"{processName}\"";

            //Currently, to get the PID, we launch the process ourselves and then attach.
            //We could launch normally however and then extract the PID from the kernel.
            //See ttdrecord!TTD::GetProcessesBeingTracedInSession for how. Directories
            //starting with ttd_a contain the PID at the end of their name. You can see
            //these directories in PowerShell by doing ipmo ntobjectmanager; ls ntobjectsession:|where name -like *ttd*
            //Note however that ntobjectsession: points to ntobject:\sessions\1 but there's a 0 session folder as well

            if (launchAndAttach)
                return LaunchAndAttachAsync(processName, arguments, outOfProcess, callback);
            else
                return LaunchNormalAsync(processName, arguments, outOfProcess, callback);
        }

        private static TtdTraceSession LaunchAndAttachAsync(
            string processName,
            string arguments,
            bool outOfProcess,
            Action<TtdDiagnosticMessage> callback)
        {
            var si = new STARTUPINFOW
            {
                cb = Marshal.SizeOf<STARTUPINFOW>()
            };

            Kernel32.CreateProcessW(
                arguments == null ? processName : $"{processName} {arguments}",
                CreateProcessFlags.CREATE_NEW_CONSOLE | CreateProcessFlags.CREATE_SUSPENDED,
                IntPtr.Zero,
                null,
                ref si,
                out var pi
            );

            TtdTraceSession session;

            try
            {
                //Note: Attach also sets CurrentSession
                session = AttachAsync(pi.dwProcessId, outOfProcess, callback);

                Kernel32.ResumeThread(pi.hThread);
            }
            finally
            {
                Kernel32.CloseHandle(pi.hProcess);
                Kernel32.CloseHandle(pi.hThread);
            }

            return session;
        }

        private static TtdTraceSession LaunchNormalAsync(
            string processName,
            string arguments,
            bool outOfProcess,
            Action<TtdDiagnosticMessage> callback)
        {
            var eventName = Guid.NewGuid().ToString();
            using var wait = new EventWaitHandle(false, EventResetMode.ManualReset, eventName);

            var opts = new TtdOptions
            {
                Launch = arguments == null ? processName : $"{processName} {arguments}",
                OnInitCompleteEvent = eventName,
                NoUI = true,
            };

            if (install.Type != TtdInstallType.System32)
                opts.AcceptEula = true;

            DispatcherOperation op;

            var preIds = GetActiveTraceProcessIds();

            if (outOfProcess)
                op = TtdOutOfProcExecutor.Execute(opts, install, callback);
            else
                op = ExecuteAsync(opts, callback);

            WaitHandle.WaitAny(new[] { op.WaitHandle, wait });

            var postIds = GetActiveTraceProcessIds();

            var newIds = postIds.Except(preIds).ToArray();

            var session = new TtdTraceSession(newIds[0], op, callback);

            try
            {
                if (newIds.Length == 0)
                    throw new InvalidOperationException("Failed to to identify the PID of the newly launched process.");

                if (newIds.Length > 1)
                    throw new InvalidOperationException($"Failed to to identify the PID of the newly launched process: multiple TTD traces were started at once ({string.Join(", ", newIds)})");

                CurrentSession = session;
            }
            catch
            {
                session.Stop();

                throw;
            }

            return CurrentSession;
        }

        public static void Attach(int processId, Action<TtdDiagnosticMessage> callback = null) =>
            AttachAsync(processId, callback).Wait();

        public static TtdTraceSession AttachAsync(int processId, Action<TtdDiagnosticMessage> callback = null) =>
            AttachAsync(processId, false, callback);

        public static TtdTraceSession AttachAsync(Process process, Action<TtdDiagnosticMessage> callback = null) =>
            AttachAsync(process.Id, callback);

        public static TtdTraceSession AttachAsync(int processId, bool outOfProcess, Action<TtdDiagnosticMessage> callback = null)
        {
            bool selfTrace = processId == Process.GetCurrentProcess().Id;

            if (selfTrace)
            {
                if (callback != null)
                    throw new ArgumentException($"Cannot specify a {nameof(callback)} when tracing the current process.", nameof(callback));
            }

            var eventName = Guid.NewGuid().ToString();
            using var wait = new EventWaitHandle(false, EventResetMode.ManualReset, eventName);

            var opts = new TtdOptions
            {
                Attach = processId,
                OnInitCompleteEvent = eventName,
                NoUI = true,
            };

            if (install.Type != TtdInstallType.System32)
                opts.AcceptEula = true;

            DispatcherOperation op;

            if (outOfProcess)
            {
                if (selfTrace)
                {
#if NETFRAMEWORK
                    if (Debugger.IsAttached)
                        VsDebugger.Detach(Process.GetCurrentProcess());
#endif
                }

                op = TtdOutOfProcExecutor.Execute(opts, install, callback);
            }
            else
                op = ExecuteAsync(opts, callback);

            WaitHandle.WaitAny(new[] { op.WaitHandle, wait });

            CurrentSession = new TtdTraceSession(processId, op, callback);

            return CurrentSession;
        }

        public static string Help(TtdHelpMode helpMode)
        {
            string arguments;

            switch (helpMode)
            {
                case TtdHelpMode.Default: //-? with the right mode
                case TtdHelpMode.Unrestricted: //-? with the right mode
                case TtdHelpMode.Extended: //-?? with the right mode
                case TtdHelpMode.Hardware: //-hardware help
                case TtdHelpMode.Config: //-ConfigHelp
                default:
                    throw new NotImplementedException($"Getting help for mode {helpMode} is not implemented");
            }

            return ExecuteAndLog(arguments);
        }

        public static void Execute(TtdOptions options, Action<TtdDiagnosticMessage> callback = null) =>
            ExecuteAsync(options, callback).Wait();

        public static void Execute(string arguments, Action<TtdDiagnosticMessage> callback = null) =>
            ExecuteAsync(arguments, callback).Wait();

        public static DispatcherOperation ExecuteAsync(TtdOptions options, Action<TtdDiagnosticMessage> callback = null) =>
            ExecuteAsync(options.ToString(), callback);

        public static DispatcherOperation ExecuteAsync(string arguments, Action<TtdDiagnosticMessage> callback = null) =>
            TtdInProcExecutor.Execute(arguments, install, callback);

        private static string ExecuteAndLog(string arguments)
        {
            var result = new List<string>();

            Execute(arguments, m => result.Add(m.Message));

            return string.Join(Environment.NewLine, result);
        }

        internal static void TryOpenInWinDbg(string[] messages)
        {
            if (windbgInstall != null)
            {
                var windbgPath = Path.Combine(windbgInstall.Folder, "..", "..", "DbgX.Shell.exe");
                var runFile = TryGetRunFile(messages);

                if (File.Exists(windbgPath) && runFile != null)
                {
                    Process.Start(windbgPath, runFile);
                }
            }
        }

        private static string TryGetRunFile(string[] messages)
        {
            var runStrings = new[]
            {
                "Full trace dumped to \\b(.*)\\b",
                "Full trace written to \\b(.*)\\b",
                "Recording process .* on trace file: \\b(.*)\\b",
                "Corrupted trace file written to '\\b([^']*)\\b'."
            };

            var outStrings = new[]
            {
                "OUTPUT FILE '\\b([^']*)\\b' FOR MORE DETAILS",
                "Output file is \\b(.*)\\b",
                "\\(See \\b(.*)\\b",
                "Error text from \\b(.*)\\b (see file for full details):",
                "\\b(.*)\\b contains additional information about the recording session."
            };

            string runPath = null;
            string outPath = null;

            foreach (var message in messages)
            {
                if (runPath == null)
                {
                    foreach (var str in runStrings)
                    {
                        var match = new Regex(str, RegexOptions.IgnoreCase).Match(message);

                        if (match.Success)
                        {
                            runPath = match.Groups[1].Value;
                            break;
                        }
                    }
                }

                if (outPath == null)
                {
                    foreach (var str in outStrings)
                    {
                        var match = new Regex(str, RegexOptions.IgnoreCase).Match(message);

                        if (match.Success)
                        {
                            outPath = match.Groups[1].Value;
                            break;
                        }
                    }
                }

                if (runPath != null && outPath != null)
                    break;
            }

            return runPath;
        }

        public static int[] GetActiveTraceProcessIds()
        {
            /* ttdrecord!TTD::GetProcessIdsOfAllGuestProcesses drives enumerating all process IDs that are currently being traced.
             * First it calls TTD::GetAllSessionIds, which enumerates all processes running on the system and then does NtQueryInformationProcess(ProcessSessionInformation)
             * to get the session ID of each process. Then, for each of these processes it calls TTD::GetProcessesBeingTracedInSession which performs
             * the logic that you see below. Normally, trace sessions will run under \Sessions\<sessionId>\BaseNamedObjects, however I think when
             * a user does not have proper permissions, it can instead be configured to run in the global \BaseNamedObjects directory instead. Our TTD runner
             * is designed for debugging applications in the current user's session, so we don't worry looking at the global directory. */

            var sessionId = Kernel32.ProcessIdToSessionId(Kernel32.GetCurrentProcessId());

            var pids = Ntdll.EnumerateDirectories($"\\Sessions\\{sessionId}\\BaseNamedObjects")
                .Where(v => v.StartsWith("ttd_a_"))
                .Select(v =>
                {
                    var ch = v.LastIndexOf('_');

                    var str = v.Substring(ch + 1);

                    var num = Convert.ToInt32(str, 16);

                    return num;
                })
                .ToArray();

            return pids;
        }
    }
}