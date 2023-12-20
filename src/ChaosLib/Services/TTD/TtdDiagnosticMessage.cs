using System.Diagnostics;
using ClrDebug;

namespace ChaosLib.TTD
{
    [DebuggerDisplay("[{Level.ToString(),nq}] {Message}")]
    public class TtdDiagnosticMessage
    {
        public TTD_LOG_LEVEL Level { get; }

        public HRESULT ErrorHR { get; }

        public string Message { get; }

        public TtdDiagnosticMessage(TTD_LOG_LEVEL level, HRESULT errorHR, string message)
        {
            Level = level;
            ErrorHR = errorHR;
            Message = message;
        }

        public override string ToString()
        {
            return Message;
        }
    }
}