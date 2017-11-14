using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Configuration;
using System.Threading;
using Connection;
using System.Messaging;

namespace ClientProyect
{
    public class Client
    {

        public static IProtocol protocol = new Protocol();
        public static bool connectedToServer = false;
        public static bool userLogedIn = false;
        public static bool keepConnection = true;
        public static string user = null;
        public static bool finishChat = false;

        static void Main(string[] args)
        {
            ClientStart();
        }

        private static TcpClient clientSocket = new TcpClient();
        private static NetworkStream clientStream = default(NetworkStream);

        private static void ClientStart()
        {

            try
            {
                string clientIP = ConfigurationManager.AppSettings["clientIP"];
                Random randomValue = new Random();
                int serverPort = Int32.Parse(ConfigurationManager.AppSettings["serverPort"]);
                string serverIP = ConfigurationManager.AppSettings["serverIP"];
                ;
                clientSocket.Connect(serverIP, serverPort);
                clientStream = clientSocket.GetStream();

                connectedToServer = true;
            }
            catch (Exception)
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
                    SendOption(0, clientSocket);

                    string userName = Console.ReadLine();
                    while (userName.Equals(""))
                    {
                        Console.WriteLine("Ingrese su nombre de usuario: ");
                        userName = Console.ReadLine();
                    }
                    Console.WriteLine("Ingrese su contraseña: ");
                    string password = Console.ReadLine();
                    while (password.Equals(""))
                    {
                        Console.WriteLine("Ingrese su contraseña: ");
                        password = Console.ReadLine();
                    }

                    string userPass = $"{userName}#{password}";


                    protocol.SendData(userPass, clientSocket);
                    userLogedIn = ReceiveConectionAccess(clientSocket,userName);

                    if (userLogedIn)
                    {
                        user = userName;
                    }
                }


                while (keepConnection)
                {
                    int option = MainMenu();
                    SendOption(option, clientSocket);
                    string message = protocol.RecieveData(clientSocket);
                    ProcessOption(message, clientSocket, option);
                }

