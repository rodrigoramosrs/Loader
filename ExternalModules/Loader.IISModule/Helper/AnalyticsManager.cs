﻿using Loader.Helper;
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

            DoLogData(Action, Details);

            StringBuilder builder = new StringBuilder(Details);

            try
            {
                
                new Thread(() => DoPost("Loader.IISModule", Action, builder.ToString())).Start();
            }
            catch (Exception ex)
            {
#if DEBUG 
                throw ex;
#else
                LogHelper.WriteErrorLog(ex.ToString());
#endif

            }



        }

        private static void DoLogData(string Action, string Details)
        {
            if (!ConfigurationManager.LogToFile) return;

            string Folder = ConfigurationManager.RootPath + "\\Analytics";
            
            if (!Directory.Exists(Folder))
                Directory.CreateDirectory(Folder);

            File.WriteAllText(Folder + "\\Analytics_Data_" + DateTime.Now.ToString("HHmmssddMMyyyy") + ".log", Action + ":" + Details);

        }

        private static void DoPost(string Category , string ActionName , string Description, int AnalyticsType = 0)
        {

            try
            {
                WebRequest httpWebRequest = WebRequest.Create(ConfigurationManager.LoaderURL + "api/Analytics/Send");

                // Create POST data and convert it to a byte array.  
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                var JsonObject = new { Category = Category, ActionName = ActionName, Description = Description, AnalyticsType = AnalyticsType };
                string JSONString = JSONSerializer.ToJavaScriptObjectNotation(JsonObject);

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    


                    /* string json = "{" +
                                     "\"Category\":\"" + Category + "\"," +
                                     "\"ActionName\":\"" + ActionName + "\"," +
                                     "\"Description\":\"" + Description + "\"," +
                                     "\"AnalyticsType\":" + AnalyticsType + 

                                   "}";*/
                    streamWriter.Write(JSONString);
                    Debugger.Write("AnalyticsManager.DoPost() Posted to " + ConfigurationManager.LoaderURL + "api/Analytics/Send | Data: " + JSONString);

                }
                try
                {
                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                    Debugger.Write("AnalyticsManager.DoPost() ex1 " + ex.ToString());
                    LogHelper.WriteErrorLog(ex.ToString());
                }

            }
            catch (Exception ex)
            {
#if DEBUG
                throw ex;
#else
                Debugger.Write("AnalyticsManager.DoPost() ex2 " + ex.ToString());
                LogHelper.WriteErrorLog(ex.ToString());
#endif
            }

        }
    }
}
