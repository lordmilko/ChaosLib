using System;
using System.Runtime.InteropServices;
using ClrDebug;
using static ClrDebug.HRESULT;

namespace ChaosLib
{
    public class MemoryReader
    {
        private IntPtr hProcess;

        public MemoryReader(IntPtr hProcess)
        {
            this.hProcess = hProcess;
        }

        public unsafe HRESULT ReadVirtual(long address, IntPtr buffer, int bytesRequested, out int bytesRead)
        {
            //ReadProcessMemory will fail if any part of the region to read does not have read access, which can commonly occur
            //when attempting to read across a page boundary. As such, memory requests must be "chunked" to be within each page,
            //which we do by gradually shifting the pointer of our buffer along until we've read everything we're after

            var pageSize = 0x1000;

            var totalRead = 0;

            HRESULT hr = S_OK;

            //For some reason, when trying to read the optHeaderMagic in GetMachineAndResourceSectionRVA() when trying to establish an SOS
            //against PowerShell 7, when using the buffer provided by mscordaccore, ReadProcessMemory would say it read 2 bytes, but no memory would actually change.
            //Manually copying into mscordaccore's buffer after we safely read into our own buffer appears to resolve this
            var innerBuffer = Marshal.AllocHGlobal(pageSize);

            try
            {
                while (bytesRequested > 0)
                {
                    //This bit of magic ensures we're not reading more than 1 page worth of data. I don't understand how this works
                    //however Microsoft use it all the time so you know it's right
                    var readSize = pageSize - (int)(address & (pageSize - 1));
                    readSize = Math.Min(bytesRequested, readSize);

                    var result = Kernel32.ReadProcessMemory(
                        hProcess,
                        (IntPtr) (void*) address,
                        innerBuffer,
                        readSize,
                        out bytesRead
                    );

                    Buffer.MemoryCopy(innerBuffer.ToPointer(), buffer.ToPointer(), bytesRead, bytesRead);

                    if (!result)
                    {
                        //Some methodtables' parents appear to point to an invalid memory address. When we read these invalid memory addresses,
                        //pass them back to the DAC and then are asked to read some actual data from these invalid locations, this will naturally fail with ERROR_PARTIAL_COPY,
                        //but really its a total failure
                        if (totalRead > 0)
                            hr = S_OK;
                        else
                            hr = (HRESULT)Marshal.GetHRForLastWin32Error();

                        break;
                    }

                    totalRead += bytesRead;
                    address += bytesRead;
                    buffer = new IntPtr(buffer.ToInt64() + bytesRead);
                    bytesRequested -= bytesRead;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(innerBuffer);
            }

            if (hr == S_OK)
                bytesRead = totalRead;
            else
                bytesRead = 0;

            return hr;
        }
    }
}
