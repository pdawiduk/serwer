using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace serwer
{
    class Program
    {
        const String DISCOVER = "DISCOVER";
        const int portNumber = 7;
        const String ipAddress = "239.0.0.222";
        static IPAddress multicastAddress = IPAddress.Parse(ipAddress);
        static List<string> serverList = new List<string>();

        static void Main(string[] args)
        {

            sendMessage(DISCOVER, portNumber);
            getServerIPs(DISCOVER, portNumber);

            Console.WriteLine("adresy IP ");
            foreach (String strIp in serverList)
                Console.WriteLine("ip na liscie " + strIp);
            Console.ReadKey();
        }

        private static void sendMessage(string DISCOVER, int portNumber)
        {
            UdpClient udpClient = new UdpClient();

            udpClient.JoinMulticastGroup(multicastAddress);
            IPEndPoint serverIP = new IPEndPoint(multicastAddress, portNumber);

            Byte[] buffer = Encoding.ASCII.GetBytes(DISCOVER);
            udpClient.Send(buffer, buffer.Length, serverIP);
        }

        private static void getServerIPs(string DISCOVER, int portNumber)
        {
            UdpClient udpClient = new UdpClient();
            IPEndPoint localEP = new IPEndPoint(IPAddress.Any, portNumber);
            udpClient.ExclusiveAddressUse = false;
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            udpClient.ExclusiveAddressUse = false;
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, portNumber));
            udpClient.JoinMulticastGroup(multicastAddress);

            Byte[] data = null;

            while ((data = udpClient.Receive(ref localEP)) != null)
            {
                 Console.WriteLine("mam serwer");
                 string strData = Encoding.ASCII.GetString(data);
                 serverList.Add(strData);
                 data = null;
            }

            
          
        }
    }
}
