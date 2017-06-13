using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace serwer
{
    class Program
    {
        const String DISCOVER = "DISCOVER";
        const string PATH_SERVER = "servers.json";
        const string PATH_USER = "user.json";
        const String IP_ADDRESS = "239.0.0.222";
        const string ANSWER_YES = "y";
        const string ANSWER_NO = "n";
        private const int PORT_NUMBER = 7;
        private const int WRONG_CHOOSE = -1;
        static IPAddress multicastAddress = IPAddress.Parse(IP_ADDRESS);
        static List<Server> serverList = new List<Server>();
        private int indexOfServer;
        static User user;
        static UdpClient udpClient = new UdpClient();

        static void Main(string[] args)
        {
            getUserName();
            loadServersFromJson();
            Console.WriteLine("Hello " + user.userName);




            while (serverList.Count == 0)
            {

                Console.WriteLine("==========Pusta lista serverow szukam dostepnych serwerow=======================");
                serverSerice();

            }
            showAndSearchNewsServers();



            Console.WriteLine("====================================================");

            Server server = null;

            while (server == null & serverList.Count > 0)
            {
                try
                {

                    int serverIndex = choseAServer();
                    serverList.ElementAt(serverIndex).Choosed = true;
                    server = serverList.ElementAt(serverIndex);
                    saveServersToJson();
                }

                catch (Exception ex)
                {
                    Console.WriteLine("nie ma takiego serwera");
                }
            }



            TcpIpConnect(server.Ip, server.Port);

            saveServersToJson();

            //    TcpIpConnect(server.Ip,(int) server.Port);
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
                //71 linia breakpoint
                string strData = "";
                Thread.Sleep(1200);
                while (udpClient.Available > 0)
                {
                    data = udpClient.Receive(ref localEP);
                    Console.WriteLine(data);
                    strData = Encoding.ASCII.GetString(data);
                    serverList.Add(parseIPandPortNewServer(strData));

                }

                sendMessage("END", portNumber);




            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            udpClient.Close();

        }

        private static Server parseIPandPortNewServer(string strData)
        {
            string[] serverData = strData.Split(':');
            if (serverData.Length == 2)
                return new Server(serverData[0], int.Parse(serverData[1]), false);

            else return null;
        }

        private static void saveServersToJson()
        {

            string json = JsonConvert.SerializeObject(serverList, Formatting.Indented);
            File.WriteAllText(PATH_SERVER, json);

            string newJson = JsonConvert.SerializeObject(user, Formatting.Indented);
            File.WriteAllText(PATH_USER, newJson);
        }

        private static void loadServersFromJson()
        {
            try
            {
                string fileStream = File.ReadAllText(PATH_SERVER);
                serverList = JsonConvert.DeserializeObject<List<Server>>(fileStream);

            }
            catch (Exception e)
            {
                Console.WriteLine("===========Nie ma zadnych serwerow z ktorych wczesniej korzystales==============");
            }
        }

        private static void showAndSearchNewsServers()
        {
            if (serverList.Count > 0)
            {
                Console.WriteLine("==========================Last finded servers========================");
                printAllAvailibiesServers();
                Console.WriteLine("=======================Do you want to search new servers y/n ?========================");

                switch (Console.ReadLine())
                {

                    case ANSWER_YES:
                        serverList.Clear();
                        serverSerice();
                        Console.WriteLine("==========================Znalazłem takie serwery========================");
                        printAllAvailibiesServers();
                        break;

                    case ANSWER_NO:
                        sendMessage("END", PORT_NUMBER);
                        udpClient.Close();
                        break;

                }
            }
            else
            {
                serverSerice();
                Console.WriteLine("========================Znalezione Serwery===========================");
                for (int i = 0; i < serverList.Count; i++)
                    Console.WriteLine((i + 1) + "    " + serverList.ElementAt(i).Ip + ":" + serverList.ElementAt(i).Port);
            }
        }

        private static void printAllAvailibiesServers()
        {
            for (int i = 0; i < serverList.Count; i++)
            {
                Server server = serverList.ElementAt(i);
                if (server.Choosed == true)
                    Console.WriteLine((i + 1) + "    " + server.Ip + ":" + server.Port + " ostatnio polaczony");
                else
                    Console.WriteLine((i + 1) + "    " + server.Ip + ":" + server.Port);
            }
        }

        private static void serverSerice()
        {
            sendMessage(DISCOVER, PORT_NUMBER);
            getServerIPs(DISCOVER, PORT_NUMBER);
        }

        private static void getUserName()
        {
            try
            {
                string fileStream = File.ReadAllText(PATH_USER);
                user = JsonConvert.DeserializeObject<User>(fileStream);

            }
            catch (Exception e)
            {

                Console.WriteLine("NOwy uzytkownik");
                Console.WriteLine("Write your name ");
                user = new User(Console.ReadLine());

            }



        }

        // to do : wysylanie numeru portu 


        private static void TcpIpConnect(String ip, int port)
        {
            Thread.Sleep(1000);
            try
            {
                TcpClient socket = new TcpClient(new IPEndPoint(IPAddress.Parse(ip), port));


                try { socket.Client.Connect(new IPEndPoint(IPAddress.Parse(ip), port)); }
                catch (SocketException ex)
                {

                    Console.WriteLine("nie moge polaczyc sie z ip blad : " + ex);
                }

                byte[] buffer = null;

                buffer = Encoding.ASCII.GetBytes("NICK:" + user);
                socket.Client.Send(buffer);
                buffer = null;
                int freqency = 0;

                Console.WriteLine("podaj czestotliwosc wysylania losowych liczb w zakresie 10 - 10 000 ms");
                const int MIN_VALUE_FREQUENCY = 10;
                const int MAX_VALUE_FREQUENCY = 10000;

                while (freqency < MIN_VALUE_FREQUENCY | freqency > MAX_VALUE_FREQUENCY)
                {
                    try
                    {
                        freqency = int.Parse(Console.ReadLine());
                        buffer = Encoding.ASCII.GetBytes("FREQ:" + freqency);
                        socket.Client.Send(buffer);
                        buffer = null;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("wpisz wartosc liczbowa");
                    }
                }
                Console.WriteLine("===========wysylanie licz ============ ");

                for (int i = 0; i < 5; i++)
                {

                    string liczba = randomValue();
                    Console.WriteLine("wyslalem   = " + liczba);
                    buffer = Encoding.ASCII.GetBytes(liczba);
                    socket.Client.Send(buffer);
                    buffer = null;
                    Thread.Sleep(freqency);
                }

                socket.Client.Disconnect(true);
                socket.Close();

                Console.WriteLine("Shut Down server :) ");
            }
            catch (Exception ex)
            {
                Console.WriteLine("problem z polaczeniem TCP/IP sprawdz adres IP exception " + ex.StackTrace);

            }

        }

        private static string randomValue()
        {
            Random random = new Random();
            return "VALUE:" + random.Next(1, 543).ToString();
        }


        private static int choseAServer()
        {

            if (serverList.Count == 0) serverSerice();
            Console.WriteLine("Wpisz nr serwera do ustawienia polaczenia");
            try
            {

                int choosenIndex = int.Parse(Console.ReadLine()) - 1;

                while (choosenIndex <= WRONG_CHOOSE)
                {
                    Console.WriteLine("podales zly index ");
                    return WRONG_CHOOSE;
                }

                return choosenIndex;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Twoj wybor jest nie wlasciwy popraw go ");
                return WRONG_CHOOSE;
            }
        }


    }
}
