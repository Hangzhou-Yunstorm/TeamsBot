using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Stanley.Fileinfos.ExportTool
{
    public class ExportFileInfo
    {
        public string id { get; set; }
        public string name { get; set; }
        public string fullname { get; set; }
        public string extension { get; set; }
        public long size { get; set; }
    }
}
