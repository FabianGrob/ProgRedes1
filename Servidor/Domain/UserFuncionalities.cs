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

        public void printFriends()
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
        public ValidReturn SendSolicitude(User anUser)
        {
            ValidReturn valid = new ValidReturn(true, "Solicitude sent");
            if (User.Friends.Contains(anUser))
            {
                valid.ReAssign(false, "You are already friends!");
            }
            if (User.PendingFriends.Contains(anUser))
            {
                AddFriend(anUser);
                User.PendingFriends.Remove(anUser);
                anUser.Friends.Add(User);
                valid.ReAssign(true, "You are are now friends!");
            }
            return valid;
        }
        public void Showsolicitudes()
        {
            Console.WriteLine("Your friend solicitudes: ");
            foreach (User possibleFriend in User.PendingFriends)
            {
                Console.WriteLine(possibleFriend.UserName + " has " + possibleFriend.Friends.Count + " friends ");
            }
        }
        public ValidReturn AcceptSolicitude(User anUser)
        {
            ValidReturn valid = new ValidReturn(true, "You are are now friends!");
            AddFriend(anUser);
            User.PendingFriends.Remove(anUser);
            return valid;
        }
        public void showUsersInServer()
        {
            throw new NotImplementedException();
        }
    }
}
