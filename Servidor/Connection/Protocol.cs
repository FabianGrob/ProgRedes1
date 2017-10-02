﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Connection
{
    public class Protocol : IProtocol
    {
        public Protocol()
        {
        }

        public string ReceiveData(TcpClient socket)
        {
            NetworkStream stream = socket.GetStream();
            byte[] dataLength = new byte[10025];
            stream.Read(dataLength, 0, 4);
            int length = BitConverter.ToInt32(dataLength, 0);
            byte[] bytesMessage = new byte[length];
            stream.Read(bytesMessage, 0, length);

            string message = Encoding.ASCII.GetString(bytesMessage);

            while (message.Equals("$"))
            {
                message = ReceiveData(socket);
            }
            SendData("$", socket);
            return message;
            //byte[] dataLength = new byte[4];
            //stream.Read(dataLength, 0, dataLength.Length);
            //bool a = stream.DataAvailable;
            //int length = BitConverter.ToInt32(dataLength, 0);
            //byte[] bytesMessage = new byte[length];
            //stream.Read(bytesMessage, dataLength.Length, bytesMessage.Length + dataLength.Length);

            //string message = Encoding.ASCII.GetString(bytesMessage);

            //return message;

        }

        public void SendData(string message, TcpClient socket)
        {
            NetworkStream stream = socket.GetStream();

            byte[] data = Encoding.ASCII.GetBytes(message);

            int length = data.Length;
            var lengthBinary = BitConverter.GetBytes(length);

            byte[] toSend = new byte[data.Length + lengthBinary.Length];

            lengthBinary.CopyTo(toSend, 0);
            data.CopyTo(toSend, lengthBinary.Length);


            stream.Write(toSend, 0, toSend.Length);

        }
    }
}
