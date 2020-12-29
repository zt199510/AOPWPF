using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ServerTest
{
    public static class Config
    {
         static Config()
        {
            LoadVersion();
        }

        public static string VersionPath { get; set; } = @".\ClientTest.exe";

        public static byte[] ClientHash;
        public static void LoadVersion()
        {
            try
            {
                if (File.Exists(VersionPath))
                    using (FileStream stream = File.OpenRead(VersionPath))
                    using (MD5 md5 = MD5.Create())
                        ClientHash = md5.ComputeHash(stream);
                else ClientHash = null;
            }
            catch (Exception ex)
            {
               
            }
        }
    }
}
