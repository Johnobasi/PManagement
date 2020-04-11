using Newtonsoft.Json;
using Remitly.ProcessingManager.RemitlyCommon;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PermissionManagement.Controllers
{
    public class RemitlyPayoutController : Controller
    {

        // GET: Remitly
        [HttpGet]
        public ActionResult GetTransfer()
        {
            return View();
        }


        [HttpPost]
        public async Task<ActionResult> GetTransferAsync(string ReferenceNumber)
        {
            RemitlyRemittance remittance = new RemitlyRemittance();
            using (var httpClient = new HttpClient())
            {
                string url = ConfigurationManager.AppSettings["RemitlyBaseURL"];
                using (var response = await httpClient.GetAsync("/IMTOAPI.Project/Remitly/GetTransfer/ " + ReferenceNumber))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    remittance = JsonConvert.DeserializeObject<RemitlyRemittance>(apiResponse);
                }
            }

            return View();
        }

        [HttpGet]
        public async Task<ActionResult> UpdateTransfer(int Id)
        {
            UpdateTransferResponse updateTransfer = new UpdateTransferResponse();
            using (var httpClient = new HttpClient())
            {
                string url = ConfigurationManager.AppSettings["RemitlyBaseURL"];
                using (var response = await httpClient.GetAsync($"/IMTOAPI.Project/Remitly/UpdateTransfer" + Id))
                {
                    string UpdateResponse = await response.Content.ReadAsStringAsync();
                    updateTransfer = JsonConvert.DeserializeObject<UpdateTransferResponse>(UpdateResponse);
                }
            }
            return View(updateTransfer);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateTransfer(UpdateTransferResponse update)
        {

            UpdateTransferResponse NewUpdate = new UpdateTransferResponse();
            using (var httpClient = new HttpClient())
            {
                string url = ConfigurationManager.AppSettings["RemitlyBaseURL"];

                var content = new MultipartFormDataContent();
                content.Add(new StringContent(update.Reference_Number), "Reference Number");
                content.Add(new StringContent(update.Created_On), "Created On");
                content.Add(new StringContent(update.Payer_Codes.ToString()), "Payer Code");
                content.Add(new StringContent(update.Type), "Type");
                content.Add(new StringContent(update.State), "State");

                using (var response = await httpClient.PutAsync($"/IMTOAPI.Project/Remitly/UpdateTransfer/", content))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    ViewBag.Result = "Success";
                    var receivedUpdate = JsonConvert.DeserializeObject<UpdateTransferResponse>(apiResponse);
                }
            }

            return View(NewUpdate);
            
        }
    }
}