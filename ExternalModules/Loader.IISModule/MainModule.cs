using Loader.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web;


namespace Loader.IISModule
{
    public class MainModule : IHttpModule
    {
        Stopwatch timer;

        
        //TraceSource tsStatus;
        public void Init(HttpApplication application)
        {
            application.EndRequest += Application_EndRequest;
            application.BeginRequest += Application_BeginRequest;
            timer = new Stopwatch();
        }

        private void Application_BeginRequest(object sender, EventArgs e)
        {
            timer.Reset();
            timer.Start();
            var context = (sender as HttpApplication).Context;

           

            /*try
            {
                this.SendStatistic("Application_BeginRequest", ((HttpApplication)sender).Context);
            }
            catch (Exception ex)
            {
                this.SendStatistic("Application_BeginRequest.Exception\r\n" + ex, null); 
            }*/

                //HttpApplication application = (HttpApplication)sender;
                //HttpContext context = application.Context;


                /*
                tsStatus.TraceEvent(TraceEventType.Start, 0, "[REQDATA MODULE] START EndRequest");

                // start writing out the request data

                context.Response.Write("<hr>");
                context.Response.Write("<b><font size=2 color=green>REQUEST HEADERS</font></b><br>");
                context.Response.Write("<font size=2>");
                context.Response.Write("METHOD : " + context.Request.HttpMethod + "<br>");
                context.Response.Write("URL : " + context.Request.Url + "<br>");
                context.Response.Write("QUERYSTRING : " + context.Request.QueryString + "<br>");
                context.Response.Write("</font><br>");

                tsStatus.TraceEvent(TraceEventType.Verbose, 0, "[REQDATA MODULE] done with Req Data, moving onto Response");

                // now response data

                context.Response.Write("<b><font size=2 color=blue>RESPONSE HEADERS</font></b><br>");
                context.Response.Write("<font size=2>");
                context.Response.Write("STATUS CODE : " + context.Response.StatusCode.ToString() + "." + context.Response.SubStatusCode.ToString() + "<br>");
                context.Response.Write("CONTENT TYPE : " + context.Response.ContentType.ToString() + "<br>");
                context.Response.Write("EXPIRES : " + context.Response.Expires.ToString() + "<br>");
                context.Response.Write("</font><br>");

                if (context.Response.StatusCode > 399)
                {
                    tsStatus.TraceEvent(TraceEventType.Warning, 0, "[REQDATA MODULE] error status code detected");
                }

                tsStatus.TraceEvent(TraceEventType.Verbose, 0, "[REQDATA MODULE] done with Response Data");

                // set cache policy on response so it's not cached.

                context.Response.DisableKernelCache();
                tsStatus.TraceEvent(TraceEventType.Verbose, 0, "[REQDATA MODULE] cache setting is (" + context.Response.Cache.ToString() + ")");

                tsStatus.TraceEvent(TraceEventType.Stop, 0, "[REQDATA MODULE] STOP - EndRequest");*/
        }

        private void Application_EndRequest(object sender, EventArgs e)
        {
            timer.Stop();
            this.SendStatistic((sender as HttpApplication).Context);
        }




        List<string> exclusionList = new List<string>() { ".jpg", ".jpeg", ".png", ".bmp", ".javascript", ".js", ".png", ".css", ".ico", "chatserver.svc" };
        private void SendStatistic(HttpContext context)
        {
            try
            {
                foreach (var exclusion in exclusionList)
                    if (context.Request.Path.ToLower().Contains(exclusion)) return;


                StringBuilder builder = new StringBuilder();
                string PoolName = context.Request.ServerVariables["APP_POOL_ID"];
                string Path = context.Request.Path;
                
                builder.AppendLine("Alepsed: " + this.ConvertToTimeString(timer.ElapsedMilliseconds));
                builder.AppendLine("Method: " + context.Request.HttpMethod);

                if(!string.IsNullOrEmpty(context.Request.QueryString.ToString()))
                    builder.AppendLine("Query: " + context.Request.QueryString);

                AnalyticsManager.SendData(PoolName + " - " + context.Request.UserHostAddress + "[./" + Path + "]", builder.ToString());
            }
            catch (Exception)
            {
#if DEBUG 
                throw;
#endif

            }


        }

        private string ConvertToTimeString(long AlepsedTimeMiliseconds)
        {
            var timeSpan = TimeSpan.FromMilliseconds(AlepsedTimeMiliseconds);
            return string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                        timeSpan.Hours,
                        timeSpan.Minutes,
                        timeSpan.Seconds,
                        timeSpan.Milliseconds);
        }

        public void Dispose()
        {
        }
    }
}
