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
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ConsoleTest
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
            InitGPIO();
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimer_TickAsync;
            dispatcherTimer.Interval = new TimeSpan(0, 10, 0);
            dispatcherTimer.Start();
            //App.DeviceManager.ConnectToServiceAsync();

        }
        private async void DispatcherTimer_TickAsync(object sender, object e)
        {
            try
            {
                string filename =  "data.csv";
                var task = App.DataBaseAccess.GetRegistersAsync();
                task.Wait();
                List<Registre> result = task.Result;

                Debug.WriteLine("----------Criando arquivo---------");
                string csv = CreateCSVFile.CreateFromRegistres(result);
                Debug.WriteLine(csv);

                Debug.WriteLine("----------Enviando arquivo: " + filename);
                using (var dbx = new DropboxClient("x"))
                {
                    var full = await dbx.Users.GetCurrentAccountAsync();
                    Debug.WriteLine("{0} - {1}", full.Name.DisplayName, full.Email);
                    await Upload(dbx, "/Lareso_Biodigestor_01", filename, csv);
                }
                //App.DataBaseAccess.RemoveRegisters(result);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erro: " + ex.Message);
            }
        }

        public async Task SyncRegistersAsync(List<Registre> registres)
        {
            try
            {


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


        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                return;
            }

            buttonPin = gpio.OpenPin(BUTTON_PIN);

            //if (buttonPin.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
            //    buttonPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
            //else
                buttonPin.SetDriveMode(GpioPinDriveMode.Input);

            pin = gpio.OpenPin(LED_PIN);
            pin.SetDriveMode(GpioPinDriveMode.Output);
            pin.Write(GpioPinValue.Low);

            buttonPin.DebounceTimeout = TimeSpan.FromMilliseconds(50);

            buttonPin.ValueChanged += buttonPin_ValueChanged;

        }

        private void buttonPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            if (e.Edge == GpioPinEdge.RisingEdge)
            {
                Debug.WriteLine("----------Adicionando registro---------");
                App.DataBaseAccess.AddRegister(new Registre() { RegistreDate = DateTime.Now });
                pin.Write(GpioPinValue.High);
            }
            else
            {
                pin.Write(GpioPinValue.Low);
            }

            
        }

        private const int BUTTON_PIN = 4;
        private GpioPin buttonPin;
        private GpioPin pin;
        private int LED_PIN = 17;
    }
}
