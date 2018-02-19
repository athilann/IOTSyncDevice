using Communication.Microcontroller.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communication
{
    public interface CommunicationDevice
    {
        event DataEventHandler OnDataReceived;
        event ErroEventHandler OnErroReceived;
        Task ConnectToServiceAsync();
        void AbortConnection();
        void Disconnect();
        Task<uint> SendCommandAsync(RequestCommandsCode command);
    }

    public delegate void DataEventHandler(object source, DataEventArgs e);
    public delegate void ErroEventHandler(object source, ErroEventArgs e);

    public class DataEventArgs : EventArgs
    {
        private byte[] EventCommand;
        public DataEventArgs(byte[] command)
        {
            EventCommand = command;
        }
        public byte[] GetCommand()
        {
            return EventCommand;
        }
    }

    public class ErroEventArgs : EventArgs
    {
        private DeviceErrorEnum error;
        public ErroEventArgs(DeviceErrorEnum erro)
        {
            error = erro;
        }
        public DeviceErrorEnum GetError()
        {
            return error;
        }
    }
}
