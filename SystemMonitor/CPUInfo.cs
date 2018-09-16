using System;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace SystemMonitor
{
    public class CPUInfo
    {
        private const int INDEX = 0x1a2;
        readonly PerformanceCounter _useInfo;
        readonly Ols _ols;
        readonly int _cores;
        readonly bool _canReadTemp;

        public CPUInfo()
        {
            _useInfo = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _ols = new Ols();
            _cores = GetCoresCount();

            _canReadTemp = _ols.GetStatus() == 0;
        }

        public double UsagePercentage => _useInfo.NextValue();

        public double Temperature => GetCpuTemp();

        private double GetCpuTemp()
        {
            try
            {
                if (!_canReadTemp)
                {
                    return 0;
                }

                double[] temps = new double[_cores];

                int r = 0;
                uint eax = 0, edx = 0;

                // MSR_TEMPERATURE_TARGET
                // for PROCHOT assertion (probably same as TjMax), only works on i7
                r = _ols.Rdmsr(INDEX, ref eax, ref edx);
                uint tjmax = (eax >> 16) & 0xFF;

                for (int i = 0; i < _cores; i++)
                {
                    UIntPtr mask = (UIntPtr)(1 << (i * 4 / 2));

                    r = _ols.RdmsrPx(0x19c, ref eax, ref edx, mask);

                    if (r != 0 && (eax & 0x80000000) != 0)
                    {
                        temps[i] = tjmax - ((eax >> 16) & 0x7f);
                    }
                }

                return temps.Average();
            }
            catch
            {
                return 0;
            }
        }

        private static int GetCoresCount()
        {
            int coreCount = 0;
            using (var searcher = new ManagementObjectSearcher("Select NumberOfCores from Win32_Processor").Get())
            {
                foreach (var item in searcher)
                {
                    coreCount += Convert.ToInt32(item["NumberOfCores"]);
                }
                return coreCount;
            }
        }
    }
}
