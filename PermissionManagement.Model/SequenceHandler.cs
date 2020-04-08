using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PermissionManagement.Model
{
    public class SequenceHandler
    {
        public string SequenceName { get; set; }
        public string SequencePrefix { get; set; }
        public int SequenceLength { get; set; }
        public int CurrentSequenceNo { get; set; }
    }
}
