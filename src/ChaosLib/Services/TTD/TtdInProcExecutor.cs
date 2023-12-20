using System;
using System.Collections.Generic;
using System.Linq;
using ClrDebug;

namespace ChaosLib.TTD
{
    internal class TtdInProcExecutor
    {
        public static DispatcherOperation Execute(string arguments, TtdInstall install, Action<TtdDiagnosticMessage> callback = null)
        {
            var thread = new DispatcherThread("TTD");
            thread.Start();

            var op = thread.InvokeAsync(() =>
            {
                try
                {
                    var sink = new TtdDiagnosticsSink();

                    var messages = new List<TtdDiagnosticMessage>();

                    sink.OnReportMessage += (s, e) =>
                    {
                        if (IgnoreMessage(e))
                            return;

                        messages.Add(e);

                        callback?.Invoke(e);
                    };

                    //You must specify an exe name, otherwise certain things like -?? and -ConfigHelp won't work
                    install.Execute(sink, $"ttd {arguments}", TTD_TRACE_MODE.Extended, "eula!").ThrowOnNotOK();

                    var errors = messages.Where(v => v.Level == TTD_LOG_LEVEL.Error).ToArray();

                    if (errors.Length > 0)
                        throw new InvalidOperationException(string.Join(Environment.NewLine, errors.Select(v => v.Message)));

                    ttd.TryOpenInWinDbg(messages.Select(v => v.Message).ToArray());
                }
                finally
                {
                    ttd.CurrentSession = null;

                    thread.Dispatcher.BeginInvokeShutdown();
                }
            });

            return op;
        }

        private static bool IgnoreMessage(TtdDiagnosticMessage message)
        {
            if (message.Level == TTD_LOG_LEVEL.Info)
            {
                //Ignore startup banner
                if (message.Message.Contains("Microsoft (R) TTTracer"))
                    return true;

                //In Windows 10+, VerifyVersionInfo() requires that your EXE include an application manifest
                //to truly report as being "Windows 10 compatible". We don't have that, which means when TTD
                //checks whether we're Windows 10+ or not, VerifyVersionInfo() returns false, thus leading TTD
                //to conclude we're a "downlevel OS"
                if (message.Message.Contains("This recording session is using TTD on a downlevel OS"))
                    return true;
            }

            if (message.Level == TTD_LOG_LEVEL.Warning)
            {
                //Same VerifyVersionInfo() downlevel issue as above
                if (message.Message.Contains("Failed to find IDNA for this recording session"))
                    return true;
            }

            if (message.Level == TTD_LOG_LEVEL.Info && message.Message.Contains("Microsoft (R) TTTracer"))
                return true;

            return false;
        }
    }
}