
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
