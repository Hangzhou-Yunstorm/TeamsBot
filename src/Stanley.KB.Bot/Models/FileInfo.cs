using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stanley.KB.Bot.Models
{
    public class FileInfo
    {
        public string id { get; set; }
        public string fullname { get; set; }
        public string name { get; set; }
        public string extension { get; set; }
        public long size { get; set; }
    }
}
