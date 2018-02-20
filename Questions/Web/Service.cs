using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Questions.Web
{
    public class Service
    {
        public class Response
        {
            public HttpStatusCode StstusCode { get; set; }
            public string Message { get; set; }
        }
        //public static T ToObject<T>(this string json) => JsonConvert.DeserializeObject<T>(json, Converter.Settings);
        //public static string ToJson(this Proxy self) => JsonConvert.SerializeObject(self, Converter.Settings);
        public static Response TestExecutePostRequest(string uri, string apiPath, string body)
        {
            var client = new RestClient(uri);

            client.Proxy = System.Net.HttpWebRequest.GetSystemWebProxy();
            client.Proxy.Credentials = CredentialCache.DefaultCredentials;

            // Get the proxy summary
            var request = new RestRequest(apiPath, Method.POST);
            //request.AddHeader("Authorization", $"Bearer {accessToken}");
            request.AddHeader("apiKey", "WcIoXV5uzPsZUGEXvsqqLhJMmHYDKc6l");
            request.AddHeader("Content-Type","application/json");
            //request.AddJsonBody(body);
            request.AddParameter("text/json", body, ParameterType.RequestBody);
            //request.AddBody(body);
            try
            {
                //var cancellationTokenSource = new CancellationTokenSource();
                IRestResponse response3 = client.Execute(request);

                return new Response() { StstusCode = response3.StatusCode, Message = response3.Content };
            }
            catch (Exception ex)
            {
                return new Response() { StstusCode = HttpStatusCode.Ambiguous, Message = ex.Message };
            }
        }
    }
}
