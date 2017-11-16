using Domain;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerProyect
{
    public abstract class RemotingShared : MarshalByRefObject
    {

        public static string RemotingName = "MyRemotingName";
        public static string ServerIpAddress = "localhost";
        public static int Port = 65432;

        public static string GetIpAddress()
        {
            ServerIpAddress = ConfigurationManager.AppSettings["serverIP"];
            return ServerIpAddress;
        }

        public abstract List<User> GetUsers();

        public abstract bool RegisterUser(string name, string pass);

        public abstract bool DeleteUser(string name);

        public abstract bool ModifyUser(string name, string newName, string newPass);

    }
}
