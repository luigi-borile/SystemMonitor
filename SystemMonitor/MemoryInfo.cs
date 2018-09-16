using Microsoft.VisualBasic.Devices;

namespace SystemMonitor
{
    public class MemoryInfo
    {
        readonly ComputerInfo _pcInfo;

        public MemoryInfo()
        {
            _pcInfo = new ComputerInfo();
        }

        public double UsagePercentage => 100 - (Available * 1.0 / Total * 100);

        public ulong Total => _pcInfo.TotalPhysicalMemory;

        public ulong Available => _pcInfo.AvailablePhysicalMemory;

        public ulong Used => Total - Available;
    }
}
