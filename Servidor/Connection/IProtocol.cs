using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Connection
{
    public interface IProtocol
    {
        string RecieveData(TcpClient socket);
        void SendData(string message, TcpClient socket);
        void SendFile(FileStream file, TcpClient socket);
        void RecieveFile(TcpClient socket, string fileName, int noOfPackets);
        int CalculateNoOfPackets(FileStream file);
    }
}
