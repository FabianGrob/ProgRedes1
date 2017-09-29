using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
   public class ValidReturn
    {
        public bool Valid { get; set; }
        public string Reason { get; set; }
        public ValidReturn() {
            Valid = true;
            Reason = "OK";
        }
        public ValidReturn(bool isValid,string aReason)
        {
            Valid = isValid;
            Reason = aReason;
        }
        public void ReAssign(bool isValid, string aReason)
        {
            Valid = isValid;
            Reason = aReason;
        }
    }
}
