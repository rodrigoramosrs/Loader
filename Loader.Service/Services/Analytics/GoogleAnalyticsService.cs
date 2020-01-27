using Hangfire;
using Loader.Domain.Models.Analytics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Loader.Service.Services.Analytics
{
    public class GoogleAnalyticsService : BaseAnalyticsService
    {

        private static List<AnalyticsInformationData> _DataToSend = new List<AnalyticsInformationData>();
        private static object Locker = new object();
       
        public GoogleAnalyticsService(string DefaultAnalyticsID, string ExceptionAnalyticsID, string CustomerName, string CustomerID) 
            : base(DefaultAnalyticsID, ExceptionAnalyticsID, CustomerName, CustomerID) { }

        //public GoogleAnalyticsService(string AnalyticsID, ExceptionAnalyticsID, string CustomerName, string CustomerID) : base(DefaultAnalyticsID, ExceptionAnalyticsID, CustomerName, CustomerID) { }

        public override Task<bool> SendInformation(string ActionName, string Description)//(AnalyticsInformationData AnalyticsData)
        {
            this.ScheduleDataToSend(new AnalyticsInformationData()
            {
                ActionName = ActionName,//ExceptionData.Exception.Message.ToString(),
                //Category = "Loader.Application.AnalyticsException",
                Description = Description
            });
            return Task.FromResult(true);

            var analyticsData = new AnalyticsInformationData()
            {
                ActionName = ActionName,//ExceptionData.Exception.Message.ToString(),
                //Category = "Loader.Application.AnalyticsException",
                Description = Description
            };


            var helper = new GoogleAnalyticsHelper(this.DefaultAnalyticsID, this.CustomerID);
            var result = helper.TrackEvent(analyticsData, CustomerID, CustomerName).Result;//.Result;//AnalyticsData.Category, AnalyticsData.Name, "{" + $"\"ID\": {CustomerID}" + $", \"Name\": \"{CustomerName}\"" + "}", null).Result;// AnalyticsData.Value).Result;
            if (!result)
            {
                new Exception("something went wrong");
            }

            return Task.FromResult(true);
        }

        public override Task<bool> SendException(string ActionName, Exception ex)//(AnalyticsExceptionData ExceptionData)
        {
            this.ScheduleDataToSend(new AnalyticsInformationData()
            {
                ActionName = ActionName,
                Description = base.FormatExceptionMessage(ex, 0)
            });
            return Task.FromResult(true);


            var analyticsData = new AnalyticsInformationData()
            {
                ActionName = ActionName,
                Description = base.FormatExceptionMessage(ex, 0)
            };

            var helper = new GoogleAnalyticsHelper(this.ExceptionAnalyticsID, this.CustomerID);
            var result = helper.TrackEvent(analyticsData, CustomerID, CustomerName).Result;//.Result;//AnalyticsData.Category, AnalyticsData.Name, "{" + $"\"ID\": {CustomerID}" + $", \"Name\": \"{CustomerName}\"" + "}", null).Result;// AnalyticsData.Value).Result;
            if (!result)
            {
                new Exception("something went wrong");
            }

            return Task.FromResult(true);

        }

        public override Task<bool> SendPageView(string HostName, string PageName, string Title = null)
        {
            throw new NotImplementedException();
        }

        private void ScheduleDataToSend(AnalyticsInformationData data)
        {
            lock (Locker)
            {
                _DataToSend.Add(data);
            }
        }

        [DisableConcurrentExecution(10)]
        [AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public override async Task FlushData()
        {
            lock (Locker)
            {
                GoogleAnalyticsHelper analyticsHelper = new GoogleAnalyticsHelper(this.ExceptionAnalyticsID, this.CustomerID);
                foreach (var data in _DataToSend)
                {
                    Task.Run(() => analyticsHelper.TrackEvent(data, CustomerID, CustomerName));
                }
                _DataToSend.Clear();
                GC.Collect();
            }

            //return Task.FromResult(true);
        }
    }

    // More information about API - see https://developers.google.com/analytics/devguides/collection/protocol/v1/devguide
    public class GoogleAnalyticsHelper
    {
        private readonly string endpoint = "https://www.google-analytics.com/collect";
        private readonly string googleVersion = "1";
        private readonly string googleTrackingId; // UA-XXXXXXXXX-XX
        private readonly string googleClientId; // 555 - any user identifier

        public GoogleAnalyticsHelper(string trackingId, string clientId)
        {
            this.googleTrackingId = trackingId;
            this.googleClientId = clientId;
           
        }


        public async /*Task<HttpResponseMessage>*/ Task<bool> TrackEvent(AnalyticsInformationData AnalyticsData, string CustomerID, string CustomerName)//string category, string action, string label, string type = "event", string value = null)
        {

            var postData = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("v", googleVersion),
                    new KeyValuePair<string, string>("tid", googleTrackingId),
                    new KeyValuePair<string, string>("cid", googleClientId),
                    new KeyValuePair<string, string>("t",  Enum.GetName(typeof(AnalyticsType), AnalyticsData.AnalyticsType).ToString().ToLower()), //
                   
                };

            switch (AnalyticsData.AnalyticsType)
            {
                case AnalyticsType.Exception:
                case AnalyticsType.Event:

                    /*if (string.IsNullOrEmpty(AnalyticsData.Category))
                        throw new ArgumentNullException(nameof(AnalyticsData.Category));

                    if (string.IsNullOrEmpty(AnalyticsData.ActionName))
                        throw new ArgumentNullException(nameof(AnalyticsData.ActionName));*/


                    postData.Add(new KeyValuePair<string, string>("ec", $"{CustomerID} - {CustomerName}"));
                    postData.Add(new KeyValuePair<string, string>("ea", AnalyticsData.ActionName));//));

                    postData.Add(new KeyValuePair<string, string>("el", AnalyticsData.Description));
                    /*
                    if (AnalyticsData.Label != null)
                        postData.Add(new KeyValuePair<string, string>("el", AnalyticsData.Label));
                    else
                        postData.Add(new KeyValuePair<string, string>("el", $"Customer {CustomerID} - {CustomerName}"));//"{" + $"\"ID\": {CustomerID}" + $", \"Name\": \"{CustomerName}\"" + "}"));
                        */
                    if (AnalyticsData.Value != null)
                        postData.Add(new KeyValuePair<string, string>("ev", AnalyticsData.Value?.ToString()));

                    /*
                    postData.Add(new KeyValuePair<string, string>("ec", AnalyticsData.Category));
                    postData.Add(new KeyValuePair<string, string>("ea", AnalyticsData.Name));

                    if (AnalyticsData.Label != null)
                        postData.Add(new KeyValuePair<string, string>("el", AnalyticsData.Label));
                    else
                        postData.Add(new KeyValuePair<string, string>("el", $"Customer {CustomerID} - {CustomerName}"));//"{" + $"\"ID\": {CustomerID}" + $", \"Name\": \"{CustomerName}\"" + "}"));

                    if (AnalyticsData.Value != null)
                        postData.Add(new KeyValuePair<string, string>("ev", AnalyticsData.Value?.ToString()));
                        */
                    break;
                case AnalyticsType.PageView:
                    //&dh=mydemo.com  |  Document hostname.
                    //&dp =/ home     |  Page.
                    //& dt = homepage | Title.
                    postData.Add(new KeyValuePair<string, string>("dh", AnalyticsData.HostName));
                    postData.Add(new KeyValuePair<string, string>("dp", $"{AnalyticsData.PageName}?id={CustomerID}&name={CustomerName}"));
                    string divider = string.IsNullOrEmpty(AnalyticsData.Title) ? "" : " | ";
                    postData.Add(new KeyValuePair<string, string>("dt", $"{AnalyticsData.Title}{divider}Customer {CustomerID} - {CustomerName}"));
                    break;

            }

            Task.Run(() => {
                    using (var httpClient = new HttpClient())
                    {
                        var data = httpClient.PostAsync(endpoint, new FormUrlEncodedContent(postData)).Result;
                    }
                });// .ConfigureAwait(false);
            return await Task.FromResult(true);
        }
    }
}
