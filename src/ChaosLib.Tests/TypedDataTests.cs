using System;
using System.Diagnostics;
using System.Linq;
using ChaosLib.TypedData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChaosLib.Tests
{
    [TestClass]
    public class TypedDataTests
    {
        [TestMethod]
        public void TypedData_ListEntry()
        {
            Test(peb =>
            {
                var moduleList = (DbgRemoteListEntryHead) peb["Ldr"]["InLoadOrderModuleList"];

                var list = moduleList.ToList("ntdll!_LDR_DATA_TABLE_ENTRY", "InMemoryOrderLinks");

                var ntdll = list.Select(v => v["BaseDllName"].ToString()).Single(v => v.Contains("ntdll"));

                Assert.AreEqual("C:\\Windows\\system32\\ntdll.dll", ntdll, true);
            });
        }

        private void Test(Action<DbgRemoteObject> validate)
        {
            Kernel32.SetDllDirectory(DbgEngResolver.GetDbgEngPath());

            var process = Process.GetCurrentProcess();

            var dbgHelpSession = new DbgHelpSession(process.Handle, invadeProcess: false);
            var ntdll = process.Modules.Cast<ProcessModule>().Single(m => m.ModuleName == "ntdll.dll");

            dbgHelpSession.AddModule(ntdll.ModuleName, (long) ntdll.BaseAddress);

            var peb = Ntdll.NtQueryInformationProcess<PROCESS_BASIC_INFORMATION>(process.Handle, PROCESSINFOCLASS.ProcessBasicInformation).PebBaseAddress;

            var provider = new TypedDataProvider(dbgHelpSession);

            var remotePeb = provider.CreateObject(peb, "ntdll!_PEB");

            validate(remotePeb);
        }
    }
}
