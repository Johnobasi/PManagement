using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermissionManagement.Model
{
    public class TransactionStatus
    {
        public string ProcessStatus { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentReference { get; set; }
        public DateTime TimeRetrieved { get; set; }
        public DateTime? TimeProcessed { get; set; }
        public int DateRetrieved { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public string UserMessage { get; set; }
        public string RequestReference { get; set; }
        public DateTime? IMTOUpdateTime { get; set; }
        public string IsIMTOUpdated { get; set; }
        public int IMTOUpdateTrialCount { get; set; }
        public int PostTrialCount { get; set; }
        public string IMTOReason { get; set; }

    }
}
