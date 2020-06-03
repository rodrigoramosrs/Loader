using Loader.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace Loader.IISModule
{
    public class MainModule : IHttpModule
    {
        Stopwatch timer;

        
        //TraceSource tsStatus;
        public void Init(HttpApplication application)
        {
            Helper.Debugger.Write("MainModule.Init");
            application.EndRequest += Application_EndRequest;
            application.BeginRequest += Application_BeginRequest;
            timer = new Stopwatch();
        }

        private void Application_BeginRequest(object sender, EventArgs e)
        {
            Helper.Debugger.Write("MainModule.Application_BeginRequest");
            timer.Reset();
            timer.Start();
            var context = (sender as HttpApplication).Context;
        }

        private void Application_EndRequest(object sender, EventArgs e)
        {
            
            try
            {
                timer.Stop();
                this.SendStatistic((sender as HttpApplication).Context);
                Helper.Debugger.Write("MainModule.Application_EndRequest");
            }
            catch (Exception ex)
            {
                Helper.Debugger.Write("MainModule.Application_EndRequest.Exception - " + ex.ToString());
                LogHelper.WriteErrorLog(ex.ToString());
            }
            
        }


        List<string> exclusionList = new List<string>() { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".javascript", ".js", ".png", ".css", ".ico", "chatserver.svc", ".axd" };
        
        private void SendStatistic(HttpContext context)
        {
            Helper.Debugger.Write("MainModule.SendStatistic - Aleped " + timer.ElapsedMilliseconds + " | Required " + ConfigurationManager.LogRequestTimeAt);
            if (ConfigurationManager.LogRequestTimeAt > timer.ElapsedMilliseconds) return;

            foreach (var exclusion in exclusionList)
                if (context.Request.Path.ToLower().Contains(exclusion)) return;

            try
            {
                bool IsSlowRequest = timer.ElapsedMilliseconds > ConfigurationManager.ValidSlowDownRequestAt;

                StringBuilder builder = new StringBuilder();
                string PoolName = context.Request.ServerVariables["APP_POOL_ID"];
                string Path = string.Empty;
                string SiteName = HostingEnvironment.ApplicationHost.GetSiteName();
                

                builder.AppendLine("Alepsed: " + this.ConvertToTimeString(timer.ElapsedMilliseconds) + (IsSlowRequest ? " [SLOW]" : ""));

                //builder.AppendLine("Method: " + context.Request.HttpMethod);
                builder.AppendLine("PoolName: " + PoolName);
                
                if (!string.IsNullOrEmpty(context.Request.ServerVariables["HTTP_SOAPACTION"]))
                {
                    string soapAction = context.Request.ServerVariables["HTTP_SOAPACTION"].Replace("http://tempuri.org/", string.Empty).Replace("\"",string.Empty);
                    Path = context.Request.Path + "/" + soapAction;
                    builder.AppendLine("Action: " + soapAction + "["+ context.Request.HttpMethod + "]");
                }
                else
                {
                    Path = context.Request.Path + "[" + context.Request.HttpMethod + "]";
                }

                if (!string.IsNullOrEmpty(context.Request.QueryString.ToString()))
                    builder.AppendLine("Query: " + context.Request.QueryString);

                builder.AppendLine("Client: " + context.Request.UserHostAddress);

                AnalyticsManager.SendData("[" + SiteName + "] [./" + Path + "]", builder.ToString());


            }
            catch (Exception ex)
            {
                
#if DEBUG
                throw;
#else
                LogHelper.WriteErrorLog(ex.ToString());
#endif


            }


        }

        private string ConvertToTimeString(long AlepsedTimeMiliseconds)
        {
            var timeSpan = TimeSpan.FromMilliseconds(AlepsedTimeMiliseconds);
            return string.Format("{0:D2}m:{1:D2}s",
                        //timeSpan.Hours,
                        timeSpan.Minutes,
                        timeSpan.Seconds);
            /*return string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                        timeSpan.Hours,
                        timeSpan.Minutes,
                        timeSpan.Seconds,
                        0);*/
            //timeSpan.Milliseconds);
        }

        public void Dispose()
        {
        }

       
    }
}
