using System.Collections.Generic;

namespace Remitly.ProcessingManager.RemitlyCommon
{
    public class ListTransferResponse
    {
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public IList<TransferHeader> TransferList { get; set; }
    }
}
