using Database.Model;
using Dropbox.Api;
using Dropbox.Api.Files;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace FileSync
{
    public class FileSyncAccess
    {

        public async Task SyncRegistersAsync(List<Registre> registres)
        {
            try
            {
                Debug.WriteLine("----------Criando arquivo---------");
                string csv = CreateCSVFile.CreateFromRegistres(registres);
                string filename = DateTime.Now.ToString("ddMMyyyyHHmm") + ".csv";
                Debug.WriteLine(csv);

                Debug.WriteLine("----------Enviando arquivo: " + filename);
                using (var dbx = new DropboxClient("dpugWbLLRQUAAAAAAAACL57r7ZkFLokva7oVxKBXhIcoKKYrBz1tcXXhFZhkHowp"))
                {
                    var full = await dbx.Users.GetCurrentAccountAsync();
                    Debug.WriteLine("{0} - {1}", full.Name.DisplayName, full.Email);
                    await Upload(dbx, "/devices/device1/data", filename, csv);
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        private async Task Upload(DropboxClient dbx, string folder, string file, string content)
        {
            using (var mem = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                var updated = await dbx.Files.UploadAsync(
                    folder + "/" + file,
                    WriteMode.Overwrite.Instance,
                    body: mem);
                Debug.WriteLine("Saved {0}/{1} rev {2}", folder, file, updated.Rev);
            }
        }

    }
}
