
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class Chat
    {
        public List<SentFile> Files { get; set; }
        public List<Message> Messages { get; set; }
        public User User1 { get; set; }
        public User User2 { get; set; }

        public void addMessage(string line, User sender)
        {
            Message newMessage = new Message()
            {
                User = sender,
                Line = line
            };
            Messages.Add(newMessage);
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
                return (((User1.Equals(((Chat)obj).User1)) && (User2.Equals(((Chat)obj).User2))) || ((User1.Equals(((Chat)obj).User2)) && (User2.Equals(((Chat)obj).User1))));
            }
        }
    }
}
