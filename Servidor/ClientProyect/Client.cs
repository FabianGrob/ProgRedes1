﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Configuration;
using System.Threading;
using Connection;

namespace ClientProyect
{
    public class Client
    {

        public static IProtocol protocol = new Protocol();
        public static bool connectedToServer = false;
        public static bool userLogedIn = false;
        public static bool keepConnection = true;
        public static string user = null;
        public static bool chating = false;



        static void Main(string[] args)
        {
            ClientStart();

        }

        private static void ClientStart()
        {
            TcpClient clientSocket = new TcpClient();
            //Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                string clientIP = ConfigurationManager.AppSettings["clientIP"];
                Random randomValue = new Random();
                int port = randomValue.Next(6001, 6500);

                int serverPort = Int32.Parse(ConfigurationManager.AppSettings["serverPort"]);
                string serverIP = ConfigurationManager.AppSettings["serverIP"];

                //client.Bind(new IPEndPoint(IPAddress.Parse(clientIP), port));
                //client.Connect(IPAddress.Parse(serverIP), serverPort);

                clientSocket.Connect(serverIP, port);
                NetworkStream clientStream = clientSocket.GetStream();
                
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
                    SendOption(0, clientStream);

                    protocol.SendData(userName, clientStream);
                    protocol.SendData(password, clientStream);
                    userLogedIn = ReceiveConectionAccess(clientStream);

                    if (userLogedIn)
                    {
                        user = userName;
                    }
                }


                while (keepConnection)
                {
                    int option = MainMenu();
                    SendOption(option, clientStream);
                    string message = protocol.ReceiveData(clientStream);
                    ProcessOption(message, clientStream, option);
                }

                clientStream.Shutdown(SocketShutdown.Both);
                clientStream.Close();
                Console.ReadLine();
            }
        }

        private static bool ReceiveConectionAccess(NetworkStream socket)
        {
            var message = protocol.ReceiveData(socket);

            switch (message)
            {
                case "CONNECT":
                    Console.WriteLine("Usuario conectado");
                    return true;
                case "REGISTERED":
                    Console.WriteLine("Usuario registrado");
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
            int option = convertToInt(line);
            while (!(option <= 6 && option > 0) || !IsValidOption(line))
            {
                Console.WriteLine("Opcion no valida, seleccione una opcion correcta");
                line = Console.ReadLine();
                option = convertToInt(line);
            }
            return option;
        }

        private static void SendOption(int option, NetworkStream clientStream)
        {
            string information = option + "%" + user;

            protocol.SendData(information, clientStream);
        }

        private static void SendName(string name, NetworkStream clientStream)
        {
            string information = name + "%" + user;

            protocol.SendData(information, clientStream);
        }

        private static void ProcessOption(string message, NetworkStream clientStream, int option)
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
                        break;
                    }
                    SendName(line3, clientStream);
                    string serverResponse3 = protocol.ReceiveData(clientStream);
                    switch (serverResponse3)
                    {
                        case "WRONGNAME":
                            Console.WriteLine("El usuario no existe.");
                            ProcessOption(message, clientStream, option);
                            break;

                        case "ALREADYADDED":
                            Console.WriteLine("Tu y este usuario ya eran amigos.");
                            break;

                        case "REQUESTSENT":
                            Console.WriteLine("Solicitud enviada.");
                            break;

                        case "ADDED":
                            Console.WriteLine("Agregado con exito!");
                            break;
                    }
                    break;
                case 4:
                    PrintUsers(message);
                    Console.WriteLine("Escriba el nombre del usuario al que quiere responder la solicitud '0' para volver:");
                    string line4 = Console.ReadLine();
                    if (line4.Equals("0"))
                    {
                        break;
                    }
                    SendName(line4, clientStream);
                    string serverResponse4 = protocol.ReceiveData(clientStream);
                    switch (serverResponse4)
                    {
                        case "WRONGNAME":
                            Console.WriteLine("El usuario no existe.");
                            ProcessOption(message, clientStream, option);
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
                                break;
                            }

                            SendOption(option, clientStream);
                            string response = protocol.ReceiveData(clientStream);
                            Console.WriteLine(response);
                            break;
                    }
                    break;
                case 5:
                    PrintUsers(message);
                    Console.WriteLine("Escriba el nombre del usuario con el que quiere abrir un chat o '0' para volver atras.");
                    string line5 = Console.ReadLine();
                    if (line5.Equals("0"))
                    {
                        break;
                    }
                    SendName(line5, clientStream);
                    string serverResponse5 = protocol.ReceiveData(clientStream);
                    switch (serverResponse5)
                    {
                        case "WRONGNAME":
                            Console.WriteLine("El usuario no existe o no es amigo tuyo.");
                            ProcessOption(message, clientStream, option);
                            break;

                        case "OK":

                            break;
                    }
                    break;
                case 6:
                    keepConnection = false;
                    Console.WriteLine(message);
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

        public static int convertToInt(string line)
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
    }
}