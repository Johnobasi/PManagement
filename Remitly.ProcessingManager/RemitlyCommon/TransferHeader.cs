using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remitly.ProcessingManager.RemitlyCommon
{
    public class TransferHeader
    {
        public string Reference_Number { get; set; }
        public string Created_On { get; set; }
        public string State { get; set; }
        public string[] Payer_Codes { get; set; }
        public string Type { get; set; }
    }
}
