using System;
using System.IO;
using System.Runtime.InteropServices;
using System.DirectoryServices.ActiveDirectory;

namespace CredCheck
{
    class Program
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int LogonUserA(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref int phToken);

        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();

        public static int LOGON32_LOGON_NETWORK = 3;
        public static int LOGON32_PROVIDER_DEFAULT = 0;

        public enum ErrorCodes
        {
            LOGON_FAILED = 1326
        }

        public static void Help()
        {
            Console.WriteLine("Usage CredCheck.exe <username>/[-h] <password> <domain> [other option(s)]\n" +
                "Small utility to verify harvested credentials using LogonUserA\n" +
                "-h             display this menu\n" +
                "username       username to authenticate as\n" +
                "password       password to authenticating user\n" +
                "domain         domain to authenticate to\n" +
                "other options:\n" +
                "fileOutput     file to save output to\n"
                );
        }

        static void Main(string[] args)
        {
            try
            {

                Domain.GetComputerDomain();

                string fileSavePath = null;

                if (args[0] == "-h") { Help(); return; }
                if (args.Length < 3) { throw new Exception("Too Few Arguements, use CredCheck.exe -h to view help menu"); }
                if (args.Length == 4) { fileSavePath = args[3]; }

                string username = args[0];
                string password = args[1];
                string domain = args[2];

                int _out = 0;
                _ = LogonUserA(username, domain, password, LOGON32_LOGON_NETWORK, LOGON32_PROVIDER_DEFAULT, ref _out);

                if (!string.IsNullOrEmpty(fileSavePath))
                {
                    if (_out != 0)
                    {
                        File.AppendAllText(fileSavePath, $"Username: {username}\nPassword: {password}\nDomain: {domain}\nValid Crendentials");
                    }
                    else
                    {
                        File.AppendAllText(fileSavePath, $"Username: {username}\nPassword: {password}\nDomain: {domain}\n" +
                            $"Error: {((ErrorCodes)GetLastError())}");
                    }
                    Console.WriteLine($"Output written to {fileSavePath}");
                }
                else {
                    if (_out != 0) { Console.WriteLine("Valid Crendentials"); } else { Console.WriteLine($"Error: {((ErrorCodes)GetLastError())}"); }
                }
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
        }
    }
}
