﻿using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cliente
{
    public class Chat
    {
        public List<Message> Messages { get; set; }
        public User User1 { get; set; }
        public User User2 { get; set; }

        public ValidReturn addMessage(string line, User sender)
        {
            ValidReturn added = new ValidReturn();
            Message newMessage = new Message(line, sender);
            added = newMessage.isLineValid();
            if (added.Valid)
            {
                Messages.Add(newMessage);
            }
            return added;
        }
    }
}
