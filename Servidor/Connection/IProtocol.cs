using System;
using System.Collections.Generic;
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
    }
}
