using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace SystemMonitor
{
    public class NetworkInfo
    {
        const string URL_CHECK_PUBLIC_IP = "http://checkip.dyndns.org";
        const string URL_CHECK_PRIVATE_IP = "www.google.com";
        readonly Stopwatch _networkStopwatch;
        string _ipPubblico;
        long _receivedBytes, _sentBytes;
        bool _reconnected;
        NetworkInterface _netInfo;

        public NetworkInfo()
        {
            _networkStopwatch = new Stopwatch();
        }

        public string PublicIP
        {
            get
            {
                if (!_reconnected)
                {
                    return _ipPubblico;
                }

                try
                {
                    using (var x = WebRequest.Create(URL_CHECK_PUBLIC_IP).GetResponse())
                    {
                        using (var reader = new StreamReader(x.GetResponseStream()))
                        {
                            string response = reader.ReadToEnd();
                            response = response.Split(':')[1];
                            response = response.Substring(0, response.IndexOf('<'));
                            _ipPubblico = response.Trim();
                        }
                    }
                }
                catch
                {
                    _ipPubblico = "-";
                }

                return _ipPubblico;
            }
        }

        public string LocalIP
        {
            get
            {
                try
                {
                    using (var u = new UdpClient(URL_CHECK_PRIVATE_IP, 1))
                    {
                        string ipLocale = ((IPEndPoint)u.Client.LocalEndPoint).Address.ToString();

                        _reconnected = _netInfo == null;
                        _netInfo = _netInfo ?? NetworkInterface.GetAllNetworkInterfaces().Single(ni => 
                            ni.GetIPProperties().UnicastAddresses.Any(ip => ip.Address.ToString().Equals(ipLocale)));
                        _reconnected = _reconnected && _netInfo != null;

                        return ipLocale;
                    }
                }
                catch
                {
                    _netInfo = null;
                    _ipPubblico = "-";
                    return "-";
                }
            }
        }

        public void GetNetworkSpeed(out string download, out string upload)
        {
            if (_netInfo == null)
            {
                download = upload = "-";
                return;
            }

            try
            {
                var ricevuti = _netInfo.GetIPv4Statistics().BytesReceived;
                var kbsRicevuti = (ricevuti - _receivedBytes) / _networkStopwatch.Elapsed.TotalSeconds;

                var inviati = _netInfo.GetIPv4Statistics().BytesSent;
                var kbsInviati = (inviati - _sentBytes) / _networkStopwatch.Elapsed.TotalSeconds;

                _networkStopwatch.Restart();
                _receivedBytes = ricevuti;
                _sentBytes = inviati;

                download = $"{Helper.GetBytesFormatted((ulong)kbsRicevuti)}/s";
                upload = $"{Helper.GetBytesFormatted((ulong)kbsInviati)}/s";
            }
            catch
            {
                download = upload = "-";
            }
        }
    }
}
