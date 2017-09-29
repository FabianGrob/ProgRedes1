using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Configuration;
using System.Threading;

namespace ClientProyect
{
    class Client
    {

        public static bool connectedToServer = false;
        public static bool userLogedIn = false;

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

                connectedToServer = true;
            }
            catch (SocketException)
            {
                Console.WriteLine("Error. Imposible conectar");
                Thread.Sleep(2000);
                ClientStart();
            }


            if (connectedToServer)
            {
                Console.WriteLine("Ingrese su nombre de usuario: ");
                while (!userLogedIn)
                {
                    string userName = Console.ReadLine();
                    while (userName.Equals(""))
                    {
                        Console.WriteLine("Ingrese su nombre de usuario: ");
                        userName = Console.ReadLine();
                    }
                   
                    if (string.Compare(userName, "exit") == 0)
                    {
                        connectedToServer = false;
                    }

                    else
                    {
                        byte[] data = Encoding.UTF8.GetBytes(userName);
                        int iDataSent = client.Send(data);
                        Console.WriteLine("Sent " + iDataSent + " bytes");
                    }
                }


                client.Shutdown(SocketShutdown.Both);
                client.Close();
                Console.ReadLine();
            }
        }
            
            
        
    }
}