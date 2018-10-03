using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace WIFIPasswordLeak
{
    class Program
    {
        static void Main(string[] args)
        {
            List<String> networks = EnumNetworks();
            Dictionary<String, String> passKeys = CorrelatePasskeys(networks);
            foreach (KeyValuePair<String, String> kv in passKeys)
            {
                Console.WriteLine("Network: " + kv.Key + " ; Passkey: " + kv.Value);
            }
            Console.ReadKey();
        }
        
        private static Dictionary<String, String> CorrelatePasskeys(List<String> names)
        {
            Dictionary<String, String> ntop = new Dictionary<String, String>();
            foreach (String name in names)
            {
                Process passWifi = new Process();
                passWifi.StartInfo.CreateNoWindow = true;
                passWifi.StartInfo.FileName = "netsh.exe";
                passWifi.StartInfo.Arguments = "wlan show profile name=\"" + name + "\" key=clear";
                passWifi.StartInfo.UseShellExecute = false;
                passWifi.StartInfo.RedirectStandardError = true;
                passWifi.StartInfo.RedirectStandardInput = true;
                passWifi.StartInfo.RedirectStandardOutput = true;
                passWifi.Start();

                String output = passWifi.StandardOutput.ReadToEnd();

                passWifi.WaitForExit();

                String[] lines = output.Split('\n');
                foreach (String line in lines)
                {
                    String curLine = line.Trim();
                    if (curLine.Contains("Key Content"))
                    {
                        ntop[name] = curLine.Split(":")[1].Trim();
                        ntop[name] = ntop[name] == "" ? null : ntop[name];
                    }
                }
            }
            return ntop;
        }

        private static List<String> EnumNetworks()
        {
            String args = "wlan show profile";
            Process enumWifi = new Process();
            enumWifi.StartInfo.CreateNoWindow = true;
            enumWifi.StartInfo.FileName = "netsh.exe";
            enumWifi.StartInfo.Arguments = args;
            enumWifi.StartInfo.UseShellExecute = false;
            enumWifi.StartInfo.RedirectStandardError = true;
            enumWifi.StartInfo.RedirectStandardInput = true;
            enumWifi.StartInfo.RedirectStandardOutput = true;
            enumWifi.Start();
            
            String output = enumWifi.StandardOutput.ReadToEnd();

            enumWifi.WaitForExit();

            String[] lines = output.Split('\n');
            List<String> names = new List<String>();
            foreach (String line in lines)
            {
                String curLine = line.Trim();
                if (curLine.Contains("All"))
                {
                    names.Add(curLine.Split(':')[1].Trim());
                }
            }
            
            return names;
        }
    }
}
