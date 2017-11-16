using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using System.Text;
using System.Threading;
using Connection;
using Domain;
using System.Collections;
using System.Messaging;
using System.IO;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;

namespace ServerProyect
{
    public class Server : RemotingShared
    {

        public static IProtocol protocol = new Protocol();
        public static bool running = true;
        public static List<User> registeredUsers = new List<User>();
        public static List<Chat> chats = new List<Chat>();
        public static Object userLocker = new Object();
        public static Object chatsLocker = new Object();
        public static Hashtable clientsList = new Hashtable();



        static void Main(string[] args)
        {
            //StartQueue();
            ServerStart();
        }

        private static void ServerStart()
        {
            string serverIP = ConfigurationManager.AppSettings["serverIP"];
            int serverPort = Int32.Parse(ConfigurationManager.AppSettings["serverPort"]);
            ServerIpAddress = ConfigurationManager.AppSettings["serverIP"];
            TcpListener serverSocket = new TcpListener(IPAddress.Parse(serverIP), serverPort);
            TcpClient clientSocket = default(TcpClient);

            TcpChannel lTcpChannel = new TcpChannel(RemotingShared.Port);
            ChannelServices.RegisterChannel(lTcpChannel, true);
            Type lRemotingSharedType = typeof(Server);
            RemotingConfiguration.ApplicationName = RemotingShared.RemotingName + "App";
            RemotingConfiguration.RegisterWellKnownServiceType(lRemotingSharedType, RemotingShared.RemotingName, WellKnownObjectMode.Singleton);

            try
            {
                serverSocket.Start();
                Console.WriteLine("Esperando cliente...");

                while (running)
                {
                    clientSocket = serverSocket.AcceptTcpClient();
                    NetworkStream networkStream = clientSocket.GetStream();
                    Console.WriteLine("Cliente conectado con éxito!");
                    Thread clientTrhead = new Thread(() => ClientThread(clientSocket));
                    clientTrhead.Start();
                }
                clientSocket.Close();
                serverSocket.Stop();
                Console.WriteLine("Server finished");
                Console.ReadLine();
            }
            catch (Exception)
            {
                Console.WriteLine("Imposible conectar");
            }

        }

        static void ClientThread(TcpClient client)
        {
            try
            {
                string message = protocol.RecieveData(client);

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

        public static void ProcessOption(int option, string userName, TcpClient client)
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
                    string data3 = protocol.RecieveData(client);
                    string response = SendFriendRequest(data3);
                    protocol.SendData(response, client);
                    break;
                case 4:
                    string userList4 = GetPendingFriends(userName);
                    protocol.SendData(userList4, client);
                    string data4 = protocol.RecieveData(client);
                    string nameCheck = CheckNameOnPendingRequests(data4);
                    protocol.SendData(nameCheck, client);
                    if (nameCheck.Equals("OK"))
                    {
                        string userDecision = protocol.RecieveData(client);
                        string finalMessage = FriendRequestProcess(data4, userDecision);
                        protocol.SendData(finalMessage, client);
                        StartQueue();
                        SendToQueue("Accion del usuario: " + userName + " => " + finalMessage);
                    }
                    break;

                case 5:
                    string userList5 = GetFriends(userName);
                    protocol.SendData(userList5, client);
                    string userNames = protocol.RecieveData(client);
                    string isAFriend = CheckNameIsAFriend(userNames);
                    protocol.SendData(isAFriend, client);
                    if (isAFriend.Equals("OK"))
                    {
                        StartChat(userNames, client);
                    }
                    break;
                case 6:
                    Disconnect(userName);
                    Console.WriteLine("Desconectado " + userName);
                    protocol.SendData("Desconectado con exito", client);
                    break;
            }
        }

        private static string ReceiveUserData(TcpClient socketClient)
        {
            var userPass = protocol.RecieveData(socketClient);
            string[] userData = userPass.Split('#');
            string userName = userData[0];
            string password = userData[1];


            lock (userLocker)
            {
                User userAux = new User
                {

                    UserName = userName,
                    Password = password,
                    ChatingWith = "NO USER",
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
                            clientsList[userName] = socketClient;
                            StartQueue();
                            SendToQueue("Se ha conectado con exito el siguiente usuario: " + userName);
                            return "CONNECT";
                        }
                        else
                        {
                            return "PASSWORDERROR";
                        }
                    }

                }
                else
                {
                    userAux.Connection();
                    registeredUsers.Add(userAux);
                    StartQueue();
                    SendToQueue("Se ha registrado con exito el siguiente usuario: " + userName);
                    clientsList.Add(userName, socketClient);
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
                            StartQueue();
                            SendToQueue("El usuario: " + activeUser.UserName + " ha enviado una solicitud a " + userToAdd.UserName);
                            return "REQUESTSENT";
                        }
                        else
                        {
                            if (activeUser.PendingFriends.Contains(userToAdd))
                            {
                                activeUser.PendingFriends.Remove(userToAdd);
                                activeUser.Friends.Add(userToAdd);
                                userToAdd.Friends.Add(activeUser);
                                StartQueue();
                                SendToQueue("Los usuarios: " + activeUser.UserName + " y " + userToAdd.UserName + " ahora son amigos");
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

        public static string CheckNameIsAFriend(string data)
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
                    if (activeUser.Friends.Contains(userToCheck))
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
                    //Console.WriteLine($"{activeUser.UserName} acepto la solicitud de {userToAccept.UserName}");
                    //agregar enviar logs

                    return $"Ahora tu y {userToAccept.UserName} son amigos.";
                }
                else
                {
                    activeUser.PendingFriends.Remove(userToAccept);
                    //Console.WriteLine($"{activeUser.UserName} rechazo la solicitud de {userToAccept.UserName}");
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
                StartQueue();
                SendToQueue("El usuario: " + userName + "se ha desconectado");
            }
        }

