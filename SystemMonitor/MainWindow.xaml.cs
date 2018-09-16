using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace SystemMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string NUMBER_FORMAT = "0.0";
        readonly CPUInfo _cpuInfo;
        readonly MemoryInfo _memoryInfo;
        readonly NetworkInfo _networkInfo;
        readonly DiskInfo _diskInfo;
        readonly DispatcherTimer _timerCpu, _timerRam, _timerDisk, _timerNetwork;

        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize system statistics reader
            _cpuInfo = new CPUInfo();
            _memoryInfo = new MemoryInfo();
            _networkInfo = new NetworkInfo();
            _diskInfo = new DiskInfo();

            cpuGraph.LineColor =
                ramGraph.LineColor =
                cpuTempGraph.LineColor =
                (Brush)TryFindResource("LineColor");
            cpuGraph.BackColor =
                ramGraph.BackColor =
                cpuTempGraph.BackColor =
                (Brush)TryFindResource("BackColor");

            // Set the window location to top-left
            Top = 0;
            Left = SystemParameters.PrimaryScreenWidth - Width;

            // Initialize timers
            _timerCpu = new DispatcherTimer(TimeSpan.FromMilliseconds(1000), DispatcherPriority.Background, TimerCpu_Tick, Dispatcher);
            _timerRam = new DispatcherTimer(TimeSpan.FromMilliseconds(2000), DispatcherPriority.Background, TimerRam_Tick, Dispatcher);
            _timerDisk = new DispatcherTimer(TimeSpan.FromMilliseconds(5000), DispatcherPriority.Background, TimerDisk_Tick, Dispatcher);
            _timerNetwork = new DispatcherTimer(TimeSpan.FromMilliseconds(1000), DispatcherPriority.Background, TimerNetwork_Tick, Dispatcher);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Helper.SetBottom(this);

            _timerCpu.Start();
            _timerRam.Start();
            _timerDisk.Start();
            _timerNetwork.Start();
        }

        private void TimerCpu_Tick(object sender, EventArgs e)
        {
            var usage = _cpuInfo.UsagePercentage;
            var temp = _cpuInfo.Temperature;

            cpuGraph.SetNewValue(usage);
            txtCpu.Text = $"{usage.ToString(NUMBER_FORMAT)} %";

            cpuTempGraph.SetNewValue(temp);
            txtCpuTemp.Text = $"{temp.ToString(NUMBER_FORMAT)} °C";
        }

        private void TimerRam_Tick(object sender, EventArgs e)
        {
            var usage = _memoryInfo.UsagePercentage;

            ramGraph.SetNewValue(usage);

            txtRam.Text = $"{Helper.GetBytesFormatted(_memoryInfo.Used)} / " +
                    $"{Helper.GetBytesFormatted(_memoryInfo.Total)} - {usage.ToString(NUMBER_FORMAT)} %";
        }

        private void TimerDisk_Tick(object sender, EventArgs e)
        {
            var usage = _diskInfo.UsagePercentage;
            barDisco.Value = usage;

            txtDisk.Text = $"{Helper.GetBytesFormatted(_diskInfo.Used)} / " +
                        $"{Helper.GetBytesFormatted(_diskInfo.Total)} - {usage.ToString(NUMBER_FORMAT)} %";
        }

        private void TimerNetwork_Tick(object sender, EventArgs e)
        {
            _networkInfo.GetNetworkSpeed(out string down, out string up);

            txtDownload.Text = down;
            txtUpload.Text = up;
            txtLocalIP.Text = _networkInfo.LocalIP;
            txtPublicIP.Text = _networkInfo.PublicIP;
        }
    }
}
