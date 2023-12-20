using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ClrDebug;

namespace ChaosLib.TTD
{
    internal static class TtdOutOfProcExecutor
    {
        public static DispatcherOperation Execute(TtdOptions options, TtdInstall install, Action<TtdDiagnosticMessage> callback = null)
        {
            var thread = new DispatcherThread("TTD");
            thread.Start();

            var op = thread.InvokeAsync(() =>
            {
                try
                {
                    var messages = new List<TtdDiagnosticMessage>();

                    var service = new ProcessService(new EventConsole(message =>
                    {
                        var level = TTD_LOG_LEVEL.Info;

                        if (Regex.IsMatch(message, "Error: .*"))
                            level = TTD_LOG_LEVEL.Error;

                        var diagMsg = new TtdDiagnosticMessage(level, HRESULT.S_OK, message);

                        messages.Add(diagMsg);

                        callback?.Invoke(diagMsg);
                    }));

                    service.Execute(install.TTTracer, options.ToString(), writeHost: true);

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
    }
}
