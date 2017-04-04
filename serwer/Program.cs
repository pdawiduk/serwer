using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
        const string PATH = "servers.json";
        const String IP_ADDRESS = "239.0.0.222";
        const string ANSWER_YES = "y";
        const string ANSWER_NO = "n";
        const int PORT_NUMBER = 7;
        
        static IPAddress multicastAddress = IPAddress.Parse(IP_ADDRESS);
        static List<Server> serverList = new List<Server>();

        static void Main(string[] args)
        {
            loadServersFromJson();
            saveServersToJson();

            Console.WriteLine("adresy IP ");
            foreach (Server strIp in serverList)
                Console.WriteLine("ip na liscie " + strIp.Ip);
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
            Console.WriteLine(IPAddress.Any.ToString());
            udpClient.JoinMulticastGroup(multicastAddress);

            Byte[] data = null;
            try
            {
                while (udpClient.Available > 0)
                {
                    data = udpClient.Receive(ref localEP);
                    string strData = Encoding.ASCII.GetString(data);
                    serverList.Add(new Server(strData));
                }

                if (serverList.Count == 0) {
                    Console.WriteLine(" I don't found any servers !!!!! ");
                    Console.WriteLine("Check your's internet connect");
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());
            }


        }

        private static void saveServersToJson()
        {

            string json = JsonConvert.SerializeObject(serverList, Formatting.Indented);
            File.WriteAllText(PATH, json);
        }

        private static void loadServersFromJson()
        {
            string fileStream = File.ReadAllText(PATH);
            List<Server> tmpServers = JsonConvert.DeserializeObject<List<Server>>(fileStream);

            Console.WriteLine("==========================Last finded servers========================");
            for (int i = 0; i < tmpServers.Count; i++)
                Console.WriteLine(i + 1 + "    " + tmpServers.ElementAt(i).Ip);
            Console.WriteLine("=======================Do you want to search new servers y/n ?========================");
            string answer = Console.ReadLine();

            switch (answer) {

                case ANSWER_YES:
                    sendMessage(DISCOVER, PORT_NUMBER);
                    getServerIPs(DISCOVER, PORT_NUMBER);
                    break;

                case ANSWER_NO:
                    serverList = tmpServers;
                    break;
               
            }
        }
    }
}
