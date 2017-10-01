using System;
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

        public string ReceiveData(Socket socket)
        {
            byte[] dataLength = new byte[4];
            socket.Receive(dataLength);

            int length = BitConverter.ToInt32(dataLength, 0);
            byte[] bytesMessage = new byte[length];
            socket.Receive(bytesMessage);

            string message = Encoding.ASCII.GetString(bytesMessage);

            return message;
        }

        public void SendData(string message, Socket socket)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);

            int length = data.Length;
            var lengthBinary = BitConverter.GetBytes(length);

            var toSend = new byte[data.Length + lengthBinary.Length];

            lengthBinary.CopyTo(toSend, 0);
            data.CopyTo(toSend, lengthBinary.Length);

            int sent = 0;
            while (sent < toSend.Length)
            {
                sent += socket.Send(toSend, sent, toSend.Length - sent, SocketFlags.None);
            }
        }
    }
}
