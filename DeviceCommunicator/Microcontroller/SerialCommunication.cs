using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using System.Threading;
using Windows.UI.Xaml;
using Communication.Microcontroller.Command;

namespace Communication.Microcontroller
{
    public class SerialCommunication : Device
    {
        private string serialID = "";
        private SerialDevice serialDevice;
        private DataReader reader;
        private DataWriter writer;
        private DispatcherTimer _toVivoTimeout;
        private DispatcherTimer _reconectionTimer = null;
        private int _toVivoTimeoutTick;
        private int _connectionTries = 0;

        public CancellationTokenSource ReadCancellationTokenSource { get; private set; }

        public SerialCommunication()
        {

        }

        public override void AbortConnection()
        {
        }

        public override async Task ConnectToServiceAsync()
        {
            State = ConnectionState.Connecting;
            try
            {
                _connectionTries++;
                var selector = SerialDevice.GetDeviceSelector();
                var devices = await DeviceInformation.FindAllAsync(selector);
                if (devices.Any())
                {
                    serialDevice = await TryGetSerialDevice(devices, "USB");
                    //serialDevice = await TryGetSerialDevice(devices, "FT232R USB UART");

                    if (serialDevice == null)
                    {
                        //OnExceptionOccuredEvent(this, new CataCash.Communication.ErroEventArgs(DeviceErrorEnum.COMMUNICATION_PROBLEMS));
                        Console.WriteLine("Serial device does not exists.");
                        State = ConnectionState.Disconnected;
                        throw new DeviceException("Serial device does not exists.", DeviceErrorEnum.COMMUNICATION_PROBLEMS);
                    }

                    serialDevice.BaudRate = 115200;
                    serialDevice.DataBits = 8;
                    serialDevice.Parity = SerialParity.None;
                    serialDevice.StopBits = SerialStopBitCount.One;
                    serialDevice.IsRequestToSendEnabled = true;
                    await Task.Delay(10);
                    serialDevice.IsDataTerminalReadyEnabled = true;
                    await Task.Delay(10);
                    serialDevice.IsRequestToSendEnabled = false;
                    await Task.Delay(10);
                    serialDevice.IsDataTerminalReadyEnabled = false;

                    ReadCancellationTokenSource = new CancellationTokenSource();

                    writer = new DataWriter(serialDevice.OutputStream);
                    reader = new DataReader(serialDevice.InputStream);
                    Task listen = ListenForMessagesAsync();
                    //Task send = SendForMessagesAsync();
                    _reconectionTimer?.Stop();
                    _reconectionTimer = null;
                    _connectionTries = 0;
                    this.State = ConnectionState.Connected;

                    OnExceptionOccuredEvent(this, new Communication.ErroEventArgs(DeviceErrorEnum.OK));

                    Console.WriteLine("Successfully connected.");
                }
                else
                {
                    throw new DeviceException("There is no device.", DeviceErrorEnum.NO_DEVICE_FOUND);
                }
            }
            catch (TaskCanceledException)
            {
                State = ConnectionState.Disconnected;
            }
            catch (Exception ex)
            {
                State = ConnectionState.Disconnected;
                if (_toVivoTimeout != null)
                {
                    _toVivoTimeout.Stop();
                    _toVivoTimeout = null;
                }
                if (_reconectionTimer == null)
                {
                    _reconectionTimer = new DispatcherTimer();
                    _reconectionTimer.Interval = TimeSpan.FromSeconds(5);
                    _reconectionTimer.Tick += ReconectionTimer_Tick;
                    _reconectionTimer.Start();
                }

                if (_connectionTries > 1)
                    return;

                var deviceExceptions = ex as DeviceException;
                if (deviceExceptions != null)
                {
                    if (deviceExceptions.Error.HasValue)
                        OnExceptionOccuredEvent(this, new Communication.ErroEventArgs(deviceExceptions.Error.Value), ex);
                    Console.WriteLine(deviceExceptions.Message, ex);
                    return;
                }

                OnExceptionOccuredEvent(this, new Communication.ErroEventArgs(DeviceErrorEnum.COMMUNICATION_PROBLEMS), ex);
                Console.WriteLine("Communication error on connecting.", ex);

            }
        }

        private async Task<SerialDevice> TryGetSerialDevice(DeviceInformationCollection devices, string name)
        {
            SerialDevice device = null;
            try
            {
                var id = devices.FirstOrDefault(x => x.Name.Contains(name))?.Id;
                if (id != null)
                {
                    device = await SerialDevice.FromIdAsync(id);
                }
                else
                {
                    device = await SerialDevice.FromIdAsync(devices[0].Id);
                }
            }
            catch (Exception)
            {

            }
            return device;
        }

        private void ReconectionTimer_Tick(object sender, object e)
        {
            if (State == ConnectionState.Connecting || State == ConnectionState.Connected)
                return;

            ConnectToServiceAsync();
        }

        public override async void Disconnect()
        {
            if (reader != null)
            {
                reader = null;
            }
            if (writer != null)
            {
                writer.DetachStream();
                writer = null;
            }
            if (serialDevice != null)
            {
                serialDevice.Dispose();
                serialDevice = null;
            }
            this.State = ConnectionState.Disconnected;
            await Task.Delay(1000);
            if (_reconectionTimer == null)
            {
                _reconectionTimer = new DispatcherTimer();
                _reconectionTimer.Interval = TimeSpan.FromSeconds(5);
                _reconectionTimer.Tick += ReconectionTimer_Tick;
                _reconectionTimer.Start();
            }
        }

        private List<RequestCommandsCode> commandsList = new List<RequestCommandsCode>();


