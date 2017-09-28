using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint localServerEndpoint = new IPEndPoint(IPAddress.Parse("172.17.6.3"), 3500);

            server.Bind(localServerEndpoint);
            server.Listen(1);
            Console.WriteLine("Server started");

            bool notFinished = true;
            int threadCount = 0;

            while (notFinished)
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
