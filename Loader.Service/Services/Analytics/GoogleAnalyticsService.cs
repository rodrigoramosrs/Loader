using Loader.Domain.Models.Analytics;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Loader.Service.Services.Analytics
{
    public class GoogleAnalyticsService : BaseAnalyticsService
    {
        public GoogleAnalyticsService(string AnalyticsID, string CustomerName, string CustomerID) : base(AnalyticsID, CustomerName, CustomerID) { }

        public override Task<bool> Send(AnalyticsData AnalyticsData)
        {
            var helper = new GoogleAnalyticsHelper(this.AnalyticsID, this.CustomerID);
            var result = helper.TrackEvent(AnalyticsData,CustomerID, CustomerName).Result;//AnalyticsData.Category, AnalyticsData.Name, "{" + $"\"ID\": {CustomerID}" + $", \"Name\": \"{CustomerName}\"" + "}", null).Result;// AnalyticsData.Value).Result;
            if (!result.IsSuccessStatusCode)
            {
                new Exception("something went wrong");
            }

            return Task.FromResult(true);
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

        public async Task<HttpResponseMessage> TrackEvent(AnalyticsData AnalyticsData, string CustomerID, string CustomerName)//string category, string action, string label, string type = "event", string value = null)
        {

            using (var httpClient = new HttpClient())
            {
                var postData = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("v", googleVersion),
                    new KeyValuePair<string, string>("tid", googleTrackingId),
                    new KeyValuePair<string, string>("cid", googleClientId),
                    new KeyValuePair<string, string>("t",  Enum.GetName(typeof(AnalyticsData.eAnalyticsType), AnalyticsData.AnalyticsType).ToString().ToLower()), //
                   
                };
                
                switch (AnalyticsData.AnalyticsType)
                {
                    case AnalyticsData.eAnalyticsType.Event:

                        if (string.IsNullOrEmpty(AnalyticsData.Category))
                            throw new ArgumentNullException(nameof(AnalyticsData.Category));

                        if (string.IsNullOrEmpty(AnalyticsData.Name))
                            throw new ArgumentNullException(nameof(AnalyticsData.Name));

                        postData.Add(new KeyValuePair<string, string>("ec", AnalyticsData.Category));
                        postData.Add(new KeyValuePair<string, string>("ea", AnalyticsData.Name));

                        if (AnalyticsData.Label != null)
                            postData.Add(new KeyValuePair<string, string>("el", AnalyticsData.Label));
                        else
                            postData.Add(new KeyValuePair<string, string>("el", $"Customer {CustomerID} - {CustomerName}"));//"{" + $"\"ID\": {CustomerID}" + $", \"Name\": \"{CustomerName}\"" + "}"));

                        if (AnalyticsData.Value != null)
                            postData.Add(new KeyValuePair<string, string>("ev", AnalyticsData.Value?.ToString()));

                        break;
                    case AnalyticsData.eAnalyticsType.PageView:
                        //&dh=mydemo.com  |  Document hostname.
                        //&dp =/ home     |  Page.
                        //& dt = homepage | Title.
                        postData.Add(new KeyValuePair<string, string>("dh", AnalyticsData.HostName));
                        postData.Add(new KeyValuePair<string, string>("dp", $"{AnalyticsData.PageName}?id={CustomerID}&name={CustomerName}"));
                        string divider = string.IsNullOrEmpty(AnalyticsData.Title) ? "" : " | ";
                        postData.Add(new KeyValuePair<string, string>("dt",  $"{AnalyticsData.Title}{divider}Customer {CustomerID} - {CustomerName}"));
                        break;

                }

                return await httpClient.PostAsync(endpoint, new FormUrlEncodedContent(postData)).ConfigureAwait(false);
            }
        }
    }
}
