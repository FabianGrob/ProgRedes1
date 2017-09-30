using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Domain;

namespace ServerProyect
{
    public class Server
    {

        public static bool running = true;
        public List<User> registeredUsers = new List<User>();
        public List<Chat> chats = new List<Chat>();


        static void Main(string[] args)
        {
            ServerStart();
        }

        private static void ServerStart()
        {
            try
            {
                string serverIP = ConfigurationManager.AppSettings["serverIP"];
                int serverPort = Int32.Parse(ConfigurationManager.AppSettings["serverPort"]);

                Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint localServerEndpoint = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
                server.Bind(localServerEndpoint);
                server.Listen(10);
                Console.WriteLine("Server started");

                
                int threadCount = 0;

                while (running)
                {
                    Socket clientConnection = server.Accept();
                    threadCount++;
                    int i = threadCount;
                    Thread clientTrhead = new Thread(() => threadFunc(clientConnection, i));
                    clientTrhead.Start();
                }
                server.Close();
                Console.WriteLine("Server finished");
                Console.ReadLine();
            }
            catch (Exception)
            {
                Console.WriteLine("Imposible conectar");
            }
            
        }

        static void threadFunc(Socket clientConnection, int threadId)
        {
            bool notFinishedThread = true;
            while (notFinishedThread)
            {
                byte[] data = new byte[1024];
                int dataRecived = clientConnection.Receive(data);
                if (dataRecived == 0)
                {
                    Console.WriteLine("ThreadID: " + threadId + "Connection closed from remote endpoint");
                    clientConnection.Shutdown(SocketShutdown.Both);
                    clientConnection.Close();
                    notFinishedThread = false;
                }
                else
                {
                    String text = Encoding.UTF8.GetString(data);
                    Console.WriteLine("ThreadID: " + threadId + "Data received: " + text.Trim());
                }
            }
        }
    }
}