        public static void StartChat(string UserNames, TcpClient clientSocket)
        {
            string[] splitedData = UserNames.Split('%');
            User activeUser = GetUser(splitedData[1]);
            User userToChat = GetUser(splitedData[0]);
            StartQueue();
            SendToQueue("El usuario: " + activeUser.UserName + " ha iniciado un chat con: " + userToChat.UserName);
            activeUser.ChatingWith = splitedData[0];


            Chat newChat = new Chat
            {
                User1 = activeUser,
                User2 = userToChat,
                Messages = new List<Domain.Message>(),
                Files = new List<SentFile>()
            };
            Chat currentChat = GetChat(newChat);

            GetChatMessages(currentChat, clientSocket);
            GetPendingFiles(currentChat, clientSocket, activeUser, true);

            bool chating = true;
            while (chating)
            {
                userToChat = GetUser(splitedData[0]);

                string messageRecieved = protocol.RecieveData(clientSocket);

                if (MessageIsCommand(messageRecieved))
                {
                    ExecuteCommand(messageRecieved, activeUser, userToChat, clientSocket, currentChat, ref chating);
                }
                else
                {
                    string messageToSend = $"{activeUser.UserName} dice: {messageRecieved}";

                    if (userToChat.ChatingWith.Equals(activeUser.UserName))
                    {
                        protocol.SendData(messageToSend, (TcpClient)clientsList[userToChat.UserName]);
                    }
                    currentChat.addMessage(messageRecieved, activeUser);
                }
            }
        }

        private static void GetPendingFiles(Chat currentChat, TcpClient clientSocket, User activeUser, bool loadingChat)
        {
            List<SentFile> toSend = new List<SentFile>();
            bool pendingFiles = false;
            protocol.SendData("/4", clientSocket);

            foreach (var sentFile in currentChat.Files)
            {
                if (!sentFile.User.UserName.Equals(activeUser.UserName))
                {
                    pendingFiles = true;
                    toSend.Add(sentFile);
                    
                }
            }
            int fileAmount = toSend.Count;
            protocol.SendData(fileAmount.ToString(), clientSocket);

            foreach (var file in toSend)
            {
                OfferFile(file, clientSocket, currentChat);
            }

            if (!pendingFiles && !loadingChat)
            {
                protocol.SendData("/4.2", clientSocket);
            }
            foreach (var file in toSend)
            {
                DeleteServerFile(file.FileServerPath, currentChat);
            }

        }

        private static void OfferFile(SentFile fileToSend, TcpClient clientSocket, Chat currentChat)
        {
            bool waitingAnswer = true;
            while (waitingAnswer)
            {
                var userResponse = protocol.RecieveData(clientSocket);
                if (userResponse.Equals("si"))
                {
                    StartQueue();
                    SendToQueue("Un usuario acepto un archivo");
                    waitingAnswer = false;
                    protocol.SendData("1", clientSocket);
                    FileStream file = new FileStream(fileToSend.FileServerPath, FileMode.Open, FileAccess.Read);
                    int noOfPackets = protocol.CalculateNoOfPackets(file);

                    protocol.SendData("" + noOfPackets, clientSocket);
                    protocol.SendData(fileToSend.FileName, clientSocket);
                    protocol.SendFile(file, clientSocket);

                }
                else
                {
                    if (userResponse.Equals("no"))
                    {
                        waitingAnswer = false;
                        protocol.SendData("2", clientSocket);
                        StartQueue();
                        SendToQueue("Un usuario rechazo un archivo");
                    }
                    else
                    {
                        protocol.SendData("3", clientSocket);
                    }
                }
            }
        }

        private static void DeleteServerFile(string path, Chat currentChat)
        {
            SentFile file = GetFile(path, currentChat);
            currentChat.Files.Remove(file);
        }

        private static SentFile GetFile(string path, Chat currentChat)
        {
            foreach (var file in currentChat.Files)
            {
                if (file.FileServerPath.Equals(path))
                {
                    return file;
                }
            }
            return null;
        }

