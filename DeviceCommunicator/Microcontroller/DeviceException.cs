using System;

namespace Communication.Microcontroller
{
    [Serializable]
    internal class DeviceException : Exception
    {
        public DeviceErrorEnum? Error { get; set; }

        public DeviceException()
        {
        }

        public DeviceException(string message) : base(message)
        {
        }

        public DeviceException(string message, DeviceErrorEnum? error = null) : base(message)
        {
            this.Error = error;
        }

        public DeviceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}