        public override async Task<uint> SendCommandAsync(RequestCommandsCode command)
        {
            uint sentMessageSize = 0;

            if (State != ConnectionState.Connected)
                return sentMessageSize;

            if (writer != null)
            {

                writer.WriteBytes(command.GetCode());
                await writer.StoreAsync();
                //if ((command != RequestCommandsCode.TOVIVO) && (command != RequestCommandsCode.CONFIRMA_COMMAND))
                //{
                //    Console.WriteLine("Data Sender: " + command + "  Código: " + BitConverter.ToString(command.GetCode()));
                //}

            }
            else
            {
                OnExceptionOccuredEvent(this, new Communication.ErroEventArgs(DeviceErrorEnum.COMMUNICATION_PROBLEMS), null);
                Console.WriteLine("Communication error on sending command: " + command);
            }
            return sentMessageSize;
        }

        private List<byte> globalBuffer = new List<byte>();



        private async Task ListenForMessagesAsync()
        {
            var ReadCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = ReadCancellationTokenSource.Token;
            _toVivoTimeout = new DispatcherTimer();
            _toVivoTimeout.Interval = new TimeSpan(0, 0, 2);
            _toVivoTimeout.Tick += ToVivoTimeout_Tick;
            // _toVivoTimeout.Start();

            while (reader != null)
            {

                try
                {
                    Task<UInt32> loadAsyncTask;

                    uint ReadBufferLength = 1;

                    cancellationToken.ThrowIfCancellationRequested();

                    reader.InputStreamOptions = InputStreamOptions.Partial;

                    loadAsyncTask = reader.LoadAsync(ReadBufferLength).AsTask(cancellationToken);

                    UInt32 bytesRead = await loadAsyncTask;

                    if (bytesRead > 0)
                    {
                        byte[] buffer = new byte[bytesRead];
                        reader.ReadBytes(buffer);
                        if (!globalBuffer.Any())
                        {

                            if (buffer[0] == INITIALCOMMAND || buffer[0] == INITIALCOMMANDCATACASH)
                                globalBuffer.Add(buffer[0]);
                            else
                                Console.WriteLine("Data Received trash: " + BitConverter.ToString(buffer));

                            continue;
                        }

                        globalBuffer.Add(buffer[0]);
                        if (globalBuffer.Count < 7)
                        {
                            if (globalBuffer.Count == 2)
                            {
                                if (ResponseCommandsCode.TOVIVO.GetCode().SequenceEqual(globalBuffer.ToArray()))
                                {
                                    Console.WriteLine("Data Received: " + BitConverter.ToString(globalBuffer.ToArray()) + "  Command:" + ResponseCommandsCode.TOVIVO);
                                    OnDataReceivedOccuredEvent(this, new Communication.DataEventArgs(globalBuffer.ToArray()));
                                    globalBuffer.Clear();
                                    continue;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                continue;
                            }

                        }
                        else
                        {
                            byte[] command = globalBuffer.ToArray();
                            byte checksum = (byte)(command[0] ^ command[1] ^ command[2] ^ command[3] ^ command[4] ^ command[5]);

                            if (checksum != command[6])
                            {
                                Console.WriteLine("Checksum problem: " + BitConverter.ToString(globalBuffer.ToArray()));
                                globalBuffer.Clear();
                                continue;
                            }

                        }


                        if (!ResponseCommandsCode.TOVIVO.GetCode().SequenceEqual(globalBuffer.ToArray()))
                        {
                            OnExceptionOccuredEvent(this, new Communication.ErroEventArgs(DeviceErrorEnum.UNKNOW_COMMAND_RECEIVED));
                            Console.WriteLine("Unknow command received: " + BitConverter.ToString(globalBuffer.ToArray()));
                            globalBuffer.Clear();
                            continue;
                        }


                        Console.WriteLine("Data Received: " + BitConverter.ToString(globalBuffer.ToArray()) + "  Command:" + ResponseCommandsCode.TOVIVO);
                        OnDataReceivedOccuredEvent(this, new Communication.DataEventArgs(globalBuffer.ToArray()));
                        globalBuffer.Clear();
                    }
                }
                catch (Exception ex)
                {
                    if (reader != null)
                    {
                        OnExceptionOccuredEvent(this, new Communication.ErroEventArgs(DeviceErrorEnum.COMMUNICATION_PROBLEMS), ex);
                        Console.WriteLine("Communication error on receiving message.", ex);
                        Disconnect();


                    }


                }
            }
        }

        private void ToVivoTimeout_Tick(object sender, object e)
        {
            _toVivoTimeoutTick++;
            if (_toVivoTimeoutTick <= 10)
            {
                // SendCommandAsync(RequestCommandsCode.TOVIVO);
            }
            else
            {
                OnExceptionOccuredEvent(this, new Communication.ErroEventArgs(DeviceErrorEnum.COMMUNICATION_PROBLEMS));
                Console.WriteLine("Communication error: TO_VIVO timeout");
                _toVivoTimeout.Stop();
                _toVivoTimeoutTick = 0;
                Disconnect();
            }
        }

        internal override async void ResetDevice()
        {
            if (State == ConnectionState.Connected)
            {
                // lockComunication = true;
                //serialDevice.IsRequestToSendEnabled = true;
                //await Task.Delay(10);
                //serialDevice.IsDataTerminalReadyEnabled = true;
                //await Task.Delay(10);
                //serialDevice.IsRequestToSendEnabled = false;
                //await Task.Delay(10);
                //serialDevice.IsDataTerminalReadyEnabled = false;
                //await Task.Delay(200);
                //App.SensorManager.resetStatusSensors();
                //SendCommandAsync(RequestCommandsCode.INICIAR_AUTO_DIAGNOSTICO);
                //await Task.Delay(5000);
                //  lockComunication = false;
            }
        }
    }
}
