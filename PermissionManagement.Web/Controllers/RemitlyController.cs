using IMTO.Common;
using IMTO.Common.RemitlyCommon;
using Newtonsoft.Json;
using Remitly.ProcessingManager.RemitlyCommon;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PermissionManagement.Controllers
{
    public class RemitlyController : Controller
    {

        // GET: Remitly
        [HttpGet]
        public ActionResult GetTransfer()
        {
            return View();
        }


        [HttpPost]
        public async Task<ActionResult> GetTransfer(RemitlyDataRetrievalDTO request)
        {
            string ip = Request.ServerVariables["REMOTE_ADDR"];
            var logger = new Logger("RemitlyDownloader", "GetTransfer", ip);
            var result = new RemitlyRemittanceResponse();

            try
            {
                logger.LogRequest(new string[] { request.RemittanceTrackingCode });

                using (var client = new HttpClient())
                {
                    //passing service service baseurl
                    string url = ConfigurationManager.AppSettings["RemitlyBaseURL"];
                    client.BaseAddress = new Uri(url);
                    var res = await client.GetAsync($"IMTOAPI.Project/Remitly/GetTransfer/{request.RemittanceTrackingCode}");

                    //Checking the response
                    if (res.IsSuccessStatusCode)
                    {
                        var TransRes = res.Content.ReadAsStringAsync().Result;
                        var resp = JsonConvert.DeserializeObject<RemitlyRemittance>(TransRes);
                        if (resp != null)
                        {
                            result.ResponseCode = "00";
                            result.ResponseMessage = "Operation successful";
                            result.RemitlyRemittance = resp;
                        }
                    }
                    else
                    {
                        var TransRes = res.Content.ReadAsStringAsync().Result;
                        var errorResult = JsonConvert.DeserializeObject<ErrorResponse>(TransRes);
                        if (errorResult != null)
                        {
                            result.ResponseCode = errorResult.code ?? "99";
                            var rsp = new List<string>();

                            if (!string.IsNullOrEmpty(errorResult.message)) rsp.Add(errorResult.message);
                            if (!string.IsNullOrEmpty(errorResult.details)) rsp.Add(errorResult.details);
                            if (rsp.Count > 0)
                                result.ResponseMessage = String.Join("|", rsp.ToArray());
                            else
                                result.ResponseMessage = "Unknown error message";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.ResponseCode = "99";

                string msg = ex.Message;
                if (ex.InnerException != null) msg = string.Format("{0} - {1}", msg, ex.InnerException.Message);
                result.ResponseMessage = string.Format("{0}|{1}", msg, ex.StackTrace);
                logger.LogError(ex);
            }
            logger.LogResponse(new string[] { result.ResponseCode, result.ResponseMessage });
            return View(result);
        }

        [HttpGet]
        public async Task<ActionResult> UpdateTransferAsync(int Id)
        {
            UpdateTransferResponse updateTransfer = new UpdateTransferResponse();
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync("{RemitlyBaseURL}/IMTOAPI.Project/Remitly/UpdateTransfer" + Id))
                {
                    string UpdateResponse = await response.Content.ReadAsStringAsync();
                    updateTransfer = JsonConvert.DeserializeObject<UpdateTransferResponse>(UpdateResponse);
                }
            }
            return View(updateTransfer);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateTransfer(RemitlyDataRetrievalDTO request)
        {
            string ip = Request.ServerVariables["REMOTE_ADDR"];
            var logger = new Logger("RemitlyDownloader", "UpdateTransfer", ip);
            var result = new UpdateTransferResponse();

            try
            {
                string actionMessage = Encoding.UTF8.GetString(Convert.FromBase64String(request.UpdateMessage));

                // Convert serialized update object to string content.
                var httpContent = new StringContent(actionMessage, Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    //passing service service baseurl
                    string url = ConfigurationManager.AppSettings["RemitlyBaseURL"];
                    client.BaseAddress = new Uri(url);
                    HttpResponseMessage res = await client.PostAsync($"IMTOAPI.Project/Remitly/UpdateTransfer/{request.RemittanceTrackingCode}", httpContent);

                    //Checking the response
                    if (res.IsSuccessStatusCode)
                    {
                        var TransRes = res.Content.ReadAsStringAsync().Result;
                        result = JsonConvert.DeserializeObject<UpdateTransferResponse>(TransRes);
                        if (result != null)
                        {
                            result.ResponseCode = "00";
                            result.ResponseMessage = "Operation successful";
                        }
                    }
                    else
                    {
                        var TransRes = res.Content.ReadAsStringAsync().Result;
                        var errorResult = JsonConvert.DeserializeObject<ErrorResponse>(TransRes);
                        if (errorResult != null)
                        {
                            result.ResponseCode = errorResult.code ?? "99";
                            var rsp = new List<string>();

                            if (!string.IsNullOrEmpty(errorResult.message)) rsp.Add(errorResult.message);
                            if (!string.IsNullOrEmpty(errorResult.details)) rsp.Add(errorResult.details);
                            if (rsp.Count > 0)
                                result.ResponseMessage = String.Join("|", rsp.ToArray());
                            else
                                result.ResponseMessage = "Unknown error message";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.ResponseCode = "99";

                string msg = ex.Message;
                if (ex.InnerException != null) msg = string.Format("{0} - {1}", msg, ex.InnerException.Message);
                result.ResponseMessage = string.Format("{0}|{1}", msg, ex.StackTrace);
                logger.LogError(ex);
            }
            logger.LogResponse(new string[] { result.ResponseCode, result.ResponseMessage });
            return View(result);
        }
    }
}