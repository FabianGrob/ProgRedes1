using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class SentFile
    {
        public string FileServerPath { get; set; }
        public string FileName { get; set; }
        public User User { get; set; }

        public SentFile()
        {
            FileServerPath = "";
            FileName = "file";
            User = null;
        }
    }
}
