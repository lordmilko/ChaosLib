using System;
using System.Collections.Generic;
using System.Linq;

namespace ChaosLib.Handle
{
    public class FileHandleInfo : HandleInfo
    {
        private static Dictionary<string, string> deviceToDriveMap = new Dictionary<string, string>();

        static FileHandleInfo()
        {
            //Get a list of volumes in NT path format, i.e. \\?\Volume{<guid>}\
            var volumes = Kernel32.FindVolumes();

            foreach (var volume in volumes)
            {
                //Our QueryDosDeviceW wrapper will trim the leading \\?\ and trailing \
                //and give us a path in the form \\Device\HarddiskVolume<number>
                var device = Kernel32.QueryDosDeviceW(volume).First();

                //Separately to the above, translate the NT volume path to a drive letter e.g. C:\
                var drive = Kernel32.GetVolumePathNamesForVolumeNameW(volume).FirstOrDefault();

                //There could be a device with no drive letter attached to it
                if (drive != null)
                {
                    deviceToDriveMap[device] = drive;
                }
            }
        }

        public FileHandleInfo(IntPtr raw) : base(raw, HandleType.File)
        {
        }

        protected override string GetHandleName()
        {
            var name = base.GetHandleName();

            foreach (var kv in deviceToDriveMap)
            {
                if (name.StartsWith(kv.Key))
                {
                    name = kv.Value + name.Substring(kv.Key.Length + 1);
                }
            }

            return name;
        }
    }
}
