using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Configuration;

namespace ClientProyect
{
    class Client
    {

        public static bool connected = false;

        static void Main(string[] args)
        {
            ClientStart();

        }

        private static void ClientStart()
        {
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                string clientIP = ConfigurationManager.AppSettings["clientIP"];
                Random randomValue = new Random();
                int port = randomValue.Next(6001, 6500);
                
                int serverPort = Int32.Parse(ConfigurationManager.AppSettings["serverPort"]);
                string serverIP = ConfigurationManager.AppSettings["serverIP"];

                client.Bind(new IPEndPoint(IPAddress.Parse(clientIP), port));
                client.Connect(IPAddress.Parse(serverIP), serverPort);

                connected = true;

                while (connected)
                {
                    string dataToSend = Console.ReadLine();
                    if (string.Compare(dataToSend, "exit") == 0)
                    {
                        connected = false;
                    }
                    else
                    {
                        byte[] data = Encoding.UTF8.GetBytes(dataToSend);
                        int iDataSent = client.Send(data);
                        Console.WriteLine("Sent " + iDataSent + " bytes");
                    }
                }
                client.Shutdown(SocketShutdown.Both);
                client.Close();
                Console.ReadLine();
            }
            catch (SocketException sE)
            {
                Console.WriteLine("Catched socket exception: " + sE.Message);
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Catched exception: " + e.Message);
                Console.ReadLine();
            }
        }
    }
}