using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPK_Proj1
{
    public class CommandLineSettings
    {
        public string Protocol { get; set; } = "";
        public string ServerIP { get; set; } = "";
        public ushort Port { get; set; } = 4567; // Default value
        public ushort Timeout { get; set; } = 250; // Default value
        public byte Retries { get; set; } = 3; // Default value
        public bool ShowHelp { get; set; } = false;
        public bool IsDebugEnabled { get; set; } = false;
    }
}
