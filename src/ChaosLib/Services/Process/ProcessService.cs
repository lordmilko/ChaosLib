using System;
using System.Diagnostics;
using System.Linq;

namespace ChaosLib
{
    class ProcessService : IProcessService
    {
        private IConsole console;

        public ProcessService(IConsole console)
        {
            this.console = console;
        }

        public string[] Execute(
            string fileName,
            ArgList arguments = default,
            string errorFormat = null,
            bool writeHost = false,
            bool shellExecute = false)
        {
            var writer = new ProcessOutputWriter(writeHost, console);
            var executor = new ProcessExecutor(
                fileName,
                arguments,
                errorFormat,
                writeHost,
                shellExecute,
                writer
            );

            executor.Execute();

            return writer.Output;
        }

        public bool IsRunning(string processName)
        {
            var processes = Process.GetProcesses();

            return processes.Any(p => p.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
