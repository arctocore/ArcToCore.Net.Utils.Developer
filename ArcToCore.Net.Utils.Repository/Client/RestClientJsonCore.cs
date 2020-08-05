using ArcToCore.Net.Utils.Core.Interface;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Net;
using System.Threading.Tasks;

namespace ArcToCore.Net.Utils.Repository.Client
{
    
    public class RestClientJsonCore : IRestClientJsonCore
    {
        #region public

        public JToken RestClientCore(string url)
        {
            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);
            var response = new RestResponse();
            Task.Run(async () =>
            {
                response = await GetResponseContentAsync(client, request) as RestResponse;
            }).Wait();
            JToken objects = null;
            try
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return response.StatusDescription + ". Please try again!";
                }

                

                objects = JToken.Parse(response.Content);
            }
            catch
            {
                throw new Exception(response.StatusDescription + ". Please try again!");
            }

            return objects;
        }

        public JToken RestContent(string content)
        {
            try
            {
                JToken objects = JToken.Parse(content);

                return objects;
            }
            catch(Exception ex)
            {
                return "Error: \n" + ex.Message + "\n.Please try again!";
            }
        }

        #endregion public

        #region private

        private Task<IRestResponse> GetResponseContentAsync(RestClient restClient, RestRequest restRequest)
        {
            var tcs = new TaskCompletionSource<IRestResponse>();
            restClient.ExecuteAsync(restRequest, response =>
            {
                tcs.SetResult(response);
            });
            return tcs.Task;
        }

        #endregion private
    }
}