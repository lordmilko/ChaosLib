using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using ClrDebug;

namespace ChaosLib
{
#if NETFRAMEWORK
    //Casting the EnvDTE.Debugger to EnvDTE80.Debugger2 provides access to the Transports member.
    //The Default transport contains a list of possible Engines that can be specified to Attach2
    public enum VsDebuggerType
    {
        Native,
        Managed,

        [Description("Managed (.NET Core, .NET 5+)")]
        Core
    }

    /// <summary>
    /// Provides facilities for attaching Visual Studio to a child process launched by the current process.
    /// </summary>
    public class VsDebugger
    {
        /// <summary>
        /// Attaches the instance of Visual Studio that is debugging the current process to another process as well.<para/>
        /// Typically this is used to attach Visual Studio to a child process that is additionally launched by the current process.
        /// </summary>
        /// <param name="target">The child process to attach to.</param>
        /// <param name="type">The type of Visual Studio debugging engine to use to perform the attach.</param>
        public static void Attach(Process target, VsDebuggerType type) =>
            Invoke(target, p => p.Attach2(type.GetDescription()));

        //Note: you cant detach if interop debugging
        public static void Detach(Process target) =>
            Invoke(target, p => p.Detach(false));

        private static void Invoke(Process target, Action<EnvDTE80.Process2> action)
        {
            while (true)
            {
                try
                {
                    using (new MessageFilter())
                    {
                        if (Debugger.IsAttached)
                        {
                            var debuggerDte = GetDTEDebuggingMe();

                            foreach (var process in debuggerDte?.Debugger.LocalProcesses.OfType<EnvDTE80.Process2>())
                            {
                                if (CheckProcessId(target, process))
                                {
                                    try
                                    {
                                        action(process);
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception("Failed to attach debugger; make sure mixed mode debugging is disabled", ex);
                                    }

                                    break;
                                }
                            }
                        }
                    }

                    break;
                }
                catch (COMException ex)
                {
                    if (((HRESULT)ex.HResult) != HRESULT.RPC_E_SERVERCALL_RETRYLATER)
                        throw;
                }
            }
        }

        private static object GetEngine(EnvDTE.Debugger debugger, string name)
        {
            //The .NET Core engine name can't be specified directly to Attach 2, so get the Engine object instead

            EnvDTE80.Debugger2 debugger2 = (EnvDTE80.Debugger2) debugger;

            var transport = debugger2.Transports.Item("default");

            foreach (EnvDTE80.Engine engine in transport.Engines)
            {
                if (engine.Name == name)
                    return engine;
            }

            //Couldn't find the engine; try using the name directly
            return name;
        }

        private static EnvDTE.DTE GetDTEDebuggingMe()
        {
            var currentProcessId = Process.GetCurrentProcess().Id;

            foreach (var process in Process.GetProcessesByName("devenv"))
            {
                var dte = GetDTE(process, false);

                if (dte?.Debugger?.DebuggedProcesses?.OfType<EnvDTE.Process>().Any(p => p.ProcessID == currentProcessId) ?? false)
                {
                    return dte;
                }
            }

            return null;
        }

        private static EnvDTE.DTE GetDTE(Process process, bool ensure)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            do
            {
                var dte = GetDTEInternal(process);

                if (dte != null)
                    return dte;

                if (stopwatch.Elapsed.TotalSeconds > 60)
                    throw new TimeoutException($"Failed to get DTE from process '{process.ProcessName}.exe' (ID: {process.Id}). Confirm whether the target is a Visual Studio process.");
            } while (ensure);

            //If ensure was false, and we didn't get a DTE, our DTE was null
            return null;
        }

        private static EnvDTE.DTE GetDTEInternal(Process process)
        {
            object dte = null;
            var monikers = new IMoniker[1];

            IRunningObjectTable runningObjectTable;
            Ole32.Native.GetRunningObjectTable(0, out runningObjectTable);

            IEnumMoniker enumMoniker;
            runningObjectTable.EnumRunning(out enumMoniker);

            IBindCtx bindContext;
            Ole32.Native.CreateBindCtx(0, out bindContext);

            do
            {
                monikers[0] = null;

                IntPtr monikersFetched = IntPtr.Zero;
                var hresult = enumMoniker.Next(1, monikers, monikersFetched);

                if (hresult == Kernel32.S_FALSE)
                {
                    // There's nothing further to enumerate, so fail
                    return null;
                }
                else
                {
                    Marshal.ThrowExceptionForHR(hresult);
                }

                var moniker = monikers[0];

                string fullDisplayName;
                moniker.GetDisplayName(bindContext, null, out fullDisplayName);

                // FullDisplayName will look something like: <ProgID>:<ProcessId>
                var displayNameParts = fullDisplayName.Split(':');

                int displayNameProcessId;
                if (!int.TryParse(displayNameParts.Last(), out displayNameProcessId))
                    continue;

                if (displayNameParts[0].StartsWith("!VisualStudio.DTE", StringComparison.OrdinalIgnoreCase) &&
                    displayNameProcessId == process.Id)
                {
                    //If the specified instance of Visual Studio is already being debugged (i.e. by someone else) we will hang indefinitely
                    //trying to get the DTE; time out instead if we don't hear back within 1 second

                    var cts = new CancellationTokenSource();

                    var task = Task.Run(() =>
                    {
                        runningObjectTable.GetObject(moniker, out dte);
                    }, cts.Token);

                    var isCompleted = task.Wait(TimeSpan.FromSeconds(1));

                    if (!isCompleted)
                        cts.Cancel();
                }
            }
            while (dte == null);

            return (EnvDTE.DTE)dte;
        }

        private static bool CheckProcessId(Process target, EnvDTE80.Process2 process)
        {
            //Doing process.ProcessID doesn't seem to be utilizing our MessageFilter, so we do our own manual retry

            while (true)
            {
                try
                {
                    //If RPC_E_SERVERCALL_RETRYLATER is thrown here, disable breaking on this exception;
                    //we will try again thanks to the message filter
                    if (process.ProcessID == target.Id)
                        return true;

                    return false;
                }
                catch
                {
                    Thread.Sleep(1);
                }
            }
        }
    }
#endif
}