        private static void ExecuteCommand(string messageRecieved, User activeUser, User userToChat, TcpClient clientSocket, Chat currentChat, ref bool chating)
        {
            if (messageRecieved.Contains(" "))
            {
                var splited = messageRecieved.Split(' ');
                if (splited[0].Equals("/send"))
                {
                    string filePath = GetFilePath(messageRecieved);

                    protocol.SendData("Enviando el archivo...", clientSocket);
                    protocol.SendData("/2", clientSocket);

                    RecieveFile(messageRecieved, activeUser, userToChat, clientSocket, currentChat);

                    protocol.SendData("/3", clientSocket);
                    protocol.SendData("Archivo enviado...", clientSocket);
                    StartQueue();
                    SendToQueue("El usuario: " + activeUser.UserName + " envio un archivo al usuario: " + userToChat.UserName);

                }
                else
                {
                    protocol.SendData("Comando invalido", clientSocket);
                }
            }
            else
            {
                switch (messageRecieved)
                {
                    case "/exit":
                        chating = false;
                        activeUser.ChatingWith = "NO USER";
                        string responseMessage = "/1";
                        protocol.SendData(responseMessage, clientSocket);
                        StartQueue();
                        SendToQueue("El usuario: " + activeUser.UserName + " ha salido de su chat abierto");
                        break;
                    case "/files":
                        GetPendingFiles(currentChat, clientSocket, activeUser, false);
                        break;
                    default:
                        protocol.SendData("Comando invalido", clientSocket);
                        break;

                }
            }
        }

        private static void RecieveFile(string messageRecieved, User activeUser, User userToChat, TcpClient clientSocket, Chat currentChat)
        {
            string filePath = GetFilePath(messageRecieved);

            SentFile newFile = new SentFile()
            {
                FileName = GetFileName(filePath),
                FileServerPath = ConfigurationManager.AppSettings["saveFileRoute"] + "serverFile" + GetFileName(filePath),
                User = activeUser
            };


            if (newFile.FileServerPath != string.Empty)
            {
                string noOfPackets = protocol.RecieveData(clientSocket);
                protocol.RecieveFile(clientSocket, newFile.FileServerPath, Int32.Parse(noOfPackets));
            }

                currentChat.Files.Add(newFile);


        }

        private static string GetFileName(string filePath)
        {
            var splited = filePath.Split('\\');
            return splited[splited.Length - 1];
        }

        private static string GetFilePath(string message)
        {
            var splited = message.Split(' ');
            string finalPath = splited[1];
            try
            {
                for (int i = 2; i < splited.Length; i++)
                {
                    finalPath += " " + splited[i];
                }
                return finalPath;
            }
            catch (Exception)
            {
                return finalPath;
            }

        }

        private static bool MessageIsCommand(string messageRecieved)
        {
            if (messageRecieved[0] == '/')
            {
                return true;
            }
            return false;
        }

        private static Chat GetChat(Chat newChat)
        {
            foreach (Chat aChat in chats)
            {
                if (newChat.Equals(aChat))
                {
                    return aChat;
                }
            }
            chats.Add(newChat);
            return newChat;
        }
        public static void GetChatMessages(Chat aChat, TcpClient clientSocket)
        {
            string messagesToShow = "";
            foreach (Domain.Message message in aChat.Messages)
            {
                messagesToShow = (messagesToShow + "#" + message.User.UserName + " dice: " + message.Line);
            }

            protocol.SendData(messagesToShow, clientSocket);

        }


        #region Remoting
        public override List<User> GetUsers()
        {
            return registeredUsers;
        }

        public override bool RegisterUser(string name, string pass)
        {
            User userExists = GetUser(name);
            if (userExists == null) 
            {
                User newUser = new User();
                newUser.UserName = name;
                newUser.Password = pass;
                lock (userLocker)
                {
                    registeredUsers.Add(newUser);

                }
                StartQueue();
                SendToQueue("Un administrador registro con exito al usuario: " + name);
                return true;
            }
            return false;
        }

        public override bool DeleteUser(string name)
        {
            User user = GetUser(name);
            if(user != null)
            {
                if (!user.Connected)
                {
                    lock (userLocker)
                    {
                        registeredUsers.Remove(user);
                    }
                    StartQueue();
                    SendToQueue("Un administrador elimino con exito al usuario: " + name);
                    return true;
                }
            }
            return false;
        }

        public override bool ModifyUser(string name, string newName, string newPass)
        {
            User user = GetUser(name);
            if (user != null)
            {
                if (!user.Connected)
                {
                    lock (userLocker)
                    {
                        user.UserName = newName;
                        user.Password = newPass;
                    }
                    StartQueue();
                    SendToQueue("Un administrador modifico con exito al usuario: " + name + " (actual " + newName + ")");
                    return true;
                }
            }
            return false;
        }
        #endregion
        #region MessageQueuing
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
        #endregion
    }
}
