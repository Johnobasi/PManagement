namespace Remitly.ProcessingManager.RemitlyCommon
{
    public class RemitlyRemittanceResponse
    {
        public RemitlyRemittance RemitlyRemittance { get; set; }

        public string ResponseCode { get; set; }

        public string ResponseMessage { get; set; }
    }
}
