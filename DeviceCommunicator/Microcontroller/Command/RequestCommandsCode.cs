using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communication.Microcontroller.Command
{
    public enum RequestCommandsCode
    {
       
    }

    public static class RequestCommandsCodeExtensions
    {
        public static byte[] GetCode(this RequestCommandsCode code)
        {
            return new byte[] { };
        }
    }
}
