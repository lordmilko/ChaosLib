using System;
using System.Collections.Generic;

namespace ChaosLib
{
    class ProcessOutputWriter
    {
        private bool writeHost;
        private IConsole console;

        private List<string> output = new List<string>();

        public string[] Output => output.ToArray();

        public ProcessOutputWriter(bool writeHost, IConsole console)
        {
            this.writeHost = writeHost;
            this.console = console;
        }

        public void Write(ProcessOutputObject record)
        {
            if (writeHost)
            {
                if (record.Data is Exception e)
                    console.WriteLine(e.Message, ConsoleColor.Red);
                else
                    console.WriteLine(record.Data.ToString());
            }
            else
            {
                if (record.Data is Exception e)
                    output.Add(e.Message);
                else
                    output.Add(record.Data.ToString());
            }
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, output);
        }
    }
}
