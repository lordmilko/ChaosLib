using System;

namespace ChaosLib
{
    interface IConsole
    {
        void WriteLine(string message, ConsoleColor? color = null);
    }

    class PhysicalConsole : IConsole
    {
        public void WriteLine(string message, ConsoleColor? color = null)
        {
            if (color == null)
                Console.Write(message);
            else
            {
                var original = Console.ForegroundColor;

                try
                {
                    Console.ForegroundColor = color.Value;

                    Console.WriteLine(message);
                }
                finally
                {
                    Console.ForegroundColor = original;
                }
            }
        }
    }

    class EventConsole : IConsole
    {
        private Action<string> action;

        public EventConsole(Action<string> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            this.action = action;
        }

        public void WriteLine(string message, ConsoleColor? color = null)
        {
            action.Invoke(message);
        }
    }
}