using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class UserFuncionalities
    {
        public User User { get; set; }

        public void PrintFriends()
        {
            Console.WriteLine("Your friends: ");
            foreach (User friend in User.Friends)
            {
                Console.WriteLine(friend.UserName + " has " + friend.Friends.Count + " friends ");
            }
        }
        public void AddFriend(User anUser)
        {
            User.Friends.Add(anUser);
        }
        public void AddPending(User anUser)
        {
            User.PendingFriends.Add(anUser);
        }

        public void ShowRequests()
        {
            Console.WriteLine("Your friend solicitudes: ");
            foreach (User possibleFriend in User.PendingFriends)
            {
                Console.WriteLine(possibleFriend.UserName + " has " + possibleFriend.Friends.Count + " friends ");
            }
        }
        public ValidReturn AcceptRequest(User anUser)
        {
            ValidReturn valid = new ValidReturn(true, "You are are now friends!");
            AddFriend(anUser);
            User.PendingFriends.Remove(anUser);
            return valid;
        }
        public void ShowUsersInServer()
        {
            throw new NotImplementedException();
        }
    }
}