                clientStream.Close();
                Console.ReadLine();
            }
        }

        private static bool ReceiveConectionAccess(TcpClient socket,string userName)
        {
            var message = protocol.RecieveData(socket);

            switch (message)
            {
                case "CONNECT":
                    Console.WriteLine("Usuario conectado");
                    StartQueue();
                    SendToQueue("Se ha conectado con exito el siguiente usuario: " +userName );
                    return true;
                case "REGISTERED":
                    Console.WriteLine("Usuario registrado");
                    StartQueue();
                    SendToQueue("Se ha registrado con exito el siguiente usuario: " + userName );
                    return true;
                case "DUPLICATED":
                    Console.WriteLine("Este usuario ya esta conectado");
                    return false;
                case "PASSWORDERROR":
                    Console.WriteLine("Contraseña incorrecta");
                    return false;
                default:
                    return false;
            }
        }

        private static int MainMenu()
        {
            Console.WriteLine("");
            Console.WriteLine("Menu Principal:");
            Console.WriteLine("1. Ver usuarios conectados");
            Console.WriteLine("2. Ver amigos");
            Console.WriteLine("3. Agregar Amigo");
            Console.WriteLine("4. Solicitudes de amistad");
            Console.WriteLine("5. Chats");
            Console.WriteLine("6. Salir");
            Console.WriteLine("Seleccione la opcion que desea realizar");

            string line = Console.ReadLine();
            int option = ConvertToInt(line);
            while (!(option <= 6 && option > 0) || !IsValidOption(line))
            {
                Console.WriteLine("Opcion no valida, seleccione una opcion correcta");
                line = Console.ReadLine();
                option = ConvertToInt(line);
            }
            return option;
        }

        private static void SendOption(int option, TcpClient clientSocket)
        {
            string information = option + "%" + user;

            protocol.SendData(information, clientSocket);
        }

        private static void SendName(string name, TcpClient clientSocket)
        {
            string information = name + "%" + user;

            protocol.SendData(information, clientSocket);
        }

        private static void ProcessOption(string message, TcpClient clientSocket, int option)
        {
            switch (option)
            {
                case 1:
                    PrintUsers(message);
                    break;
                case 2:
                    PrintUsers(message);
                    break;
                case 3:
                    PrintUsers(message);
                    Console.WriteLine("Ingrese el nombre de usuario a agregar o '0' para volver al menu");
                    string line3 = Console.ReadLine();
                    if (line3.Equals("0"))
                    {
                        SendName(line3, clientSocket);
                        protocol.RecieveData(clientSocket);
                        break;
                    }
                    SendName(line3, clientSocket);
                    string serverResponse3 = protocol.RecieveData(clientSocket);
                    switch (serverResponse3)
                    {
                        case "WRONGNAME":
                            Console.WriteLine("El usuario no existe.");
                            break;

                        case "ALREADYADDED":
                            Console.WriteLine("Tu y este usuario ya eran amigos.");
                            break;

                        case "REQUESTSENT":
                            Console.WriteLine("Solicitud enviada.");
                            StartQueue();
                            SendToQueue("El usuario: "+user + " ha enviado una solicitud a " + line3);
                            break;

                        case "ADDED":
                            Console.WriteLine("Agregado con exito!");
                            StartQueue();
                            SendToQueue("Los usuarios: " + user + " y " + line3 + " ahora son amigos");
                            break;
                    }
                    break;
                case 4:
                    PrintUsers(message);
                    Console.WriteLine("Escriba el nombre del usuario al que quiere responder la solicitud '0' para volver:");
                    string line4 = Console.ReadLine();
                    if (line4.Equals("0"))
                    {
                        SendName(line4, clientSocket);
                        protocol.RecieveData(clientSocket);
                        break;
                    }
                    SendName(line4, clientSocket);
                    string serverResponse4 = protocol.RecieveData(clientSocket);
                    switch (serverResponse4)
                    {
                        case "WRONGNAME":
                            Console.WriteLine("El usuario no existe.");
                            break;

                        case "OK":
                            Console.WriteLine("Ingrese '1' para aceptar, '2' para rechazar o '0' para volver atras.");
                            line4 = Console.ReadLine();
                            option = Convert.ToInt32(line4);

                            while (!(option <= 2 && option >= 0))
                            {
                                Console.WriteLine("Opcion no valida, seleccione una opcion correcta");
                                line4 = Console.ReadLine();
                                option = Convert.ToInt32(line4);
                            }

                            if (option == 0)
                            {
                                SendOption(option, clientSocket);
                                protocol.RecieveData(clientSocket);
                                break;
                            }

                            SendOption(option, clientSocket);
                            string response = protocol.RecieveData(clientSocket);
                            Console.WriteLine(response);
                            StartQueue();
                            SendToQueue("Accion del usuario: " + user + " => " + response);
                            break;
                    }
                    break;
                case 5:
                    PrintUsers(message);
                    Console.WriteLine("Escriba el nombre del usuario con el que quiere abrir un chat o '0' para volver atras.");
                    string contactName = Console.ReadLine();
                    if (contactName.Equals("0"))
                    {
                        SendName(contactName, clientSocket);
                        protocol.RecieveData(clientSocket);
                        break;
                    }
                    SendName(contactName, clientSocket);
                    string serverResponse5 = protocol.RecieveData(clientSocket);
                    switch (serverResponse5)
                    {
                        case "WRONGNAME":
                            Console.WriteLine("El usuario no existe o no es amigo tuyo.");
                            break;

                        case "OK":
                            Console.WriteLine($"Chat con {contactName}:");
                            Console.WriteLine($"Escriba 'exit' para salir.");
                            finishChat = false;
                            StartQueue();
                            SendToQueue("El usuario: " + user + " ha iniciado un chat con: " + contactName);
                            Thread reciveMessageThread = new Thread(() => GetMessage());
                            reciveMessageThread.Start();
                            Thread sendMessageThread = new Thread(() => SendMessage());
                            sendMessageThread.Start();
                            while (!finishChat)
                            {
                            }
                            sendMessageThread.Abort();
                            reciveMessageThread.Abort();
                            
                            break;
                    }
                    break;
                case 6:
                    keepConnection = false;
                    Console.WriteLine(message);
                    StartQueue();
                    SendToQueue("El usuario: " + user + "se ha desconectado");
                    break;

            }
        }



        private static void PrintUsers(string message)
        {
            string[] names = message.Split('#');
            for (int i = 0; i < names.Length; i++)
            {
                Console.WriteLine(names[i]);
            }

        }
        public static bool IsValidOption(string word)
        {
            if (!(word.Length == 1))
            {
                return false;
            }
            string options = "123456";
            if (options.Contains(word))
            {
                return true;
            }
            return false;
        }

        public static int ConvertToInt(string line)
        {
            if (IsValidOption(line))
            {
                return Convert.ToInt32(line);
            }
            else
            {
                return 1000;
            }
        }

        private static void GetMessage()
        {
            while (!finishChat)
            {
                clientStream = clientSocket.GetStream();
                string message = protocol.RecieveData(clientSocket);
                if (message.Equals("/1"))
                {
                    finishChat = true;
                    StartQueue();
                    SendToQueue("El usuario: " + user + " ha salido de su chat abierto");
                }
                else
                {
                    string[] chatHistory = message.Split('#');
                    for (int i = 0; i < chatHistory.Length; i++)
                    {
                        Console.WriteLine(chatHistory[i]);
                    }
                }
            }
        }

        private static void SendMessage()
        {
            while (!finishChat)
            {
                string message = Console.ReadLine();
                string meesageToSend = $"{message}";
                protocol.SendData(meesageToSend, clientSocket);

            }
        }
        public static void StartQueue()
        {
            string queueName = ".\\private$\\test";
            MessageQueue mq;
            if (MessageQueue.Exists(queueName))
            {
                mq = new MessageQueue(queueName);
            }
            else
            {
                mq = MessageQueue.Create(queueName);
            }
        }
        public static void SendToQueue(string messageToSend)
        {
            string queueName = ".\\private$\\test";
            MessageQueue mq;
            if (MessageQueue.Exists(queueName))
            {
                mq = new MessageQueue(queueName);
            }
            else
            {
                mq = MessageQueue.Create(queueName);
            }
            mq.Send(messageToSend);
        }
    }
}