using Database.Model;
using Dropbox.Api;
using Dropbox.Api.Files;
using FileSync;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace IOTSyncDevice
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private DispatcherTimer dispatcherTimer;

        public MainPage()
        {
            this.InitializeComponent();
        }


        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {

            try
            {
                var task = App.DataBaseAccess.GetRegistersAsync();
                task.Wait();
                List<Registre> result = task.Result;
                SyncRegistersAsync(result).Wait();
                App.DataBaseAccess.RemoveRegisters(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro: " + ex.Message);
            }

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 1, 0);
            //dispatcherTimer.Start();
            App.DeviceManager.ConnectToServiceAsync();
        }

        private void DispatcherTimer_Tick(object sender, object e)
        {
            //try
            //{
            //    var task = App.DataBaseAccess.GetRegistersAsync();
            //    task.Wait();
            //    List<Registre> result = task.Result;
            //    SyncRegistersAsync(result).Wait();
            //    App.DataBaseAccess.RemoveRegisters(result);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Erro: " + ex.Message);
            //}
        }

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
                   // Debug.WriteLine("{0} - {1}", full.Name.DisplayName, full.Email);
                    await Upload(dbx, "/devices/device1/data", filename, "asfdasdfasdf");
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
