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
    public class MVAnalyticsService : BaseAnalyticsService
    {

        private static List<AnalyticsInformationData> _DataToSend = new List<AnalyticsInformationData>();
        private static object Locker = new object();
       
        public MVAnalyticsService(string DefaultAnalyticsID, string ExceptionAnalyticsID, string CustomerName, string CustomerID, bool SaveAnalyticsToFile) 
            : base(DefaultAnalyticsID, ExceptionAnalyticsID, CustomerName, CustomerID, SaveAnalyticsToFile) { }

        //public GoogleAnalyticsService(string AnalyticsID, ExceptionAnalyticsID, string CustomerName, string CustomerID) : base(DefaultAnalyticsID, ExceptionAnalyticsID, CustomerName, CustomerID) { }

        public override Task<bool> SendInformation(string ActionName, string Description)//(AnalyticsInformationData AnalyticsData)
        {
            

            this.ScheduleDataToSend(new AnalyticsInformationData()
            {
                ActionName = ActionName,//ExceptionData.Exception.Message.ToString(),
                //Category = "Loader.Application.AnalyticsException",
                Description = Description
            });

            this.SaveDataToFile("INFO","Action:" + ActionName + "\r\nDescription:" + Description);
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
                MVAnalyticsHelper analyticsHelper = new MVAnalyticsHelper();
                foreach (var data in _DataToSend)
                {
                    Task.Run(() => analyticsHelper.TrackEvent(data));
                }
                _DataToSend.Clear();
                GC.Collect();
            }

            //return Task.FromResult(true);
        }
    }

    public class MVAnalyticsHelper
    {

        public MVAnalyticsHelper()
        {
           
        }


        public async /*Task<HttpResponseMessage>*/ Task<bool> TrackEvent(AnalyticsInformationData AnalyticsData)//string category, string action, string label, string type = "event", string value = null)
        {

            Task.Run(() => {
                   //Aqui vai a insert no banco
                });// .ConfigureAwait(false);
            return await Task.FromResult(true);
        }
    }
}
