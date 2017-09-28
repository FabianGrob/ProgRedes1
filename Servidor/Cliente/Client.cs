using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace Cliente
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                EndPoint localEP = new IPEndPoint(IPAddress.Parse("172.17.6.3"), 3400); //IP de mi maquina, puerto cualquiera libre              
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("172.17.6.1"), 3500); //IP de la maquina servidor, puerto cualquiera libre

                client.Bind(localEP);
                client.Connect(remoteEP);
                bool notFinished = true;
                while (notFinished)
                {
                    string dataToSend = Console.ReadLine();
                    if (string.Compare(dataToSend, "exit") == 0)
                    {
                        notFinished = false;
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