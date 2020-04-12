using System.Collections.Generic;

namespace PermissionManagement.Model
{
    public class RemitlyListResponse
    {
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public IList<RemitlyTransferHeader> TransferList { get; set; }
    }
}
