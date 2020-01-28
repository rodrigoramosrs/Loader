using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;

namespace Loader.Infra.Manager
{
    

    public class NetworkManager
    {
        public class NetworkMetrics
        {
            public string Name;
            public long Ipv4BytesSent;
            public long Ipv4BytesReceived;

            public long BytesSent;
            public long BytesReceived;
        }

        public List<NetworkMetrics> GetMetrics()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return new List<NetworkMetrics>();

            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            var NetworkMetricsResult = new List<NetworkMetrics>();
            List<NetworkInterfaceType> inclusionList = new List<NetworkInterfaceType>()
            { NetworkInterfaceType.Ethernet, NetworkInterfaceType.Ethernet3Megabit, NetworkInterfaceType.FastEthernetT, NetworkInterfaceType.GigabitEthernet };
            foreach (NetworkInterface ni in interfaces)
            {
                if (!inclusionList.Contains(ni.NetworkInterfaceType) || ni.OperationalStatus != OperationalStatus.Up) continue;

                var ipv4Statistics = ni.GetIPv4Statistics();
                var ipStatistics = ni.GetIPStatistics();
                NetworkMetricsResult.Add(new NetworkMetrics() {
                    Name = ni.Name,
                    BytesReceived = ipStatistics.BytesReceived,
                    BytesSent = ipStatistics.BytesSent,
                    Ipv4BytesSent = ipv4Statistics.BytesSent,
                    Ipv4BytesReceived = ipv4Statistics.BytesReceived,
                });

               
            }

            return NetworkMetricsResult;
        }

        

    }
}


