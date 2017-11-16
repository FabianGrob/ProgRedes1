using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Domain;

namespace WCFLibrary
{
    public class Service : IService
    {
        public string DeleteUser(string name)
        {
            throw new NotImplementedException();
        }

        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

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

        public List<User> GetUsers()
        {
            throw new NotImplementedException();
        }

        public string ModifyUser(string name, string newName, string newPass)
        {
            throw new NotImplementedException();
        }

        public string RegisterUser(string name, string pass)
        {
            throw new NotImplementedException();
        }
    }
}
