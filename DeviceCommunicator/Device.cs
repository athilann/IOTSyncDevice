using Communication.Microcontroller.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communication
{
    public abstract class Device : PropertyChangedBase, CommunicationDevice
    {

        protected const byte INITIALCOMMAND = 0xEE;

        protected const byte INITIALCOMMANDCATACASH = 0x43;

        public event DataEventHandler OnDataReceived;
        protected void OnDataReceivedOccuredEvent(object sender, DataEventArgs data, Exception ex = null)
        {
            if (OnDataReceived != null)
                OnDataReceived(sender, data);
        }
        public event ErroEventHandler OnErroReceived;

        DeviceErrorEnum? _lastEvent = null;


        protected void OnExceptionOccuredEvent(object sender, ErroEventArgs erro, Exception ex = null)
        {
            if (OnErroReceived != null && _lastEvent != erro.GetError())
            {
                _lastEvent = erro.GetError();
                OnErroReceived(sender, erro);
            }
        }

        public RelayCommand CancelCommand { get; private set; }
        public RelayCommand DisconnectCommand { get; private set; }

        private ConnectionState _State;
        public ConnectionState State { get { return _State; } set { _State = value; OnPropertyChanged(); } }

        protected Device()
        {
           CancelCommand = new RelayCommand(AbortConnection);
           DisconnectCommand = new RelayCommand(Disconnect);
        }

        public abstract Task ConnectToServiceAsync();

        public abstract void AbortConnection();

        public abstract void Disconnect();

        public abstract Task<uint> SendCommandAsync(RequestCommandsCode command);
        internal abstract void ResetDevice();
    }
    public enum ConnectionState
    {
        Disconnected,
        Connected,
        Enumerating,
        Connecting
    }

}
