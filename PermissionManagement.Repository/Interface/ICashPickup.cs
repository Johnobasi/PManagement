using PermissionManagement.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PermissionManagement.Repository.Interface
{
    public interface ICashPickup
    {
        Task<RemitlyListResponse> ListCashPickup();
        Task<RemittanceCashPickup> RetrieveReference(string referenceNumber);

        Task<RemitlyEditTransfer> EditRemittance(string referenceNumber);
    }
}
