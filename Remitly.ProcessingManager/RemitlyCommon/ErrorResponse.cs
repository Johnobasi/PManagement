using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMTO.Common.RemitlyCommon
{
    public class ErrorResponse
    {
        public string code { get; set; }
        public string message { get; set; }
        public string details { get; set; }
        public int MyProperty { get; set; }
        public int secondary { get; set; }
    }
}
