using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communication.Microcontroller.Command
{
    public enum ResponseCommandsCode
    {
        TOVIVO
    }
    public static class ResponseCommandsCodeExtensions
    {
        public static byte[] GetCode(this ResponseCommandsCode code)
        {
            if (code == ResponseCommandsCode.TOVIVO)
            {
                return new byte[] { 0xEE, 0X01, 0x03, 0X07, 0x01, 0x01, 0xEB };
            }

            return new byte[] { };
        }
    }
}
