using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cliente
{
    public class Message
    {
        public string Line { get; set; }
        public User User { get; set; }
        public Message(string line, User sender)
        {
            Line = line;
            User = sender;
        }
        public ValidReturn isLineValid()
        {
            ValidReturn valid = new ValidReturn();
            if (Line.Length == 0)
            {
                valid.ReAssign(false, "Empty Message");
            }
            return valid;
        }
    }
}
