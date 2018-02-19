using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FTPStandard
{
    public class FTPSendFile
    {

        static string destination = @"/home/athilann/public_html/devices/device1/data";
        static string host = "ftp.athilann.5gbfree.com";
        static string username = "athilann";
        static string password = "dYHLa%xve#Koq2";
        static int port = 22;

        public static void SendDataFileAsync(string dataFileName)
        {
            UploadSFTPFile(host, username, password, dataFileName, destination, port);
        }


        private static void UploadSFTPFile(string host, string username, string password, string sourcefile, string destinationpath, int port)
        {
            using (SftpClient client = new SftpClient(host, port, username, password))
            {
                client.Connect();
                client.ChangeDirectory(destinationpath);
                Debug.WriteLine("----------conectado ao ftp---------");
                using (FileStream fs = new FileStream(sourcefile, FileMode.Open))
                {
                    Debug.WriteLine("----------uppando arquivo---------");
                    client.BufferSize = 4 * 1024;
                    client.UploadFile(fs, Path.GetFileName(sourcefile));
                    Debug.WriteLine("---------- arquivo upado---------");
                }

            }
        }
    }
}
