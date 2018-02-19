using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            UploadSFTPFile(host, username, password, source, destination, port);
        }

        string source = @"C:\HST_ATMInfra\logs\SafeCentreAPI.log";
        string destination = @"/home/athilann/public_html/devices/device1/data";
        string host = "ftp.athilann.5gbfree.com";
        string username = "athilann";
        string password = "dYHLa%xve#Koq2";
        int port = 22;

        public static void UploadSFTPFile(string host, string username, string password, string sourcefile, string destinationpath, int port)
        {
            using (SftpClient client = new SftpClient(host, port, username, password))
            {
                client.Connect();
                client.ChangeDirectory(destinationpath);
                using (FileStream fs = new FileStream(sourcefile, FileMode.Open))
                {
                    client.BufferSize = 4 * 1024;
                    client.UploadFile(fs, Path.GetFileName(sourcefile));
                }

            }
        }
    }
}
