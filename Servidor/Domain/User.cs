using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class User
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool Connected { get; set; }
        public List<User> Friends { get; set; }
        public List<User> PendingFriends { get; set; }
        public User()
        {
            Id = new Guid();
            UserName = "defaultUser";
            Password = "defaultPassword";
            Connected = false;
        }


        public bool autenticate(string userName, string password)
        {
            bool valid = true;
            if (!password.Equals(this.Password))
            {
                valid = false;
            }
            else {
                Connected = true;
            }
            return valid;
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            else if (this.GetType() != obj.GetType())
            {
                return false;
            }
            else
            {
                return UserName.Equals(((User)obj).UserName);
            }
        }
        
    }
}
