using Dapper;
using Newtonsoft.Json;
using PermissionManagement.Model;
using PermissionManagement.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PermissionManagement.Repository.Implementation
{
    class PickUpRepository : ICashPickup
    {
        private readonly IDbConnection context;
        private readonly DapperContext dapperContext;

        public PickUpRepository(DapperContext dbContext)
        {
            dapperContext = dbContext;
            context = dbContext.Connection;
        }


        public async Task<RemittanceCashPickup> RetrieveReference(string referenceNumber)
        {
            var RetrieveRef = new RemitlyResponseObject();

            try
            {
                using (var client = new HttpClient())
                {
                    string url = ConfigurationManager.AppSettings["RemitlyBaseURL"];
                    client.BaseAddress = new Uri(url);
                    HttpResponseMessage res = await client.GetAsync($"/IMTOAPI.Project/Remitly/GetTransfer/ " + referenceNumber);
                    if (res.IsSuccessStatusCode)
                    {
                        var RetRef = res.Content.ReadAsStringAsync().Result;
                        var resp = JsonConvert.DeserializeObject<RemittanceCashPickup>(RetRef);

                        if (resp != null)
                        {
                            RetrieveRef.ResponseCode = "00";
                            RetrieveRef.ResponseMessage = "Operation successful";

                            RetrieveRef.ResponseResult = resp;
                        }
                    }
                    else
                    {
                        var TransRes = res.Content.ReadAsStringAsync().Result;
                        var errorResult = JsonConvert.DeserializeObject<RemitlyErrorResult>(TransRes);
                        if (errorResult != null)
                        {
                            RetrieveRef.ResponseCode = errorResult.code ?? "99";
                            RetrieveRef.ResponseMessage = "Unknown error message";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                RetrieveRef.ResponseCode = "99";
            }

            return RetrieveRef.ResponseResult;
        }

        public async Task<RemitlyEditTransfer> EditRemittance(string referenceNumber)
        {
            var result = new RemitlyEditTransfer();
            try
            {
                string actionMessage = Encoding.UTF8.GetString(Convert.FromBase64String(referenceNumber));

                // Convert serialized update object to string content.
                var httpContent = new StringContent(actionMessage, Encoding.UTF8, "application/json");
                using (var client = new HttpClient())
                {
                    string url = ConfigurationManager.AppSettings["RemitlyBaseURL"];
                    client.BaseAddress = new Uri(url);
                    HttpResponseMessage EditRes = await client.PostAsync($"/IMTOAPI.Project/Remitly/UpdateTransfer/", httpContent);

                    //Checking the response
                    if (EditRes.IsSuccessStatusCode)
                    {
                        var TransResult = EditRes.Content.ReadAsStringAsync().Result;
                        result = JsonConvert.DeserializeObject<RemitlyEditTransfer>(TransResult);
                        if (result != null)
                        {
                            result.ResponseCode = "00";
                            result.ResponseMessage = "Operation successful";
                        }
                    }
                    else
                    {
                        var TransResult = EditRes.Content.ReadAsStringAsync().Result;
                        var errorResult = JsonConvert.DeserializeObject<RemitlyErrorResult>(TransResult);
                        if (errorResult != null)
                        {
                            result.ResponseCode = errorResult.code ?? "99";
                            result.ResponseMessage = "Unknown error message";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.ResponseCode = "99";
            }
            return result;
        }


        public async Task<RemitlyListResponse> ListCashPickup()
        {
            var ListResult = new RemitlyListResponse();

            try
            {
                using (var client = new HttpClient())
                {
                    string url = ConfigurationManager.AppSettings["RemitlyBaseURL"];
                    client.BaseAddress = new Uri(url);
                    HttpResponseMessage res = await client.GetAsync($"/IMTOAPI.Project/ Remitly/listTransfer");
                    if (res.IsSuccessStatusCode)
                    {
                        var TransRes = res.Content.ReadAsStringAsync().Result;
                        RemitlyTransferHeader[] list = null;
                        list = JsonConvert.DeserializeObject<RemitlyTransferHeader[]>(TransRes);

                        if (list != null)
                        {
                            ListResult.TransferList = list;
                            ListResult.ResponseCode = "00";
                            ListResult.ResponseMessage = "Operation successful";
                        }
                    }
                    else
                    {
                        var TransRes = res.Content.ReadAsStringAsync().Result;
                        var errorResult = JsonConvert.DeserializeObject<RemitlyErrorResult>(TransRes);
                        if (errorResult != null)
                        {
                            ListResult.ResponseCode = errorResult.code ?? "99";
                            ListResult.ResponseMessage = "Unknown error message";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ListResult.ResponseCode = "99";
            }

            return ListResult;
        }

    }
}
