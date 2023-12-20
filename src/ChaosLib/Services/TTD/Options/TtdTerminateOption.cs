namespace ChaosLib.TTD
{
    public class TtdTerminateOption
    {
        public int ProcessId { get; }

        public int Code { get; }

        public TtdTerminateOption(int processId, int code)
        {
            ProcessId = processId;
            Code = code;
        }
    }
}