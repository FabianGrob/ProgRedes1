using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using System.Text;
using System.Threading;
using Connection;
using Domain;

namespace ServerProyect
{
    public class Server
    {

        public static IProtocol protocol = new Protocol();
        public static bool running = true;
        public List<User> registeredUsers = new List<User>();
        public List<Chat> chats = new List<Chat>();
        public static Object userLocker = new Object();
        public static Object locker2 = new Object();


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
                Console.WriteLine("Esperando cliente...");
                
                while (running)
                {
                    Socket client = server.Accept();
                    Console.WriteLine("Cliente conectado con éxito!");
                    Thread clientTrhead = new Thread(() => ClientThread(client));
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

        static void ClientThread(Socket client)
        {
            try
            {
                string message = protocol.ReceiveMessage(client);

                var information = message.Split('%');
                int option = Convert.ToInt32(information[0]);
                string name = "";

                ProcessOption(option, name, client);
                if (option != 4)
                {
                    ClientThread(client);
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Cliente desconectado con éxito!");
            }
        }

        public static void ProcessOption(int option, string userName, Socket client)
        {
            switch (option)
            {
                case 0:                   
                    string message = "ERROR";
                    message = ReceiveUserData(client);
                    protocol.SendData(message, client);
                    break;
                case 1:
                    string userList = GetConnectedUsers();
                    protocol.SendData(userList, client);
                    break;
                case 2:
                    string userList = GetFriends(userName);
                    protocol.SendData(userList, client);
                    break;
                case 3:
                    string userList = GetUsersToAdd(userName);
                    protocol.SendDate(userList, client);
                    string data = protocol.ReceiveData(client);
                    string response = SendFriendRequest(data);
                    protocol.SendData(response, client);
                    break;
                case 4:                    
                    break;
            }
        }

        private static string ReceiveUserData(Socket socketClient)
        {
            var userName = protocol.ReceiveMessage(socketClient);
            var password = protocol.ReceiveMessage(socketClient);

            lock (userLocker)
            {
                User userAux = new User
                {

                    UserName = userName,
                    Password = password,
                    Friends = new List<User>(),
                    PendingFriends = new List<User>(),
                    Connected = true,
                    Connections = 0,
                    ConnectedSince = DateTime.Now
                };

                if (registeredUsers.Contains(userAux))
                {
                    User user = GetUser(userName);
                    if (user.Connected)
                    {
                        Console.WriteLine("Conexion duplicada");
                        return "ERROR";
                    }
                    else
                    {
                        if (user.Password.Equals(password))
                        {
                            user.Connection();

                            Console.WriteLine("Autentificación del cliente " + userName + " reailzada con éxito!");
                            return "CONNECT";
                        }
                        else
                        {
                            Console.WriteLine("Autentificación incorrecta!");
                            return "ERROR";
                        }
                    }
                                       
                }
                else
                {
                    userAux.Connection();
                    registeredUsers.Add(userAux);
                    Console.WriteLine("Se registro el usuario: " + userName);
                    return "REGISTERED";
                }
            }

        }

        public User GetUser(string userName)
        {
            lock (userLocker)
            {
                foreach (User user in registeredUsers)
                {
                    if (user.UserName.Equals(userName))
                    {
                        return user;
                    }

                }
                return null;
            }           
        }

        public string GetConnectedUsers()
        {
            string users = "Los usuarios conectados son:#";
            int count = 1;
            lock (userLocker)
            {
                foreach (User user in registeredUsers)
                {
                    if (user.Connected)
                    {
                        users = (users + $"{count}. {user.UserName}#");
                        count++;
                    }
                }
                return users;
            }
            
        }

        public string GetFriends(string userName)
        {
            string users = "Tus amigos son:#";
            int count = 1;
            User userReq = GetUser(userName);
            lock (userLocker)
            {
                foreach (User user in userReq.Friends)
                {
                    users = (users + $"{count}. {user.UserName}.  Cantidad de amigos: {user.Friends.Count}#");
                    count++;
                }
                return users;
            }
        }

        public string GetUsersToAdd(string userName)
        {
            string users = "Los usuarios son:#";
            int count = 1;
            lock (userLocker)
            {
                foreach (User user in registeredUsers)
                {
                    if (!user.UserName.Equals(userName))
                    {
                        users = (users + $"{count}. {user.UserName}#");
                        count++;
                    }
                }
                return users;
            }
        }

        public string SendFriendRequest(string data)
        {
            string[] splitedData = data.Split('%');
            User userToAdd = GetUser(splitedData[0]);
            User activeUser = GetUser(splitedData[1]);
            if (userToAdd != null)
            {
                lock (userLocker)
                {
                    if (userToAdd.Friends.Contains(activeUser))
                    {
                        return "ALREADYADDED";
                    }
                    else
                    {
                        if (userToAdd.PendingFriends.Contains(activeUser))
                        {
                            return "REQUESTSENT";
                        }
                        else
                        {
                            if (activeUser.PendingFriends.Contains(userToAdd))
                            {
                                activeUser.PendingFriends.Remove(userToAdd);
                                activeUser.Friends.Add(userToAdd);
                                userToAdd.Friends.Add(activeUser);
                                return "ADDED";
                            }
                            else
                            {
                                userToAdd.PendingFriends.Add(activeUser);
                                return "REQUESTSENT";
                            }
                        }
                    }                   
                }
            }
            else
            {
                return "WRONGNAME";
            }
        }
    }
}
