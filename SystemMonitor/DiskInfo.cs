using System;
using System.IO;

namespace SystemMonitor
{
    public class DiskInfo
    {
        readonly DriveInfo drive;

        public DiskInfo()
        {
            drive = new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory));
        }

        public double UsagePercentage => 100 - (Available * 1.0 / Total * 100);

        public long Total => drive.TotalSize;

        public long Available => drive.TotalFreeSpace;

        public long Used => Total - Available;
    }
}
