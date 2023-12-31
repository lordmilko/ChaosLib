﻿namespace ChaosLib
{
    interface IProcessService
    {
        string[] Execute(
            string fileName,
            ArgList arguments = default,
            string errorFormat = null,
            bool writeHost = false,
            bool shellExecute = false);

        bool IsRunning(string processName);
    }
}
