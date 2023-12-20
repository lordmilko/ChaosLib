using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ChaosLib.TTD
{
    /* WinDbgX contains secret functionality. If you modify %localappdata%\DBG\DbgX.xml,
     * you can enable secret TTD options "ShowCheckers" and "EnableSelectiveRecording".
     * These options add additional options to the Time Travel tab, although I haven't
     * been able to figure out why the checkers functionality doesn't show. By creating a
     * fake DbgX.Microsoft .NET 6 WPF DLL with a resource dictionary that includes a Resource
     * /Resources/Ribbon/update_16.png and place this next to DbgX.Shell, you will be able to
     * create a new *.ttdconfig file which can be used for performing "selective recording".
     *
     * Unfortunately, even with your *.ttdconfig file in hand, the only IDbgTtdRecordOption
     * included in DbgXUI is the OutputDirectoryRecordOption that you normally see. Nevertheless,
     * this tells us all we need to know in order to analyze TtdRecordOptions in DbgX.Interfaces.Internal.
     * All the fun stuff happens in DbgXUI's TtdRecordingService and TtdRecordSessionInfo
     *
     * Arguments
     * -scenario
     * -accepteula
     * -out
     * -selectiveRecording
     * -stop <processName> / <pid> / all
     *
     * There are several "scenarios" that TTD supports:
     * - VisualStudioSnapshot
     * TtdCmdLine
     * InboxCmdLine
     * TtdExternalCmdLine
     * WindbgNext
     * WindowsTestLab
     * Unknown
     * VisualStudioTests
     * Diagtrack
     * TtdWatson
     * TtdTestLab
     *
     * The scenario name cannot be longer than 32 characters.*/

    public class TtdOptions
    {
        private Dictionary<string, object> options = new Dictionary<string, object>();

        private T GetValue<T>([CallerMemberName] string propertyName = null)
        {
            if (options.TryGetValue(propertyName, out var value))
                return (T) value;

            return default; 
        }

        private void SetValue<T>(T value, [CallerMemberName] string propertyName = null) =>
            options[propertyName] = value;

        #region Options

        //Default (Standalone)

        /// <summary>
        /// Display this help.
        /// </summary>
        [Argument("-?")]
        public bool Help
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        /// <summary>
        /// Accept one-time EULA without prompting.
        /// </summary>
        [Argument("-acceptEula")]
        public bool AcceptEula
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        /// <summary>
        /// Trace through family of child processes.
        /// </summary>
        [Argument(" -children")]
        public bool Children
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        /// <summary>
        /// Uninstall process monitor driver
        /// </summary>
        [Argument("-cleanup")]
        public bool Cleanup
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        /// <summary>
        /// Must be combined with -monitor and it will only record the target if its command line contains the string.
        /// This is useful for situations when the command line argument uniquely identifies the process you are interested in,
        /// e.g., notepad.exe specialfile.txt only the instance of notepad.exe with that file name will be recorded.
        /// </summary>
        [Option("-cmdLineFilter")]
        public string CmdLineFilter
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        /// <summary>
        /// Maximum number of recordings that can be ongoing at any one point in time.
        /// </summary>
        [Option("-maxConcurrentRecordings")]
        public int MaxConcurrentRecordings
        {
            get => GetValue<int>();
            set => SetValue(value);
        }

        /// <summary>
        /// Maximum size of the trace file in MB. When in full trace mode the default is 1024GB and the minimum value is 1MB.
        /// When in ring buffer mode the default is 2048MB, the minimum value is 1MB, and the maximum value is 32768MB.
        /// The default for in-memory ring on 32-bit processes is 256MB.
        /// </summary>
        [Option("-maxFile")]
        public int MaxFile
        {
            get => GetValue<int>();
            set => SetValue(value);
        }

        /// <summary>
        /// Disables the UI for manual control of recording.
        /// </summary>
        [Argument("-noUI")]
        public bool NoUI
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        /// <summary>
        /// Specifies a number of Virtual CPUs to be reserved and used when tracing. This value affects the total memory overhead
        /// placed on the guest process' memory by TTD. If not specified then default per platform is used: 55 for x64/ARM64
        /// and 32 for x86. Change this setting in order to limit the memory impact ONLY if you are running out of memory.
        /// The .out file will give hints this effect.
        ///
        /// Note: Changing this value to a lower number can severely impact the performance of tracing and should only be done to
        /// work around memory impact issues.
        /// </summary>
        [Option("-numVCpu")]
        public int NumVCPU
        {
            get => GetValue<int>();
            set => SetValue(value);
        }

        /// <summary>
        /// Allows an event to be signaled when tracing initialization is complete.
        /// </summary>
        [Option("-onInitCompleteEvent")]
        public string OnInitCompleteEvent
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        /// <summary>
        /// Specify a trace file name or a directory. If a directory, the directory must already exist. If a file name, the directory
        /// of the file name must already exist. If a file name, and the file name already exists, it will be overwritten without prompting. By
        /// default the executable's base name with a version number is used to prefix the trace file name.
        /// </summary>
        [Option("-out")]
        public string Out
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        /// <summary>
        /// Pass the guest process exit value through as ttd's exit value.
        /// </summary>
        [Argument("-passThroughExit")]
        public bool PassThroughExit
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        /// <summary>
        /// Specifies what support is expected from the CPUs that will be used to replay the trace.
        /// </summary>
        [Option("-replayCpuSupport")]
        public TTdReplayCpuSupportMode ReplayCpuSupport
        {
            get => GetValue<TTdReplayCpuSupportMode>();
            set => SetValue(value);
        }

        /// <summary>
        /// Trace to a ring buffer. The file size will not grow beyond the limits specified by -maxFile.Only the latter portion of the
        /// recording that fits within the given size will be preserved.
        /// </summary>
        [Argument("-ring")]
        public bool Ring
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        /// <summary>
        ///  When choosing a name for the .run file use current time as part of the file name, instead of an increasing counter.
        /// When repeatedly recording the same executable, use this switch to reduce time to start tracing.
        /// </summary>
        [Argument("-timestampFileName")]
        public bool TimestampFileName
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        /// <summary>
        /// Starts application with trace recording off.  You can use the UI to turn tracing on.
        /// </summary>
        [Argument("-tracingOff")]
        public bool TracingOff
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        //Unrestricted

        /// <summary>
        /// Attach to a process in non-interactive mode and return control to the console after it starts tracing.
        /// </summary>
        [Argument("-bg")]
        public bool Bg
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        /// <summary>
        /// Stop future tracing of a program previously specified with -onLaunch. Does not stop current tracing. For -plm apps you can only specify the
        /// package (-delete &lt;package&gt;) and all apps within that package will be removed from future tracing.
        /// </summary>
        [Argument("-delete")]
        public bool Delete
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        /// <summary>
        /// Disable CET shadow stacks before launching the target process. Recording of processes that have shadow stacks enabled is currently
        /// not supported, so this allows TTD to record processes that would otherwise have shadow stacks enabled by the OS.
        /// Note that this can only be done during process creation, so it cannot be used when attaching to processes that TTD didn't create.
        /// </summary>
        [Argument("-disableCETSS")]
        public bool DisableCETSS
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        /// <summary>
        /// Specify how recording DLLs are injected into the target process. If not specified, <see cref="TtdInjectMode.LoaderForCombinedRecording"/> is
        /// assumed by default.
        /// </summary>
        [Option("-injectMode")]
        public TtdInjectMode InjectMode
        {
            get => GetValue<TtdInjectMode>();
            set => SetValue(value);
        }

        /// <summary>
        /// Enables managed recording (default is full tracing). However, if paired with -selectiveRecording then the corresponding ttdconfig
        /// passed in must be of managed selective recording format. (As used in the Standalone Collector scenario).
        /// </summary>
        [Argument("-managed")]
        public bool Managed
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        /// <summary>
        /// Use with -onLaunch to specify a currently running parent process when the traced process will run with low privileges. Prefer to use -monitor &lt;program&gt;
        /// instead of -parent/-onlaunch which may be deprecated in the future.
        /// </summary>
        [Option("-parent")]
        public string Parent
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        /// <summary>
        /// Do not display any standard output.
        /// </summary>
        [Argument("-quiet")]
        public bool Quiet
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        /// <summary>
        /// Specify how to record a ring trace.
        /// </summary>
        [Option("-ringMode")]
        public TtdRingMode RingMode
        {
            get => GetValue<TtdRingMode>();
            set => SetValue(value);
        }

        /// <summary>
        /// Enables selective recording using the configuration file located at the specified path.The configuration file must
        /// be readable by the process being traced. (Managed selective recording format required when paired with -managed flag).
        /// </summary>
        [Option("-selectiveRecording")]
        public string SelectiveRecording
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        /// <summary>
        /// (Only with -timer) Forces the application to terminate with the specified exit code after the timer runs out.
        /// </summary>
        [Option("-tExit")]
        public int tExit
        {
            get => GetValue<int>();
            set => SetValue(value);
        }

        /// <summary>
        /// Stops recording after the specified amount of time (in seconds).
        /// </summary>
        [Option("-timer")]
        public int Timer
        {
            get => GetValue<int>();
            set => SetValue(value);
        }

        #endregion
        #region Modes

        //Default (Standalone)

        /// <summary>
        /// Attach to a running process specified by process ID.
        /// </summary>
        [Option("-attach")]
        public int Attach
        {
            get => GetValue<int>();
            set => SetValue(value);
        }

        /// <summary>
        /// Launch and trace the program (default). This is the only mode that allows arguments to be passed to the program.
        ///
        /// Note: This must be the last option in the command-line, followed by the program + &lt;arguments&gt;
        /// </summary>
        [Option("-launch")]
        public string Launch
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        /// <summary>
        /// -monitor &lt;Program&gt;<para/>
        ///
        /// Trace programs or services each time they are started (until reboot or Ctrl+C pressed). You must specify a full
        /// path to the output location with -out. More than one -monitor switch may be specified.
        /// </summary>
        [Option("-monitor")]
        public string Monitor
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        //Unrestricted

        /// <summary>
        /// Trace programs each time they are started (until reboot or Ctrl+C pressed). For -plm apps you can
        /// only specify the package(-onLaunch &lt;package&gt;) and all apps within that package will be set for TTD tracing on their next
        /// launch. There is no ability to specify only 1 app. You must specify a full path to the output location with -out. More
        /// than one -onLaunch may be specified.
        /// </summary>
        [Argument("-onLaunch")]
        public string OnLaunch
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        /// <summary>
        /// Trace program or service each time it starts (forever). You must specify a full path to the output location with
        /// -out. You may specify -persistent more than once. Use -cleanup to stop monitoring the program / service.
        /// </summary>
        [Argument("-persistent")]
        public bool Persistent
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        /// <summary>
        /// To specify a PLM app/package for tracing from launch or to launch that app. These PLM apps can only be setup for tracing if specifying the plm option.
        /// See -launch, -onlaunch, and -delete for the parameters required for each case. The default name for a single app package is 'app' and must be included.
        /// You must specify a full path to the output location with -out.
        /// </summary>
        [Argument("-plm")]
        public bool Plm
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        #endregion
        #region Control

        //Default (Standalone)

        /// <summary>
        /// Stop tracing the process specified (name, PID or "all").
        /// </summary>
        [Option("-stop")]
        public string Stop
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        /// <summary>
        /// Wait for up to the amount of seconds specified for all trace sessions on the system to end. Specify -1 to wait infinitely.
        /// </summary>
        [Option("-wait")]
        public int Wait
        {
            get => GetValue<int>();
            set => SetValue(value);
        }

        //Unrestricted

        /// <summary>
        /// -mark "&lt;string&gt;" &lt;PID&gt;<para/>
        ///
        /// Signal a guest process to insert the string into its trace file.  The string must be less than 256 characters.<para/>
        /// </summary>
        [Option("-mark")]
        public TtdMarkOption Mark
        {
            get => GetValue<TtdMarkOption>();
            set => SetValue(value);
        }

        /// <summary>
        /// -terminate &lt;code&gt; &lt;PID&gt;<para/>
        ///
        /// Terminate with the specified exit code a process specified by process ID.
        /// </summary>
        [Option("-terminate")]
        public TtdTerminateOption Terminate
        {
            get => GetValue<TtdTerminateOption>();
            set => SetValue(value);
        }

        #endregion
        #region Restricted Options

        /*
 -attemptRecordWithCETSS Record even if CET shadow stacks are enabled in the target process.
                   Recording of processes that have shadow stacks enabled is currently
                   not supported, so TTD will fail with an error message.
                   This option allows the user to override this behavior, at their own risk.
 -autoStart        If tracing stops in a guest process, automatically restart
                   it if the guest is still active.  Use -stop all to cancel
                   auto mode.
 -console <file>   Re-direct console output to the specified file or directory.
                   <file> format is similar to format of -out.
 -context <name>   Launches the guest process with the security context
                   of the passed in process name.  The process must
                   be in the same session as this client.  This
                   is not supported on OneCore.
 -dumpFull         In addition to dumping every loaded module image into the
                   trace file, takes a snapshot of the guest process on attach.
                   This option is enabled by default for non-ring traces.  This
                   option is incompatible with -ring.
 -dumpModules      Dumps a copy of every loaded module image into the trace
                   file.  This option may significantly increase the size of
                   the trace file.  This option is enabled by default for ring
                   traces.
 -initialize       Manually initialize your system for tracing.  
                   You can trace without administrator privileges
                   after the system is initialized. Not supported by inbox TTTracer.exe
 -loadOnly <PID>   Loads TTT into the process, but does not start a trace
                   session.  This is useful if the client starting the trace
                   session is not running with high enough privileges to load
                   TTT into the process.
 -maxInstructionsPerFragment <num> Specifies how many instructions can be emulated
                   contiguously before artificially breaking into a new fragment.
                   Default value is 10000.
 -maxInstructionsPerSegment <num> Specifies how many instructions will be emulated
                   per segment, which determines the parallelism granularity used
                   during replay. Default value is 3000000.
 -ni               Attach to a process in non-interactive mode.  The tracer
                   cannot attach to a waiting/sleeping process, so -ni prevents
                   timing out while waiting for the guest.
 -noAutoDumpFull   Turns off the automatic application of -dumpFull to managed
                   processes, useful when there's no intent to do managed debugging.
                   Implies -noDumpFull. -noDumpFull       Explicitly turns off both -dumpFull and -dumpModules behavior.
 -noRing           Take a full trace of the guest process (default).
 -noSkipContiguousAtomics Do not ignore atomic instructions whose sequence IDs are contiguous
 -noUI             Disables the UI for manual control of recording.
                   (default on OS with no UI)
 -recordMemoryProtect   Records a custom event with the initial memory protection
                   state of the process.
 -recordProcessorSwitches  Records a thread-local custom event whenever
                   each thread switches processors.
 -saveCrash <file> If the guest process hits an unhandled exception,
                   exit the process and save the trace file to
                   <file>.%.crash.  Do not combine with -out.
 -skipContiguousAtomics Ignore atomic instructions whose sequence IDs are contiguous
                   with the previous in the same thread. This makes it harder to
                   determine causality but it optimizes recording performance and trace
                   file size. This is the default.
 -threadsRunNativelyByDefault  Threads don't start recording automatically,
                   instead thread recording must be initiated via API call.
         */

        #endregion
        #region Unsupported Options

        [Argument("-??")]
        public bool UnsupportedHelp
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        [Argument("-ConfigHelp")]
        public bool ConfigHelp
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        /*
 -??               Display this help.
 -boot             Used internally to mark a service program tracing from boot (may be
                   specified more than once.
 -bootSvc          Used internally to mark a svchost.exe service tracing from boot (may be
                   specified more than once.
 -deleteTraceFilesOnExit Ensures that any trace and output files created by
                   this instance of TTTracer will be deleted after the guest
                   processes exit (or, for any processes still running when
                   the computer shuts down, on the next reboot).
 -downlevel        Force to trace via downlevel mode. It must be placed as first option.
                   Rest of the options will be passed to downlevel tracer.
 -dumpMinimalModuleData Explicitly turns off both -dumpFull and -dumpModules behavior,
                   and further reduces the data captured for each module
                   to the debugger's bare necessities.
 -enableBreak      Allow breaking process
 -errCheck <0|1|2|3> Enable or disable placing extra error debug checking
                   information in the log.
                   The current setting is 0 (Off).
 -getTracePID <file> Get process ID from target trace file(*.run).
 -hardware key1=value1,key2=value2,... Control hardware tracing. To get more information, use '-hardware help'.
 -inheritHandles   Specifies that the process to be recorded must inherit handles.
                   This is useful when the process initiating the recording
                   wishes to pass a handle to the process being recorded.
 -noAttachViaThread Specifies to not use an injected thread to initialize the recorder.
                    The recorder will be initialized using the old method,
                    from the process instrumentation callback instead.
 -noConfigConnect  Does not connect to running processes in service mode
 -noExit           Do not exit after trace complete
 -noFlush          Don't flush the trace file before exiting.
                   This may allows TTTracer to exit much quicker after recording ends.
 -nogroup          Does not collect extra information in the trace to allow
                   for inter-process analysis on traces taken simultaneously.
 -noReconfigureProcMon Instructs TTTracer to fail if a reconfiguration of the process monitor server would be performed otherwise.
 -noRedirect       Force to trace via the TTD tracer only (no downlevel). It must be placed as first option.
 -scenario <name>  Use this flag to mark your usage of this tool with a
                   scenario name. We collect this information and use it to
                   identify key scenarios where the tool is used.
         */

        #endregion

        public override string ToString()
        {
            var args = new List<string>();

            var properties = GetType().GetProperties().ToDictionary(p => p.Name, p =>
            {
                var arg = p.GetCustomAttribute<ArgumentAttribute>();

                if (arg != null)
                    return (Attribute) arg;

                var opt = p.GetCustomAttribute<OptionAttribute>();

                if (opt != null)
                    return opt;

                throw new InvalidOperationException($"Property {p.Name} did not have a {nameof(ArgumentAttribute)} or {nameof(OptionAttribute)}");
            });

            string launch = null;

            foreach (var item in options)
            {
                var match = properties[item.Key];

                if (match is ArgumentAttribute a)
                    args.Add(a.Name);
                else
                {
                    var opt = (OptionAttribute) match;

                    //Must be last
                    if (item.Key == nameof(Launch))
                        launch = $"{opt.Name} {item.Value}";
                    else
                        args.Add($"{opt.Name} {item.Value}");
                }
            }

            if (launch != null)
                args.Add(launch);

            return string.Join(" ", args);
        }
    }
}