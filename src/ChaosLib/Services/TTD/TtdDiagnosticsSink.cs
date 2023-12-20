using System;
using ClrDebug;

namespace ChaosLib.TTD
{
    public class TtdDiagnosticsSink : ITtdDiagnosticsSink
    {
        public event EventHandler<TtdDiagnosticMessage> OnReportMessage;

        public HRESULT GetMaximumLevelOfInterest(out TTD_LOG_LEVEL level)
        {
            //ttd.exe hardcodes to use log level 3 in wmain. While I have seen evidence
            //that there are messages that can be passed to ReportMessage with a log level of 4
            //that look like "Debug" messages, setting the log level higher didn't yield any additional messages
            level = TTD_LOG_LEVEL.Info;
            return HRESULT.S_OK;
        }

        public HRESULT ReportMessage(TTD_LOG_LEVEL level, HRESULT errorHR, string message)
        {
            OnReportMessage?.Invoke(this, new TtdDiagnosticMessage(level, errorHR, message));

            return HRESULT.S_OK;
        }
    }
}