using System.Runtime.InteropServices;
using ClrDebug;

namespace ChaosLib.TTD
{
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("6E7D7B43-642B-4898-9E58-C5A205F862A7")]
    public interface ITtdDiagnosticsSink
    {
        [PreserveSig]
        HRESULT GetMaximumLevelOfInterest(
            [Out] out TTD_LOG_LEVEL level);

        [PreserveSig]
        HRESULT ReportMessage(
            [In] TTD_LOG_LEVEL level,
            [In] HRESULT errorHR,
            [In, MarshalAs(UnmanagedType.LPWStr)] string message);
    }
}