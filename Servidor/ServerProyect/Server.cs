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
        public static List<User> registeredUsers = new List<User>();
        public static List<Chat> chats = new List<Chat>();
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
                string message = protocol.ReceiveData(client);

                var information = message.Split('%');
                int option = Convert.ToInt32(information[0]);
                string name = information[1];

                ProcessOption(option, name, client);
                if (option != 6)
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
                    string userList1 = GetConnectedUsers();
                    protocol.SendData(userList1, client);
                    break;
                case 2:
                    string userList2 = GetFriends(userName);
                    protocol.SendData(userList2, client);
                    break;
                case 3:
                    string userList3 = GetUsersToAdd(userName);
                    protocol.SendData(userList3, client);
                    string data3 = protocol.ReceiveData(client);
                    string response = SendFriendRequest(data3);
                    protocol.SendData(response, client);
                    break;
                case 4:
                    string userList4 = GetPendingFriends(userName);
                    protocol.SendData(userList4, client);
                    string data4 = protocol.ReceiveData(client);
                    string nameCheck = CheckNameOnPendingRequests(data4);
                    protocol.SendData(nameCheck, client);
                    string userDecision = protocol.ReceiveData(client);
                    string finalMessage = FriendRequestProcess(data4, userDecision);
                    protocol.SendData(finalMessage, client);
                    break;

                case 5:

                    break;
                case 6:
                    Disconnect(userName);
                    Console.WriteLine("Desconectado " + userName);
                    protocol.SendData("Desconectado con exito", client);
                    break;
            }
        }

        private static string ReceiveUserData(Socket socketClient)
        {
            var userName = protocol.ReceiveData(socketClient);
            var password = protocol.ReceiveData(socketClient);

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
                        return "DUPLICATED";
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
                            return "PASSWORDERROR";
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

        public static User GetUser(string userName)
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

        public static string GetConnectedUsers()
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

        public static string GetFriends(string userName)
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

        public static string GetUsersToAdd(string userName)
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

        public static string SendFriendRequest(string data)
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

        public static string GetPendingFriends(string userName)
        {
            string users = "Solicitudes pendientes:#";
            int count = 1;
            User activeUser = GetUser(userName);
            lock (userLocker)
            {
                foreach (User user in activeUser.PendingFriends)
                {
                    users = (users + $"{count}. {user.UserName}#");
                    count++;
                }
                return users;
            }
        }

        public static string CheckNameOnPendingRequests(string data)
        {
            string[] splitedData = data.Split('%');
            User activeUser = GetUser(splitedData[1]);
            User userToCheck = GetUser(splitedData[0]);

            if (userToCheck == null)
            {
                return "WRONGNAME";
            }
            else
            {
                lock (userLocker)
                {
                    if (activeUser.PendingFriends.Contains(userToCheck))
                    {
                        return "OK";
                    }
                    else
                    {
                        return "WRONGNAME";
                    }
                }
            }
        }

        public static string FriendRequestProcess(string data, string userDecision)
        {
            string[] splitedData = data.Split('%');
            string[] splitedUserDecision = userDecision.Split('%');
            User activeUser = GetUser(splitedData[1]);
            User userToAccept = GetUser(splitedData[0]);

            lock (userLocker)
            {
                if (splitedUserDecision[0].Equals("1"))
                {

                    activeUser.PendingFriends.Remove(userToAccept);
                    activeUser.Friends.Add(userToAccept);
                    userToAccept.Friends.Add(activeUser);
                    Console.WriteLine($"{activeUser.UserName} acepto la solicitud de {userToAccept.UserName}");
                    return $"Ahora tu y {userToAccept.UserName} son amigos.";

                }
                else
                {

                    activeUser.PendingFriends.Remove(userToAccept);
                    Console.WriteLine($"{activeUser.UserName} rechazo la solicitud de {userToAccept.UserName}");
                    return "Solicitud rechazada.";

                }
            }
            
        }

        public static void Disconnect(string userName)
        {
            User user = GetUser(userName);
            lock (userLocker)
            {
                user.Connected = false;
            }
        }
    }
}
