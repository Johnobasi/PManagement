﻿namespace PermissionManagement.Model
{
    public class RemitlyTransferHeader
    {
        public string Reference_Number { get; set; }
        public string Created_On { get; set; }
        public string State { get; set; }
        public string[] Payer_Codes { get; set; }
        public string Type { get; set; }
    }
}
