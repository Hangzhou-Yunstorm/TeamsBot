using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace Stanley.Fileinfos.ExportTool
{
    public class Options
    {
        [Option("rootPath", HelpText = "导出本地文件夹下所有的文件信息", Required = true)]
        public string RootPath { get; set; }

        [Option('o', "origin", HelpText = "远程访问路径源地址", Required = true)]
        public string Origin { get; set; }
    }
}
