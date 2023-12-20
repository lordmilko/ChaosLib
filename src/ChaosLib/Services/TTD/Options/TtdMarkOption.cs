namespace ChaosLib.TTD
{
    public class TtdMarkOption
    {
        public int ProcessId { get; }

        public string Value { get; }

        public TtdMarkOption(int processId, string value)
        {
            ProcessId = processId;
            Value = value;
        }

        public override string ToString()
        {
            return $"\"{Value}\" {ProcessId}";
        }
    }
}