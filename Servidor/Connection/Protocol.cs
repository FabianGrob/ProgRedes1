using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Connection
{
    public class Protocol : IProtocol
    {
        private const int BufferSize = 10025;

        public Protocol()
        {
        }

        public string RecieveData(TcpClient socket)
        {
            NetworkStream stream = socket.GetStream();
            byte[] dataLength = new byte[BufferSize];
            stream.Read(dataLength, 0, 4);
            int length = BitConverter.ToInt32(dataLength, 0);
            byte[] bytesMessage = new byte[length];
            stream.Read(bytesMessage, 0, length);


            string message = Encoding.ASCII.GetString(bytesMessage);

            Array.Clear(dataLength, 0, BufferSize);
            Array.Clear(bytesMessage, 0, length);

            return message;

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

        public void SendFile(FileStream file, TcpClient socket)
        {
            NetworkStream stream = socket.GetStream();
            byte[] SendingBuffer = null;

            int NoOfPackets = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(file.Length) / Convert.ToDouble(BufferSize)));
            int TotalLength = (int)file.Length, CurrentPacketLength;
            for (int i = 0; i < NoOfPackets; i++)
            {
                if (TotalLength > BufferSize)
                {
                    CurrentPacketLength = BufferSize;
                    TotalLength = TotalLength - CurrentPacketLength;
                }
                else CurrentPacketLength = TotalLength;

                SendingBuffer = new byte[CurrentPacketLength];
                file.Read(SendingBuffer, 0, CurrentPacketLength);

                stream.Write(SendingBuffer, 0, (int)SendingBuffer.Length);

            }
        }

        public void RecieveFile(TcpClient socket, string filePath)
        {
            NetworkStream stream = socket.GetStream();
            byte[] data = new byte[BufferSize];
            int RecBytes;
            int totalrecbytes = 0;

            FileStream file = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            while ((RecBytes = stream.Read(data, 0, data.Length)) > 0)
            {
                file.Write(data, 0, RecBytes);
                totalrecbytes += RecBytes;
            }
            file.Close();
        }
    }
}
