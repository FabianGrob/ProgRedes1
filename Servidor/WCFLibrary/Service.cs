using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Domain;
using ServerProyect;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;

namespace WCFLibrary
{
    public class Service : IService
    {
        private RemotingShared lShared;

        private void StablishConnection()
        {
            string lPath = "tcp://" + RemotingShared.ServerIpAddress + ":" + RemotingShared.Port + "/" + RemotingShared.RemotingName;
            TcpChannel lTcpChannel = new TcpChannel();

            if (!ChannelServices.RegisteredChannels.Any(lChannel => lChannel.ChannelName == lTcpChannel.ChannelName))
            {
                ChannelServices.RegisterChannel(lTcpChannel, true);
            }

            lShared = (RemotingShared)Activator.GetObject(typeof(RemotingShared), lPath);
        }

        public bool DeleteUser(string name)
        {
            StablishConnection();
            bool response = lShared.DeleteUser(name);
            return response;
        }

        public List<User> GetUsers()
        {
            StablishConnection();
            List<User> response = lShared.GetUsers();
            return response;
        }

        public bool ModifyUser(string name, string newName, string newPass)
        {
            StablishConnection();
            bool response = lShared.ModifyUser(name, newName, newPass);
            return response;
        }

        public bool RegisterUser(string name, string pass)
        {
            StablishConnection();
            bool response = lShared.RegisterUser(name, pass);
            return response;
        }




        //ya venian
        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }

        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }


    }
}
