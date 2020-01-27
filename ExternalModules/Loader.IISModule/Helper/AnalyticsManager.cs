using Loader.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Loader.Helper
{
    public static class AnalyticsManager
    {
        private static readonly string _LoaderURL = "";
        static AnalyticsManager()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
        }

        public static void SendData(string Action, string Details)
        {
            StringBuilder builder = new StringBuilder(Details);

            try
            {
                
                new Thread(() => DoPost("Loader.IISModule", Action, builder.ToString())).Start();
            }
            catch (Exception)
            {
#if DEBUG 
                throw;
#endif

            }



        }

        private static void DoPost(string Category , string ActionName , string Description, int AnalyticsType = 0)
        {

            try
            {
                WebRequest httpWebRequest = WebRequest.Create(ConfigurationManager.LoaderURL + "api/Analytics/Send");

                // Create POST data and convert it to a byte array.  
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    var JsonObject = new { Category = Category, ActionName = ActionName, Description = Description, AnalyticsType = AnalyticsType };
                    string JSONString = JSONSerializer.ToJavaScriptObjectNotation(JsonObject);
                    /* string json = "{" +
                                     "\"Category\":\"" + Category + "\"," +
                                     "\"ActionName\":\"" + ActionName + "\"," +
                                     "\"Description\":\"" + Description + "\"," +
                                     "\"AnalyticsType\":" + AnalyticsType + 

                                   "}";*/

                    streamWriter.Write(JSONString);
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                }
            }
            catch (Exception)
            {
#if DEBUG
                throw;
#endif
            }
            
        }
    }
}
