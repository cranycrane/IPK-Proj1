﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPK_Proj1
{
    class ArgParser
    {
        public CommandLineSettings? Parse(string[] args)
        {
            var settings = new CommandLineSettings();

            if (args.Length < 2)
            {
                Console.Error.WriteLine("ERR: Unexpected number of parameters");
                System.Environment.Exit(1);
            }

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-t":
                        if (i + 1 < args.Length && (args[i + 1] == "tcp" || args[i + 1] == "udp"))
                        {
                            settings.Protocol = args[++i];
                        }
                        else
                        {
                            Console.Error.WriteLine("ERR: Invalid value for protocol. Use 'tcp' or 'udp'");
                            System.Environment.Exit(1);
                        }
                        break;
                    case "-s":
                        if (i + 1 < args.Length)
                        {
                            settings.ServerIP = args[++i];
                        }
                        else
                        {
                            Console.Error.WriteLine("ERR: No IP address or hostname specified");
                            System.Environment.Exit(1);
                        }
                        break;
                    case "-p":
                        if (i + 1 < args.Length && ushort.TryParse(args[i + 1], out ushort parsedPort))
                        {
                            settings.Port = parsedPort;
                            i++;
                        }
                        else
                        {
                            Console.Error.WriteLine("ERR: Invalid or missing value for port");
                            System.Environment.Exit(1);
                        }
                        break;
                    case "-d":
                        if (i + 1 < args.Length && ushort.TryParse(args[i + 1], out ushort parsedTimeout))
                        {
                            settings.Timeout = parsedTimeout;
                            i++;
                        }
                        else
                        {
                            Console.Error.WriteLine("ERR: Invalid or missing value for timeout");
                            System.Environment.Exit(1);
                        }
                        break;
                    case "-r":
                        if (i + 1 < args.Length && byte.TryParse(args[i + 1], out byte parsedRetries))
                        {
                            settings.Retries = parsedRetries;
                            i++;
                        }
                        else
                        {
                            Console.Error.WriteLine("ERR: Invalid or missing value for retries");
                            System.Environment.Exit(1);
                        }
                        break;
                    case "-h":
                        settings.ShowHelp = true;
                        break;
                    case "--debug":
                        settings.IsDebugEnabled = true;
                        break;
                    default:
                        Console.Error.WriteLine($"ERR: Unknown argument: {args[i]}");
                        System.Environment.Exit(1);
                        break;
                }
            }

            return settings;
        }
    }
}
