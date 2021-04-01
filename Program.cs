using System.Collections.Generic;
using System.Management;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace EnumerateShares
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Wrong Number of Arguments!");
                Console.WriteLine("Usage: netshare.exe <hostname> <output-file>");
                System.Environment.Exit(0);
            }
            Dictionary<string,string> shares = GetNetworkShareDetailUsingWMI(args[0]);
            foreach (KeyValuePair<string,string> share in shares)
            {
                string[] lines = { share.Key + ": " + share.Value, "-----------"};
                try
                {
                    using (StreamWriter outputFile = new StreamWriter(args[1], true))
                    {
                        foreach (string line in lines)
                            outputFile.WriteLine(line);
                    }
                }
                catch (DirectoryNotFoundException)
                {
                    Console.WriteLine("Directory Not Found");
                    System.Environment.Exit(0);
                }
            }
        }
 
        public static Dictionary<string, string> GetNetworkShareDetailUsingWMI(string serverName)
        {
            Dictionary<string, string> shares = new Dictionary<string, string>();
 
            // do not use ConnectionOptions to get shares from local machine
            ConnectionOptions connectionOptions = new ConnectionOptions();
            //connectionOptions.Username = @"DomainAdministrator";
            //connectionOptions.Password = "password";
            //connectionOptions.Impersonation = ImpersonationLevel.Impersonate;
           
            ManagementScope scope = new ManagementScope("\\\\" + serverName + "\\root\\cimv2",
                                                            connectionOptions);
            try
            {
                scope.Connect();
            }
            catch (COMException)
            {
                Console.WriteLine("Invalid Server Name or Server Unavailable");
                System.Environment.Exit(0);
            }
            try
            { 
                ManagementObjectSearcher worker = new ManagementObjectSearcher(scope,
                                        new ObjectQuery("select Name,Path from win32_share"));
                foreach (ManagementObject share in worker.Get())
                {
                    shares.Add(share["Name"].ToString(), share["Path"].ToString());
                }
            }
            catch (COMException)
            {
                Console.WriteLine("Invalid Server Name or Server Unavailable");
                System.Environment.Exit(0);
            }
            return shares;
        }
    }
}
